import "babel-polyfill";

import { Renderer } from "./renderer";
import { Background } from "./background";
import { Border } from "./border";
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
import { Connection } from "./connection";
import { getToken } from "./discord";
import { Settings } from "./settings";
import { Events } from "./events";
import { LobbyCallbacks, toggleLobby } from "./lobby";
import * as PIXI from "pixi.js";
import "pixi-tilemap";

// import "./hintbox";

const size = { width: 1000, height: 500 };
const canvas = document.getElementById("gameCanvas");

//PIXI.settings.SCALE_MODE = PIXI.SCALE_MODES.NEAREST;
PIXI.settings.SCALE_MODE = PIXI.SCALE_MODES.LINEAR;
//PIXI.settings.RESOLUTION = window.devicePixelRatio || 1;
const app = new PIXI.Application({ view: canvas, transparent: true });
app.stage = new PIXI.display.Stage();
app.stage.group.enableSort = true;
const container = new PIXI.Container();
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

const renderer = new Renderer(container);
const background = new Background(container);
const border = new Border(container);
const camera = new Camera(size);
const interpolator = new Interpolator();
const leaderboard = new Leaderboard();
const minimap = new Minimap(app.stage, size);
const hud = new HUD();
const log = new Log();
const cooldown = new Cooldown();
let isSpectating = false;

let angle = 0.0;
let aimTarget = { X: 0, Y: 0 };

let cache = new Cache(container);
let view = false;
let serverTimeOffset = false;
let lastOffset = false;
let gameTime = false;
let lastPosition = false;
let worldSize = 1000;

let CustomData = false;
let CustomDataTime = false;

let currentWorld = false;

Controls.registerCanvas(canvas);

const connection = new Connection();
/*if (window.location.hash) connection.connect(window.location.hash.substring(1));
else connection.connect();*/

window.Game.primaryConnection = connection;
window.Game.isBackgrounded = false;
window.Game.cache = cache;

window.Game.reinitializeWorld = function() {
    if (currentWorld) Controls.initializeWorld(currentWorld);

    background.refreshSprite();
};

const bodyFromServer = (cache, body) => {
    const originalPosition = body.originalPosition();
    const momentum = body.velocity();
    const groupID = body.group();
    const VELOCITY_SCALE_FACTOR = 5000.0;

    var spriteIndex = body.sprite();
    var spriteName = false;
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
        Momentum: {
            X: momentum.x() / VELOCITY_SCALE_FACTOR,
            Y: momentum.y() / VELOCITY_SCALE_FACTOR
        },
        OriginalPosition: {
            X: originalPosition.x(),
            Y: originalPosition.y()
        }
    };

    return newBody;
};

const groupFromServer = (cache, group) => {
    const newGroup = {
        ID: group.group(),
        Caption: group.caption(),
        Type: group.type(),
        ZIndex: group.zindex()
    };

    return newGroup;
};

connection.onLeaderboard = lb => {
    leaderboard.update(lb, lastPosition);
    minimap.update(lb, worldSize, fleetID);
};

var fleetID = 0;
let lastAliveState = false;
let aliveSince = false;
let joiningWorld = false;

connection.onView = newView => {
    viewCounter++;

    view = {};
    view.time = newView.time();

    view.isAlive = newView.isAlive();

    if (view.isAlive && !lastAliveState) {
        lastAliveState = true;
        fleetID = newView.fleetID();
        document.body.classList.remove("dead");
        document.body.classList.remove("spectating");
        document.body.classList.add("alive");
    } else if (!view.isAlive && lastAliveState) {
        lastAliveState = false;

        setTimeout(function() {
            document.body.classList.remove("alive");
            document.body.classList.add("spectating");
            document.body.classList.add("dead");
        }, 500);

        Events.Death((gameTime - aliveSince) / 1000);

        let countDown = 3;
        let interval = false;
        const updateButton = function() {
            const button = document.getElementById("spawn");
            const buttonSpectate = document.getElementById("spawnSpectate");

            if (countDown > 0) {
                buttonSpectate.value = button.value = `${countDown--} ...`;
                buttonSpectate.disabled = button.disabled = true;
            } else {
                buttonSpectate.value = button.value = `LAUNCH!`;
                buttonSpectate.disabled = button.disabled = false;
                clearInterval(interval);
            }
        };
        updateButton();

        interval = setInterval(updateButton, 1000);
    }

    lastOffset = view.time - performance.now() + Math.random();
    if (serverTimeOffset === false) serverTimeOffset = lastOffset;
    serverTimeOffset = 0.99 * serverTimeOffset + 0.01 * lastOffset;

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
            case "message":
                log.addEntry(announcement.text());
                break;
            case "join":
                let worldKey = announcement.text();

                if (!joiningWorld) {
                    joiningWorld = true;
                    console.log("received join: " + worldKey);
                    LobbyCallbacks.joinWorld(worldKey);
                }
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

    const data = newView.customData();
    if (data) {
        CustomData = data;
        CustomDataTime = view.time;
    } else if (CustomDataTime + 5000 < view.time) {
        CustomData = false;
    }

    view.camera = bodyFromServer(cache, newView.camera());

    if (spawnOnView) {
        spawnOnView = false;
        doSpawn();
    }
};

