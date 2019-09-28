import "babel-polyfill";

import { Renderer } from "./renderer";
import { Background } from "./background";
import { Border } from "./border";
import { Overlay } from "./overlay";
import { spriteIndices } from "./spriteIndices";
import { Camera } from "./camera";
import { Cache } from "./cache";
import { Interpolator } from "./interpolator";
import { Leaderboard, clear as clearLeaderboards } from "./leaderboard";
import { Minimap } from "./minimap";
import { HUD } from "./hud";
import { Log } from "./log";
import { Cooldown } from "./cooldown";
import { Controls } from "./controls";
import { message } from "./chat";
import { Connection } from "./connection";
import { getToken } from "./discord";
import { Settings } from "./settings";
import { Events } from "./events";
import { LobbyCallbacks, toggleLobby } from "./lobby";
import * as PIXI from "pixi.js";
import "pixi-tilemap";
import "./changelog";

import "./hintbox";
import { Vector2 } from "./Vector2";
import { CustomContainer } from "./CustomContainer";

window.Game = window.Game || {};

const size = { width: 1000, height: 500 };
const canvas = document.getElementById("gameCanvas") as HTMLCanvasElement;
const zoom = 1000;

//PIXI.settings.SCALE_MODE = PIXI.SCALE_MODES.NEAREST;
PIXI.settings.SCALE_MODE = PIXI.SCALE_MODES.LINEAR;
//PIXI.settings.RESOLUTION = window.devicePixelRatio || 1;
const app = new PIXI.Application(<PIXI.ApplicationOptions>{ view: canvas, transparent: true });
app.stage = new PIXI.display.Stage();
(<PIXI.display.Stage>app.stage).group.enableSort = true;
const container = new CustomContainer();
app.stage.addChild(container);

var backgroundGroup = new PIXI.display.Group(0, true);
var tileGroup = new PIXI.display.Group(1, true);
var bodyGroup = new PIXI.display.Group(2, true);

app.stage.addChild(new PIXI.display.Layer(backgroundGroup));
app.stage.addChild(new PIXI.display.Layer(tileGroup));
app.stage.addChild(new PIXI.display.Layer(bodyGroup));

container.backgroundGroup = backgroundGroup;
container.bodyGroup = bodyGroup;

container.tiles = new PIXI.tilemap.CompositeRectTileLayer(0);
container.tiles.parentGroup = tileGroup;
container.addChild(container.tiles);

container.emitterContainer = new PIXI.particles.ParticleContainer();
container.emitterContainer.parentGroup = bodyGroup;
container.zOrder = 128;
container.addChild(container.emitterContainer);

const renderer = new Renderer(container);
const background = new Background(container);
const border = new Border(container);
const overlay = new Overlay(container, canvas, document.getElementById("plotly"));
container.plotly = document.getElementById("plotly");
const camera = new Camera(size);
const interpolator = new Interpolator();
const leaderboard = new Leaderboard();
const minimap = new Minimap(app.stage, size);
const hud = new HUD();
const log = new Log();
const cooldown = new Cooldown();
let isSpectating = false;

let angle = 0.0;
let aimTarget = new Vector2(0, 0);
let d = 500; // for steering with arrows

let keyboardSteering = false;
let keyboardSteeringSpeed = 0.075;

let cache = new Cache(container);
let view = null;
let serverTimeOffset = null;
let lastOffset = null;
let gameTime = null;
let lastPosition = null;
let worldSize = 1000;

let CustomData = null;
let CustomDataTime = null;

let currentWorld = false;

Controls.registerCanvas(canvas);

const connection = new Connection();
/*if (window.location.hash) connection.connect(window.location.hash.substring(1));
else connection.connect();*/

window.Game.primaryConnection = connection;
window.Game.isBackgrounded = false;
window.Game.cache = cache;
window.Game.controls = Controls;

window.Game.reinitializeWorld = function() {
    if (currentWorld) Controls.initializeWorld(currentWorld);

    background.refreshSprite();
};

