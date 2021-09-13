import "./hintbox";
import { NetWorldView } from './daud-net/net-world-view'
import { ClientBody } from "./cache";
import { projectObject } from "./interpolator";
import { Minimap } from "./minimap";
import { setPerf, setPlayerCount, setSpectatorCount } from "./hud";
import * as log from "./log";
import { Controls, initializeWorld, updateControlAim, registerContainer, setCurrentWorld } from "./controls";
import { Connection } from "./connection";
import { getToken } from "./discord";
import { firstLoad, joinWorld } from "./lobby";
import { GameContainer } from "./gameContainer";
import bus from "./bus";
import { Cache } from "./cache";

const spawnButton = document.getElementById("spawn") as HTMLButtonElement;
const buttonSpectate = document.getElementById("spawnSpectate") as HTMLButtonElement;
const progress = document.getElementById("cooldown") as HTMLProgressElement;
const connection = new Connection();

let cameraPositionFromServer: ClientBody;
let isAlive: boolean;
let gameTime: number;
let worldSize = 1000;
let frameCounter = 0;
let isSpectating = false;
let joiningWorld = false;
let spawnOnView = false;
let cooldownProgressValue = 0;

const container = new GameContainer(document.getElementById("gameCanvas") as HTMLCanvasElement);
const minimap = new Minimap(container);

registerContainer(container);

async function initialize():Promise<void>
{
    await container.loader.load();
    await firstLoad();
}

initialize();

bus.on("dead", () => {
    document.body.classList.remove("alive");
    document.body.classList.add("spectating");
    document.body.classList.add("dead");
    if (connection.hook.AllowedColors)
        setCurrentWorld().allowedColors = connection.hook.AllowedColors;

    initializeWorld();
    isSpectating = true;
});

bus.on("worldview", (newView: NetWorldView) => {

    gameTime = connection.currentWorldTime();

    isAlive = newView.isalive();
    container.fleetID = newView.fleetid();

    if (!isAlive && connection.hook != null)
    {
        buttonSpectate.disabled = spawnButton.disabled = (connection.hook.CanSpawn === false);
    }
    if (isAlive)
        isSpectating = false;


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
            let extra = announcement.extradata();

            if (extra) extra = JSON.parse(extra);
            if (announcement.type() == "announce" && announcement.text() == "Launch now to join the next game!") {
                container.sounds.beep.play();
            }
            log.addEntry({
                type: announcement.type()!,
                text: announcement.text()!,
                pointsDelta: announcement.pointsdelta(),
                extraData: extra,
            });
        }
    }

    setPlayerCount(newView.playercount());
    setSpectatorCount(newView.spectatorcount());

    cooldownProgressValue = newView.cooldownshoot();
    cameraPositionFromServer = Cache.bodyFromServer(newView.camera()!);

    if (spawnOnView) {
        spawnOnView = false;
        doSpawn();
    }
});

setInterval(() => {
    progress.value = cooldownProgressValue;
}, 100);

function doSpawn() {
    if ("ontouchstart" in document.documentElement) {
        window.scrollTo(0, 0);
        try {
            document.documentElement.requestFullscreen();
        } catch (e) {
            console.log("Fullscreen failed", e);
        }
    }

    document.body.classList.remove("dead");
    document.body.classList.remove("spectating");
    document.body.classList.add("alive");
    connection.sendSpawn(Controls.emoji + Controls.nick, Controls.color, Controls.ship, getToken());
    container.focus();
}

spawnButton.addEventListener("click", doSpawn);
buttonSpectate.addEventListener("click", doSpawn);

function startSpectate(hideButton = false) {
    isSpectating = true;
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

function updateStats() {
    connection.framesPerSecond = frameCounter;
    connection.viewsPerSecond = container.viewCounter;
    connection.updatesPerSecond = container.updateCounter;
    setPerf(connection.latency, connection.minimumLatency, frameCounter, connection.statViewCPUPerSecond);

    frameCounter = 0;
    container.viewCounter = 0;
    container.updateCounter = 0;
}
setInterval(updateStats, 1000);

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
    //if (document.body.classList.contains("spectating") && e.key === "Enter") {
    //    doSpawn();
    //}
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
    initializeWorld(world);
    connection.disconnect();
    connection.connect(worldKey);
});

bus.on("leaderboard", (lb) => {
    minimap.update(lb, worldSize, container.fleetID);
});

bus.on("themechange", () => {
    console.log('theme change');
    container.loader.load()
        .then(() => {
            container.cache.refreshSprites();
            initializeWorld();
        });
});

window.addEventListener("resize", () => {
    container.resize();
});

let spectateNextDebounce = false;
// Game Loop
container.engine.runRenderLoop(() => {
    frameCounter++;

    if (connection.serverClockOffset != -1 && cameraPositionFromServer) {
        gameTime = performance.now() - connection.serverClockOffset;
        //console.log(gameTime);

        projectObject(cameraPositionFromServer, gameTime);
        container.positionCamera(cameraPositionFromServer.Position);
        container.cache.tick(gameTime);
        container.scene.render()

        let spectateControl = "";
        if (isSpectating) {
            if (spectateNextDebounce && !Controls.shoot)
            {
                spectateNextDebounce = false;
            }
            if (!spectateNextDebounce && Controls.shoot)
            {
                spectateControl = "action:next";
                spectateNextDebounce = true;
            }
            else
                spectateControl = "spectating";
        }
        
        updateControlAim();
        
        connection.sendControl(
            Controls.boost,
            Controls.shoot || Controls.autofire,
            Controls.mouseX,
            Controls.mouseY,
            spectateControl,
            Controls.customData
        );
    }
});

bus.on('loaded', () => {
    console.log("done loading");
    var loadingElement = document.querySelector(".loading");
    if (loadingElement) loadingElement.classList.remove("loading");
});