let lastControl = {};

setInterval(() => {
    if (angle !== lastControl.angle || aimTarget.X !== aimTarget.X || aimTarget.Y !== aimTarget.Y || Controls.boost !== lastControl.boost || Controls.shoot !== lastControl.shoot) {
        let spectateControl = false;
        if (isSpectating) {
            if (Controls.shoot) spectateControl = "action:next";
            else spectateControl = "spectating";
        }

        connection.sendControl(angle, Controls.boost, Controls.shoot, aimTarget.X, aimTarget.Y, spectateControl);

        lastControl = {
            angle,
            aimTarget,
            boost: Controls.boost,
            shoot: Controls.shoot
        };
    }
}, 10);

LobbyCallbacks.onLobbyClose = function() {
    clearLeaderboards();
};

var spawnOnView = false;
LobbyCallbacks.onWorldJoin = function(worldKey, world) {
    if (joiningWorld) {
        joiningWorld = false;
        spawnOnView = true;
    }

    currentWorld = world;
    connection.disconnect();
    cache.empty();
    connection.connect(worldKey);

    Controls.initializeWorld(world);
};

function doSpawn() {
    Events.Spawn();
    aliveSince = gameTime;
    connection.sendSpawn(Controls.nick, Controls.color, Controls.ship, getToken());
}
document.getElementById("spawn").addEventListener("click", doSpawn);
document.getElementById("spawnSpectate").addEventListener("click", doSpawn);

function startSpectate(hideButton) {
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
    if ((window.innerWidth * 9) / 16 < window.innerHeight) {
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
    container.scale.set(width / 5500, width / 5500);
};

sizeCanvas();

window.addEventListener("resize", () => {
    sizeCanvas();
});

let frameCounter = 0;
var viewCounter = 0;
var updateCounter = 0;
let lastCamera = { X: 0, Y: 0 };

function doPing() {
    hud.framesPerSecond = frameCounter;
    connection.framesPerSecond = frameCounter;
    connection.viewsPerSecond = viewCounter;
    connection.updatesPerSecond = updateCounter;

    hud.latency = connection.latency;

    if (frameCounter === 0) {
        console.log("backgrounded");
        Game.isBackgrounded = true;
    } else Game.isBackgrounded = false;
    frameCounter = 0;
    viewCounter = 0;
    updateCounter = 0;
}

doPing();
setInterval(doPing, 1000);

const graphics = new PIXI.Graphics();
container.addChild(graphics);

let lastCustomData = false;
let spotSprites = [];

// Game Loop
app.ticker.add(() => {
    const latency = connection.minLatency || 0;
    gameTime = performance.now() + serverTimeOffset - latency / 2;
    frameCounter++;

    let position = { X: 0, Y: 0 };

    if (view) {
        position = interpolator.projectObject(view.camera, gameTime);
        position.X = Math.floor(position.X * 0.2 + lastCamera.X * 0.8);
        position.Y = Math.floor(position.Y * 0.2 + lastCamera.Y * 0.8);

        lastCamera = position;

        camera.moveTo(position.X, position.Y);
        camera.zoomTo(5500);
    }
    container.pivot.x = Math.floor(position.X - 5500 / 2);
    container.pivot.y = Math.floor(position.Y - (5500 / 2) * (9 / 16));
    container.position.x = Math.floor(container.position.x);
    container.position.y = Math.floor(container.position.y);

    renderer.draw(cache, interpolator, gameTime, fleetID);
    background.updateFocus(position.X, position.Y);
    background.draw(cache, interpolator, gameTime);
    minimap.checkDisplay();
    border.draw(cache, interpolator, gameTime);

    lastPosition = position;

    log.check();
    // cooldown.draw();

    if (Controls.mouseX) {
        const pos = camera.screenToWorld(Controls.mouseX, Controls.mouseY);

        angle = Controls.angle;
        aimTarget = {
            X: Settings.mouseScale * (pos.x - position.X),
            Y: Settings.mouseScale * (pos.y - position.Y)
        };
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
if (query.spectate && query.spectate !== "0") {
    startSpectate(true);
}
