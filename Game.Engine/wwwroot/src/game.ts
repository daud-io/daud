import { NetBody, NetWorldView, NetGroup } from "./game_generated";
import * as background from "./background";
import { Border } from "./border";
import { setContainer, bodyFromServer, ClientBody, update as updateCache, tick as cacheTick } from "./cache";
import { projectObject } from "./interpolator";
import { update as leaderboardUpdate } from "./leaderboard";
import { Minimap } from "./minimap";
import { setPerf, setPlayerCount, setSpectatorCount } from "./hud";
import * as log from "./log";
import { Controls, initializeWorld, registerContainer, setCurrentWorld } from "./controls";
import { ChatOverlay, message } from "./chat";
import { Connection } from "./connection";
import { getToken } from "./discord";
import { Settings } from "./settings";
import { Events } from "./events";
import { refreshList, joinWorld, firstLoad } from "./lobby";
import { CustomContainer } from "./CustomContainer";
import { load } from "./loader";
import "./hintbox";
import bus from "./bus";

import { Vector2, Vector3 } from "@babylonjs/core";

const size = { width: 1000, height: 500 };
const canvas = document.getElementById("gameCanvas") as HTMLCanvasElement;

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

let cameraPositionFromServer: ClientBody;
let time: number;
let isAlive: boolean;
let serverTimeOffset: number | undefined;
let gameTime: number;
let lastPosition: Vector2 = new Vector2(0, 0);
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



const container = new CustomContainer(canvas as HTMLCanvasElement);

const border = new Border(container);
const minimap = new Minimap(container);
const chat = new ChatOverlay(container)

background.setContainer(container);
registerContainer(container);

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

    let offset = Settings.latencyOffset;
    if (Settings.latencyMode == "server")
        offset += -connection.latency/2;
    
    let lastOffset = time + (offset) - performance.now();

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

    setPlayerCount(newView.playerCount());
    setSpectatorCount(newView.spectatorCount());

    if (border && newView.worldSize() != border.worldSize) {
        worldSize = newView.worldSize();
        border.updateWorldSize(newView.worldSize());
    }

    progress.value = newView.cooldownShoot();

    cameraPositionFromServer = bodyFromServer(newView.camera()!);

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

    container.resize();
    minimap.resize();
};

sizeCanvas();

window.addEventListener("resize", () => {
    sizeCanvas();
});

let lastControl: { mouseX: number, mouseY: number; boost?: boolean; shoot?: boolean; autofire?: boolean; chat?: string } = {
    mouseX: 0,
    mouseY: 0,
    boost: undefined,
    autofire: undefined,
    shoot: undefined,
    chat: undefined,
};

const loadImages = load(container);
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
    container.engine.runRenderLoop(() => {
        frameCounter++;
        container.scene.render()
        
        if (serverTimeOffset) {
            gameTime = performance.now() + serverTimeOffset;

            let spectateControl = "";
            if (isSpectating) {
                if (Controls.shoot && !lastControl.shoot) spectateControl = "action:next";
                else spectateControl = "spectating";
            }

            //let customData = ;
            connection.sendControl(
                Controls.boost, 
                Controls.shoot || Controls.autofire, 
                Controls.mouseX,
                Controls.mouseY,
                spectateControl, 
                Controls.customData
            );

            projectObject(cameraPositionFromServer, gameTime);
            container.PositionCamera(cameraPositionFromServer.Position);
            cacheTick(gameTime);
        }
    });
});
