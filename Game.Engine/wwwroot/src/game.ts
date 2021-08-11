import { NetBody, NetWorldView, NetGroup } from "./game_generated";
import * as PIXI from "pixi.js";
import { Layer, Group } from "@pixi/layers";
import * as background from "./background";
import { Border } from "./border";
import { Overlay } from "./overlay";
import { setContainer, bodyFromServer, ClientBody, update as updateCache, tick as cacheTick } from "./cache";
import { projectObject } from "./interpolator";
import { update as leaderboardUpdate } from "./leaderboard";
import { Minimap } from "./minimap";
import { setPerf, setPlayerCount, setSpectatorCount } from "./hud";
import * as log from "./log";
import { Controls, initializeWorld, registerCanvas, setCurrentWorld } from "./controls";
import { message } from "./chat";
import { Connection } from "./connection";
import { getToken } from "./discord";
import { Settings } from "./settings";
import { Events } from "./events";
import { refreshList, joinWorld, firstLoad } from "./lobby";
import { Vector2 } from "./Vector2";
import { CustomContainer } from "./CustomContainer";
import { load } from "./loader";
import "./hintbox";
import bus from "./bus";

const size = { width: 1000, height: 500 };
const canvas = document.getElementById("gameCanvas") as HTMLCanvasElement;

declare global {
    interface Window {
        magic: string | undefined;
        webkitAudioContext: typeof window.AudioContext;
    }
}
const actx = new (window.AudioContext || window.webkitAudioContext)();

function beep() {
    const duration = 0.1;
    const v = actx.createOscillator();
    const u = actx.createGain();
    v.connect(u);
    v.frequency.value = 1200;
    v.type = "square";
    u.connect(actx.destination);
    u.gain.value = 0.1;
    u.gain.setValueAtTime(0.1, actx.currentTime + duration);
    u.gain.linearRampToValueAtTime(0, actx.currentTime + duration + 0.05);
    v.start(actx.currentTime);
    v.stop(actx.currentTime + duration + 0.05);
}

const spawnButton = document.getElementById("spawn") as HTMLButtonElement;
const buttonSpectate = document.getElementById("spawnSpectate") as HTMLButtonElement;
const progress = document.getElementById("cooldown") as HTMLProgressElement;
const spawnscreen = document.querySelector(".spawnscreen") as HTMLElement;
const leaderboardRight = document.querySelector("#leaderboard") as HTMLElement;
const leaderboardLeft = document.querySelector("#leaderboard-left") as HTMLElement;
const leaderboardCenter = document.querySelector("#leaderboard-center") as HTMLElement;
const ctfArea = document.querySelector("#ctf_arena") as HTMLElement;
const statsEl = document.querySelector(".stats") as HTMLElement;
const logEl = document.querySelector("#log") as HTMLElement;
const spectateControls = document.querySelector("#spectatecontrols") as HTMLElement;
const button = document.getElementById("spawn") as HTMLButtonElement;
const connection = new Connection(onView);

let camera: ClientBody;
let time: number;
let isAlive: boolean;
let serverTimeOffset: number | undefined;
let gameTime: number;
let lastPosition: PIXI.Point = new Vector2(0, 0);
let worldSize = 1000;
let fleetID = 0;
let frameCounter = 0;
let viewCounter = 0;
let updateCounter = 0;
let lastCamera = new Vector2(0, 0);
let isSpectating = false;
let aliveSince: number;
let joiningWorld = false;
let spawnOnView = false;

const app = new PIXI.Application({ view: canvas, transparent: true, resolution: window.devicePixelRatio || 1 });

app.stage.sortableChildren = true;
const container = new CustomContainer();
app.stage.addChild(container);
app.stage.sortChildren();

const border = new Border(container);
container.plotly = document.getElementById("plotly");
const overlay = new Overlay(container, canvas, container.plotly);
const minimap = new Minimap(app.stage, size);
background.setContainer(container.backgroundGroup);
registerCanvas(canvas);


bus.on("dead", () => {
    document.body.classList.remove("alive");
    document.body.classList.add("spectating");
    document.body.classList.add("dead");
    if (connection.hook.AllowedColors) setCurrentWorld().allowedColors = connection.hook.AllowedColors;
    initializeWorld();
    isSpectating = true;

    Events.Death((gameTime - aliveSince) / 1000);
});