const bodyFromServer = (cache: Cache, body) => {
    const originalPosition = body.originalPosition();
    const momentum = body.velocity();
    const groupID = body.group();
    const VELOCITY_SCALE_FACTOR = 5000.0;

    var spriteIndex = body.sprite();
    var spriteName = null;
    if (spriteIndex >= 1000) spriteName = `map[${spriteIndex - 1000}]`;
    else spriteName = spriteIndices[spriteIndex];

    const newBody = {
        ID: body.id(),
        DefinitionTime: body.definitionTime(),
        Size: body.size() * 5,
        Sprite: spriteName,
        Mode: body.mode(),
        Color: "red",
        Group: groupID,
        OriginalAngle: (body.originalAngle() / 127) * Math.PI,
        AngularVelocity: body.angularVelocity() / 10000,
        Momentum: new Vector2(momentum.x() / VELOCITY_SCALE_FACTOR, momentum.y() / VELOCITY_SCALE_FACTOR),
        OriginalPosition: new Vector2(originalPosition.x(), originalPosition.y())
    };

    return newBody;
};

const groupFromServer = (cache, group) => {
    const newGroup = {
        ID: group.group(),
        Caption: group.caption(),
        Type: group.type(),
        ZIndex: group.zindex(),
        CustomData: group.customData()
    };

    if (newGroup.CustomData) newGroup.CustomData = JSON.parse(newGroup.CustomData);

    return newGroup;
};

connection.onLeaderboard = lb => {
    leaderboard.update(lb, lastPosition, fleetID);
    minimap.update(lb, worldSize, fleetID);
};

var fleetID = 0;
let lastAliveState = null;
let aliveSince = null;
let joiningWorld = false;

connection.onConnected = () => {
    connection.sendAuthenticate(getToken());
};

connection.onView = newView => {
    viewCounter++;

    view = {};
    view.time = newView.time();

    view.isAlive = newView.isAlive();

    fleetID = newView.fleetID();
    if (view.isAlive && !lastAliveState) {
        lastAliveState = true;
        document.body.classList.remove("dead");
        document.body.classList.remove("spectating");
        document.body.classList.add("alive");
        canvas.style.visibility = "initial";
        $(".visibility").hide();
        $(".visibility3").show();
        $("#overlay").css("opacity", "0");
    } else if (!view.isAlive && lastAliveState) {
        lastAliveState = false;

        setTimeout(function() {
            document.body.classList.remove("alive");
            document.body.classList.add("spectating");
            document.body.classList.add("dead");
            $(".visibility").fadeIn(2000);
            $(".visibility3").hide();
            $("#overlay").animate({"opacity":"0.7"}, 2000);
        }, 1000);

        Events.Death((gameTime - aliveSince) / 1000);

        /*let countDown = 3;
        let interval = null;
        const updateButton = function() {
            const button = document.getElementById("spawn") as HTMLButtonElement;
            const buttonSpectate = document.getElementById("spawnSpectate") as HTMLButtonElement;

            if (countDown > 0) {
                buttonSpectate.value = button.value = `${countDown--} ...`;
                buttonSpectate.disabled = button.disabled = true;
            } else {
                buttonSpectate.value = button.value = `LAUNCH!`;
                buttonSpectate.disabled = button.disabled = false;
                clearInterval(interval);
            }
        };
        updateButton();*/

        //interval = setInterval(updateButton, 1000);
    }

    lastOffset = view.time + connection.latency / 2 - performance.now();
    if (!serverTimeOffset) serverTimeOffset = lastOffset;
    serverTimeOffset = 0.95 * serverTimeOffset + 0.05 * lastOffset;

    const groupsLength = newView.groupsLength();
    const groups = [];
    for (let u = 0; u < groupsLength; u++) {
        const group = newView.groups(u);

        groups.push(groupFromServer(cache, group));
    }

    const updatesLength = newView.updatesLength();
    const updates = [];
    for (let u = 0; u < updatesLength; u++) {
        const update = newView.updates(u);

        updates.push(bodyFromServer(cache, update));
    }

    const announcementsLength = newView.announcementsLength();
    for (let u = 0; u < announcementsLength; u++) {
        const announcement = newView.announcements(u);
        switch (announcement.type()) {
            case "join":
                let worldKey = announcement.text();

                if (!joiningWorld) {
                    joiningWorld = true;
                    console.log("received join: " + worldKey);
                    LobbyCallbacks.joinWorld(worldKey);
                }
                break;
            default:
                let extra = announcement.extraData();

                if (extra) extra = JSON.parse(extra);

                log.addEntry({
                    type: announcement.type(),
                    text: announcement.text(),
                    pointsDelta: announcement.pointsDelta(),
                    extraData: extra
                });
                break;
        }
    }

    updateCounter += updatesLength;

    const deletes = [];
    const deletesLength = newView.deletesLength();
    for (let d = 0; d < deletesLength; d++) deletes.push(newView.deletes(d));

    const groupDeletes = [];
    const groupDeletesLength = newView.groupDeletesLength();
    for (let d = 0; d < groupDeletesLength; d++) groupDeletes.push(newView.groupDeletes(d));

    cache.update(updates, deletes, groups, groupDeletes, gameTime, fleetID);
    overlay.update(newView.customData());

    hud.playerCount = newView.playerCount();
    hud.spectatorCount = newView.spectatorCount();

    if (newView.worldSize() != border.worldSize) {
        worldSize = newView.worldSize();
        border.updateWorldSize(newView.worldSize());
    }

    cooldown.setCooldown(newView.cooldownShoot());
    /*console.log({
        playerCount: Game.Stats.playerCount,
        cooldownBoost: newView.cooldownBoost(),
        cooldownShoot: newView.cooldownShoot()
    })*/

    view.camera = bodyFromServer(cache, newView.camera());

    if (spawnOnView) {
        spawnOnView = false;
        doSpawn();
    }
};

