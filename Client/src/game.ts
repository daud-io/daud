import "./hintbox";
import { NetWorldView } from "./daud-net/net-world-view";
import { Minimap } from "./minimap";
import * as log from "./log";
import { Controls, registerContainer} from "./controls";
import { Connection } from "./connection";
import { getToken } from "./discord";
import { GameContainer } from "./gameContainer";
import * as bus from "./bus";
import "./events";
import { Cache } from "./cache";
import { Fleet } from "./models/fleet";
import { Vector2 } from "@babylonjs/core";
import { Settings } from "./settings";

const spawnButton = document.getElementById("spawn") as HTMLButtonElement;
const buttonSpectate = document.getElementById("spawnSpectate") as HTMLButtonElement;
const connection = new Connection();

let cameraPositionFromServer: Vector2 = new Vector2();
let gameTime: number;
let worldSize = 1000;
let frameCounter = 0;
let joiningWorld = false;
let spawnOnView = false;
let cooldownProgressValue = 0;



const canvas = document.getElementById("gameCanvas") as any;
if (!canvas)
    throw "no canvas";

var container: GameContainer = new GameContainer(<HTMLCanvasElement>canvas, connection);
const minimap = new Minimap(container);

registerContainer(container);


// Game Loop
container.engine.runRenderLoop(() => {
    frameCounter++;

    if (connection.serverClockOffset != -1 && cameraPositionFromServer) {
        gameTime = performance.now() - connection.serverClockOffset;
        container.cache.tick(gameTime);
        //bus.emit('prerender', gameTime);
        bus.emitPrerender(gameTime);

        let newCamera:Vector2|null = null;
        
        if (container.fleetID != 0)
        {
            var specFleet = container.cache.getGroup(container.fleetID)?.renderer as Fleet;
            if (specFleet)
                newCamera = specFleet.center();
        }
        if (newCamera == null)
        {
            if (container.alive)
            {
                let  i = 0;
                //console.log(`warn: alive but no known fleet id:${container.fleetID}`);
            }
                
            newCamera = cameraPositionFromServer;
        }

        container.positionCamera(newCamera);
        
        if (container.ready)
            container.scene.render();

        //bus.emit('postrender', gameTime);
        bus.emitPostrender(gameTime);
    }

    connection.dispatchWorldViews();
});

bus.on("dead", () => {
    document.body.classList.remove("alive");
    document.body.classList.add("spectating");
    document.body.classList.add("dead");
});

bus.on("worldview", (newView: NetWorldView) => {
    container.fleetID = newView.fleetid();

    if (!container.alive && connection.hook != null)
        buttonSpectate.disabled = spawnButton.disabled = connection.hook.CanSpawn === false;

    
    Cache.positionFromServerBody(newView.camera()!, cameraPositionFromServer);

    const announcementsLength = newView.announcementsLength();
    for (let u = 0; u < announcementsLength; u++) {
        const announcement = newView.announcements(u)!;

        if (announcement.type() == "join") {
            const worldKey = announcement.text()!;

            if (!joiningWorld) {
                joiningWorld = true;
                console.log("received join: " + worldKey);
            }
        } else {
            let extra = announcement.extradata();

            if (extra) extra = JSON.parse(extra);
            if (announcement.type() == "announce" && announcement.text() == "Launch now to join the next game!") {
                //container.sounds.beep.play();
            }
            log.addEntry({
                type: announcement.type()!,
                text: announcement.text()!,
                pointsDelta: announcement.pointsdelta(),
                extraData: extra,
            });
        }
    }

    //cooldownProgressValue = newView.cooldownshoot();

    if (spawnOnView) {
        spawnOnView = false;
        doSpawn();
    }
});

/* setInterval(() => {
     progress.value = cooldownProgressValue;
 }, 200);*/

function doSpawn() {
    document.body.classList.remove("dead");
    document.body.classList.remove("spectating");
    document.body.classList.add("alive");
    connection.sendSpawn(Controls.emoji + Controls.nick, Controls.color, Controls.ship, getToken());
    container.focus();
    if (Settings.pointerlock)
        Controls.requestPointerLock();

}

function onLaunchClick(e:MouseEvent) {
    spawnOnView = true;

}

buttonSpectate.addEventListener("click", onLaunchClick);
spawnButton.addEventListener("click", onLaunchClick);

document.addEventListener("selectstart", (e) => {
    e.preventDefault();
    return false;
});

document.addEventListener("selectstart", (e) => {
    e.preventDefault();
    e.cancelBubble = true;
});
document.getElementById('nick')?.addEventListener("selectstart", (e) => {
    e.cancelBubble = true;
});


function startSpectate(hideButton = false) {
    document.body.classList.add("spectating");
    document.body.classList.add("dead");

    if (hideButton) {
        document.body.classList.add("spectate_only");
    }
}

document.getElementById("spectate")!.addEventListener("click", () => startSpectate());

function stopSpectate() {
    document.body.classList.remove("spectating");
    document.body.classList.remove("spectate_only");
}

const deathScreen = document.getElementById("deathScreen")!;
document.getElementById("stop_spectating")!.addEventListener("click", () => {
    stopSpectate();
    deathScreen.style.display = "";
});

document.addEventListener("keydown", ({ code, key }) => {
    if (
        code == "Escape"
        || key == "Escape" // old IE
    ) {
        if (document.body.classList.contains("alive")) {
            connection.sendExit();
        } else if (!container.alive && !deathScreen.style.display) {
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

    //if (Telemetry.shouldSend())
        //Telemetry.send(container, connection);

    frameCounter = 0;
    container.viewCounter = 0;
    container.updateCounter = 0;

    const bg = container.backgrounded;
    container.backgrounded = (connection.viewsPerSecond < 2);

    // we WERE backgrounded, but not anymore
    if(bg && !container.backgrounded)
        connection.newTimingWindow();
}
setInterval(updateStats, 1000);

// clicking enter in nick causes fleet spawn
const nick = document.getElementById("nick") as HTMLInputElement;
nick.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
        Controls.nick = nick.value;
        if (connection.hook.CanSpawn) doSpawn();
    }
});

bus.on("worldjoin", (connect) => {
    console.log(`onWorldJoin: ${connect}`);
    if (joiningWorld) {
        joiningWorld = false;
        spawnOnView = true;
    }

    connection.disconnect();
    connection.connect(connect);
});

bus.on("leaderboard", (lb) => {
    minimap.update(lb, worldSize, container.fleetID);
});

bus.on("themechange", () => {
    container.loader.load().then(() => {
        container.cache.refreshSprites();
    });
});

window.addEventListener("resize", () => {
    container.resize();
});

bus.on("loaded", () => {
    console.log("done loading");
    var loadingElement = document.querySelector(".loading");
    if (loadingElement) loadingElement.classList.remove("loading");
});

bus.emit("pageReady");

async function initialize(): Promise<void> {
    await container.loader.load();
    console.log('container loaded');
    bus.emit('gameReady');
}

initialize();




(<any>window).connect = (world:string) =>
{
    bus.emit("worldjoin", world);
}