function onView(newView: NetWorldView) {
    viewCounter++;

    time = newView.time();
    isAlive = newView.isAlive();
    fleetID = newView.fleetID();

    if (!isAlive && connection.hook != null) {
        buttonSpectate.disabled = spawnButton.disabled = connection.hook.CanSpawn === false;
    }

    if (isAlive)
        isSpectating = false;

    const lastOffset = time + connection.latency / 2 - performance.now();
    if (!serverTimeOffset) serverTimeOffset = lastOffset;
    serverTimeOffset = 0.95 * serverTimeOffset + 0.05 * lastOffset;

    const updatesLength = newView.updatesLength();
    const updates: NetBody[] = [];
    for (let u = 0; u < updatesLength; u++) updates.push(newView.updates(u)!);

    const groupsLength = newView.groupsLength();
    const groups: NetGroup[] = [];
    for (let u = 0; u < groupsLength; u++) groups.push(newView.groups(u)!);

    const announcementsLength = newView.announcementsLength();
    for (let u = 0; u < announcementsLength; u++) {
        const announcement = newView.announcements(u)!;

        if (announcement.type() == "join") {
            const worldKey = announcement.text()!;

            if (!joiningWorld) {
                joiningWorld = true;
                console.log("received join: " + worldKey);
                joinWorld(worldKey);
            }
        } else {
            let extra = announcement.extraData();

            if (extra) extra = JSON.parse(extra);
            if (announcement.type() == "announce" && announcement.text() == "Launch now to join the next game!") {
                actx.resume();
                beep();
            }
            log.addEntry({
                type: announcement.type()!,
                text: announcement.text()!,
                pointsDelta: announcement.pointsDelta(),
                extraData: extra,
            });
        }
    }

    updateCounter += updatesLength;

    const deletes: number[] = [];
    const deletesLength = newView.deletesLength();
    for (let d = 0; d < deletesLength; d++) deletes.push(newView.deletes(d)!);

    const groupDeletes: number[] = [];
    const groupDeletesLength = newView.groupDeletesLength();
    for (let d = 0; d < groupDeletesLength; d++) groupDeletes.push(newView.groupDeletes(d)!);

    updateCache(updates, deletes, groups, groupDeletes, gameTime, fleetID);
    if (overlay) overlay.update(newView.customData());

    setPlayerCount(newView.playerCount());
    setSpectatorCount(newView.spectatorCount());

    if (border && newView.worldSize() != border.worldSize) {
        worldSize = newView.worldSize();
        border.updateWorldSize(newView.worldSize());
    }

    progress.value = newView.cooldownShoot();

    camera = bodyFromServer(newView.camera()!);

    if (spawnOnView) {
        spawnOnView = false;
        doSpawn();
    }
}
function doSpawn() {
    if ("ontouchstart" in document.documentElement) {
        window.scrollTo(0, 0);
        try {
            document.documentElement.requestFullscreen();
        } catch (e) {
            console.log("Fullscreen failed", e);
        }
    }
    Events.Spawn();
    document.body.classList.remove("dead");
    document.body.classList.remove("spectating");
    document.body.classList.add("alive");
    aliveSince = gameTime;
    connection.sendSpawn(Controls.emoji + Controls.nick, Controls.color, Controls.ship, getToken());
}
spawnButton.addEventListener("click", doSpawn);
buttonSpectate.addEventListener("click", doSpawn);

function startSpectate(hideButton = false) {
    isSpectating = true;
    Events.Spectate();
    document.body.classList.add("spectating");
    document.body.classList.add("dead");

    if (hideButton) {
        document.body.classList.add("spectate_only");
    }
}

document.getElementById("spectate")!.addEventListener("click", () => startSpectate());

function stopSpectate() {
    isSpectating = false;
    document.body.classList.remove("spectating");
    document.body.classList.remove("spectate_only");
}

const deathScreen = document.getElementById("deathScreen")!;
document.getElementById("stop_spectating")!.addEventListener("click", () => {
    stopSpectate();
    deathScreen.style.display = "";
});

document.addEventListener("keydown", ({ code }) => {
    if (code == "Escape") {
        if (document.body.classList.contains("alive")) {
            connection.sendExit();
        } else if (isSpectating && !deathScreen.style.display) {
            stopSpectate();
        } else {
            deathScreen.style.display = "";
            startSpectate();
        }
    }
});

function doPing() {
    connection.framesPerSecond = frameCounter;
    connection.viewsPerSecond = viewCounter;
    connection.updatesPerSecond = updateCounter;
    setPerf(connection.latency, frameCounter);

    frameCounter = 0;
    viewCounter = 0;
    updateCounter = 0;
}

doPing();
setInterval(doPing, 1000);

const query = new URLSearchParams(window.location.search);
if (query.has("spectate") && query.get("spectate") !== "0") {
    startSpectate(true);
}

// clicking enter in nick causes fleet spawn
const nick = document.getElementById("nick") as HTMLInputElement;
nick.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
        Controls.nick = nick.value;
        if (connection.hook.CanSpawn) doSpawn();
    }
});

// clicking enter in spectate mode causes fleet spawn
document.body.addEventListener("keydown", (e) => {
    if (document.body.classList.contains("spectating") && e.key === "Enter") {
        doSpawn();
    }
});

const worlds = document.getElementById("worldsWrapper")!;
document.getElementById("wcancel")!.addEventListener("click", () => {
    worlds.classList.add("closed");
});