let lastControl = {
    angle: null,
    aimTarget: null,
    boost: null,
    shoot: null,
    chat: null
};

setInterval(() => {
    if (
        angle !== lastControl.angle ||
        aimTarget.x !== aimTarget.x ||
        aimTarget.y !== aimTarget.y ||
        Controls.boost !== lastControl.boost ||
        Controls.shoot !== lastControl.shoot ||
        message.txt !== lastControl.chat
    ) {
        let spectateControl = null;
        if (isSpectating) {
            if (Controls.shoot) spectateControl = "action:next";
            else spectateControl = "spectating";
        }

        var customData = null;

        if (message.time + 3000 > Date.now()) customData = JSON.stringify({ chat: message.txt });

        connection.sendControl(angle, Controls.boost, Controls.shoot, aimTarget.x, aimTarget.y, spectateControl, customData);

        lastControl = {
            angle,
            aimTarget,
            boost: Controls.boost,
            shoot: Controls.shoot,
            chat: message.txt
        };
    }
}, 10);

LobbyCallbacks.onLobbyClose = function() {
    clearLeaderboards();
};

var spawnOnView = false;
LobbyCallbacks.onWorldJoin = function(worldKey, world) {
    console.log(`onWorldJoin: ${worldKey} ${world}`);
    if (joiningWorld) {
        joiningWorld = false;
        spawnOnView = true;
    }

    currentWorld = world;
    connection.disconnect();
    cache.empty();
    connection.connect(worldKey);
    serverTimeOffset = false;

    Controls.initializeWorld(world);
};

function doSpawn() {
    Events.Spawn();
    aliveSince = gameTime;
    connection.sendSpawn(Controls.nick, Controls.color, Controls.ship, getToken());
    document.getElementById("overlay").style.opacity = 0;
    document.getElementById("selfNickContainer").innerHTML = Controls.nick;
    $(".visibility2").show();
    $(".visibility3").show();    
}
document.getElementById("spawn").addEventListener("click", doSpawn);
document.getElementById("spawnSpectate").addEventListener("click", doSpawn);

function startSpectate(hideButton = false) {
    isSpectating = true;
    Events.Spectate();
    document.body.classList.add("spectating");
    document.body.classList.add("dead");

    if (hideButton) {
        document.body.classList.add("spectate_only");
    }
}

document.getElementById("spectate").addEventListener("click", () => {
    startSpectate();
});

function stopSpectate() {
    isSpectating = false;
    document.body.classList.remove("spectating");
    document.body.classList.remove("spectate_only");
}