bus.on("worldjoin", (worldKey, world) => {
    console.log(`onWorldJoin: ${worldKey} ${world}`);
    if (joiningWorld) {
        joiningWorld = false;
        spawnOnView = true;
    }

    setCurrentWorld(world);
    loadImages.then(() => initializeWorld(world));
    connection.disconnect();
    connection.connect(worldKey);
    serverTimeOffset = undefined;
});

const sizeCanvas = () => {
    let width: number;
    let height: number;
    if ((window.innerWidth * 9) / 16 < window.innerHeight) {
        width = window.innerWidth;
        height = (width * 9) / 16;
    } else {
        height = window.innerHeight;
        width = (height * 16) / 9;
    }

    size.width = Math.floor(width);
    size.height = Math.floor(height);

    const scale = `scale(${Math.min(width / 1440, 1)})`;
    [spawnscreen, leaderboardRight, statsEl, logEl, leaderboardLeft, ctfArea].forEach((x) => {
        x.style.transform = scale;
    });
    leaderboardCenter.style.transform = `translate(-50%, 0) ${scale}`;
    spectateControls.style.transform = `translate(-50%, 0) ${scale}`;

    leaderboardRight.style.transformOrigin = `top right`;
    statsEl.style.transformOrigin = `bottom left`;
    logEl.style.transformOrigin = `bottom left`;
    leaderboardCenter.style.transformOrigin = `top center`;
    leaderboardLeft.style.transformOrigin = `top left`;
    ctfArea.style.transformOrigin = `top center`;
    spectateControls.style.transformOrigin = `bottom center`;

    minimap.size(size);

    app.renderer.resize(width, height);
    app.renderer.resolution = window.devicePixelRatio || 1;
    container.scale.set(width / 5500, width / 5500);
};

sizeCanvas();

window.addEventListener("resize", () => {
    sizeCanvas();
});

let angle = 0.0;
let aimTarget = new Vector2(0, 0);

let lastControl: { angle?: number; aimTarget?: PIXI.Point; boost?: boolean; shoot?: boolean; autofire?: boolean; chat?: string } = {
    angle: undefined,
    aimTarget: undefined,
    boost: undefined,
    autofire: undefined,
    shoot: undefined,
    chat: undefined,
};

const loadImages = load();
loadImages.then(() => {
    setContainer(container);
    document.querySelector(".loading")!.classList.remove("loading");

    bus.emit("loaded");

    refreshList(true).then(firstLoad);
    setInterval(refreshList, 1000);

    bus.on("leaderboard", (lb) => {
        leaderboardUpdate(lb, lastPosition, fleetID);
        minimap.update(lb, worldSize, fleetID);
    });

    // Game Loop
    app.ticker.add(() => {
        if (Controls.mouseX) {
            const pos = container.toLocal(new Vector2(Controls.mouseX, Controls.mouseY));
            angle = Controls.angle;
        aimTarget = new Vector2(Settings.mouseScale * (pos.x - lastPosition.x), Settings.mouseScale * (pos.y - lastPosition.y));
        }

        if (
            angle !== lastControl.angle ||
            aimTarget.x !== aimTarget.x ||
            aimTarget.y !== aimTarget.y ||
            Controls.boost !== lastControl.boost ||
            Controls.shoot !== lastControl.shoot ||
            Controls.autofire !== lastControl.autofire ||
            message.txt !== lastControl.chat
        ) {
            let spectateControl = "";
            if (isSpectating) {
                if (Controls.shoot && !lastControl.shoot) spectateControl = "action:next";
                else spectateControl = "spectating";
            }

            let customData = Controls.customData;

            if (message.time + 3000 > Date.now()) customData = JSON.stringify({ chat: message.txt });
            if (window.magic) {
                customData = JSON.stringify(Object.assign(JSON.parse(customData || "{}"), { magic: window.magic }));
                window.magic = undefined;
            }

            connection.sendControl(angle, Controls.boost, Controls.shoot || Controls.autofire, aimTarget.x, aimTarget.y, spectateControl, customData);

            lastControl = {
                angle,
                aimTarget,
                boost: Controls.boost,
                shoot: Controls.shoot,
                autofire: Controls.autofire,
                chat: message.txt,
            };
        }

        frameCounter++;

        if (serverTimeOffset) {
            const position = new Vector2(0, 0);
            gameTime = performance.now() + serverTimeOffset;

            projectObject(camera, gameTime);
            position.x = Math.floor(camera.Position.x * 0.2 + lastCamera.x * 0.8);
            position.y = Math.floor(camera.Position.y * 0.2 + lastCamera.y * 0.8);
            lastCamera = position;

            container.pivot.x = Math.floor(position.x - 5500 / 2);
            container.pivot.y = Math.floor(position.y - (5500 / 2) * (9 / 16));
            container.position.x = Math.floor(container.position.x);
            container.position.y = Math.floor(container.position.y);

            cacheTick(gameTime);

            background.updateFocus(new Vector2(position.x, position.y));
            background.draw();

            lastPosition = position;
        }
    });
});