document.getElementById("stop_spectating").addEventListener("click", () => {
    stopSpectate();
    document.getElementById("deathScreen").style.visibility = "hidden";
});

document.addEventListener("keydown", ({ keyCode, which }) => {
    if (keyCode == 27 || which == 27) {
        if (lastAliveState) {
            connection.sendExit();
        } else if (isSpectating) {
            stopSpectate();
        } else if (document.body.classList.contains("lobby")) {
            toggleLobby();
        } else {
            startSpectate();
        }
    }
});

const sizeCanvas = () => {
    let width;
    let height;
    if ((window.innerWidth * 9) / 16 >= window.innerHeight) {
        width = window.innerWidth;
        height = (width * 9) / 16;
    } else {
        height = window.innerHeight;
        width = (height * 16) / 9;
    }

    size.width = Math.floor(width);
    size.height = Math.floor(height);
    minimap.size(size);
    app.renderer.resize(width, height);
    container.scale.set(width / zoom, width / zoom);
};

sizeCanvas();

window.addEventListener("resize", () => {
    sizeCanvas();
});

let frameCounter = 0;
var viewCounter = 0;
var updateCounter = 0;
let lastCamera = new Vector2(0, 0);

function doPing() {
    hud.framesPerSecond = frameCounter;
    connection.framesPerSecond = frameCounter;
    connection.viewsPerSecond = viewCounter;
    connection.updatesPerSecond = updateCounter;

    hud.latency = connection.latency;

    if (frameCounter === 0) {
        //console.log("backgrounded");
        window.Game.isBackgrounded = true;
    } else window.Game.isBackgrounded = false;
    frameCounter = 0;
    viewCounter = 0;
    updateCounter = 0;
}

doPing();
setInterval(doPing, 1000);

const graphics = new PIXI.Graphics();
container.addChild(graphics);

const fleetSizeDisplay = document.getElementById("fleetSize");
const dangerZoneWarning = document.getElementById("dangerZoneWarning");

let lastCustomData = false;
let spotSprites = [];

// Game Loop
app.ticker.add(() => {
    const latency = connection.minLatency || 0;
    gameTime = performance.now() + serverTimeOffset;
    frameCounter++;

    for (var key in cache.groups) {
        if (cache.groups[key].ID == fleetID) {
             fleetSizeDisplay.innerHTML = cache.groups[key].renderer.ships.length;
        }
    }

    let position = new Vector2(0, 0);

    if (view) {
        var positionA = interpolator.projectObject(view.camera, gameTime);
        position = new Vector2(positionA.x, positionA.y);
        position.x = Math.floor(position.x * 0.2 + lastCamera.x * 0.8);
        position.y = Math.floor(position.y * 0.2 + lastCamera.y * 0.8);

        lastCamera = position;

        camera.moveTo(position);
        camera.zoomTo(zoom);
    }
    container.pivot.x = Math.floor(position.x - zoom / 2);
    container.pivot.y = Math.floor(position.y - (zoom / 2) * (9 / 16));
    container.position.x = Math.floor(container.position.x);
    container.position.y = Math.floor(container.position.y);

    renderer.draw(cache, interpolator, gameTime, fleetID);
    background.updateFocus(new Vector2(position.x, position.y));
    background.draw();
    minimap.checkDisplay();

    lastPosition = position;
    
    if ((Math.abs(position.x) > worldSize || Math.abs(position.y) > worldSize) && document.body.classList.contains("alive")) {
        dangerZoneWarning.style.display = "block";
    } else {
        dangerZoneWarning.style.display = "none";
    }

    log.check();
    // cooldown.draw();

    if (Controls.mouseX) {
        var pos;

        if (
            Controls.numUp ||
            Controls.numUpRight ||
            Controls.numRight ||
            Controls.numDownRight ||
            Controls.numDown ||
            Controls.numDownLeft ||
            Controls.numLeft ||
            Controls.numUpLeft ||
            keyboardSteering
        ) {
            var i = 0;
            if (Controls.numUp) {
                angle = mergeSet(angle, (3 * Math.PI) / 2, i);
                i++;
            }
            if (Controls.numUpRight) {
                angle = mergeSet(angle, (7 * Math.PI) / 4, i);
                i++;
            }
            if (Controls.numRight) {
                angle = mergeSet(angle, 0, i);
                i++;
            }
            if (Controls.numDownRight) {
                angle = mergeSet(angle, Math.PI / 4, i);
                i++;
            }
            if (Controls.numDown) {
                angle = mergeSet(angle, Math.PI / 2, i);
                i++;
            }
            if (Controls.numDownLeft) {
                angle = mergeSet(angle, (3 * Math.PI) / 4, i);
                i++;
            }
            if (Controls.numLeft) {
                angle = mergeSet(angle, Math.PI, i);
                i++;
            }
            if (Controls.numUpLeft) {
                angle = mergeSet(angle, (5 * Math.PI) / 4, i);
                i++;
            }
            /*if (Controls.right || Controls.left || Controls.up || Controls.down || keyboardSteering) {
            if (Controls.right && !Controls.left) {
                angle += keyboardSteeringSpeed * Math.PI;
            } else if (Controls.left && !Controls.right) {
                angle -= keyboardSteeringSpeed * Math.PI;
            }
            if (Controls.up) {
                angle += Math.PI;
            } // optional
            */
            aimTarget = new Vector2(d * Math.cos(angle), d * Math.sin(angle));
            keyboardSteering = true;
        } else {
            pos = camera.screenToWorld(new Vector2(Controls.mouseX, Controls.mouseY));
            angle = Controls.angle;
            aimTarget = new Vector2(Settings.mouseScale * (pos.x - position.x), Settings.mouseScale * (pos.y - position.y));
        }
    }

    if (CustomData != lastCustomData) {
        lastCustomData = CustomData;

        for (let i = 0; i < spotSprites.length; i++) container.removeChild(spotSprites[i]);

        spotSprites = [];

        //graphics.clear();

        if (CustomData) {
            const data = JSON.parse(CustomData);
            /*if (data.spots)
            {
                for (let i=0; i<data.spots.length; i++)
                {
                    let spot = data.spots[i];
                    let texture = textures["obstacle"];
                    if (texture) {
                        let sprite = new PIXI.Sprite(texture);
                        sprite.position.x = spot.X;
                        sprite.position.y = spot.Y;
                        sprite.scale.set(0.1, 0.1);

                        container.addChild(sprite);

                        spotSprites.push(sprite);
                    } else console.log("cannot find texture");
                }
            }*/
        }
    }
});

document.body.classList.remove("loading");

function parseQuery(queryString) {
    const query = {};
    const pairs = (queryString[0] === "?" ? queryString.substr(1) : queryString).split("&");
    for (let i = 0; i < pairs.length; i++) {
        const pair = pairs[i].split("=");
        query[decodeURIComponent(pair[0])] = decodeURIComponent(pair[1] || "");
    }
    return query;
}

const query = parseQuery(window.location.search);
if ((<any>query).spectate && (<any>query).spectate !== "0") {
    startSpectate(true);
}

canvas.onmousemove = function() {
    keyboardSteering = false;
};

// clicking enter in nick causes fleet spawn
document.getElementById("nick").addEventListener("keyup", function(e) {
    if (e.keyCode === 13) {
        doSpawn();
    }
});

// clicking enter in spectate mode causes fleet spawn
document.body.addEventListener("keydown", function(e) {
    if (document.body.classList.contains("spectating") && e.keyCode === 13) {
        doSpawn();
    }
});

// toggle worlds with W
const worlds = document.getElementById("worlds");
document.body.addEventListener("keydown", function(e) {
    if (document.body.classList.contains("dead") && document.getElementById("nick") !== document.activeElement && e.keyCode === 87) {
        if (worlds.classList.contains("closed")) {
            worlds.classList.remove("closed");
        } else {
            worlds.classList.add("closed");
        }
    }
});

document.getElementById("wcancel").addEventListener("click", function() {
    worlds.classList.add("closed");
});

function mergeSet(a0, a, i) {
    var ret = (a0 * i + a) / (i + 1);
    if (Math.abs(a - a0) > Math.PI) {
        ret += Math.PI;
    }
    return ret;
}
