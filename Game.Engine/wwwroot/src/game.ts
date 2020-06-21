﻿"use strict";

import * as PIXI from "pixi.js";
window.PIXI = PIXI;

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

// import "pixi-tilemap";
import "./changelog";

import "./hintbox";
import { Vector2 } from "./Vector2";
import { CustomContainer } from "./CustomContainer";
declare global {
    interface Window {
        Game: any;
        discordData: any;
    }
}
window.Game = window.Game || {};

const size = { width: 1000, height: 500 };
const canvas = document.getElementById("gameCanvas") as HTMLCanvasElement;

PIXI.settings.SCALE_MODE = PIXI.SCALE_MODES.LINEAR; // PIXI.SCALE_MODES.NEAREST
PIXI.settings.RESOLUTION = window.devicePixelRatio || 1;
const app = new PIXI.Application({ view: canvas, transparent: true });
app.stage.sortableChildren = true;

const container = new CustomContainer();
container.sortableChildren = true;
container.zIndex = 3;
app.stage.addChild(container);

const backgroundGroup = new PIXI.Container();
backgroundGroup.zIndex = 0;
const tileGroup = new PIXI.Container();
tileGroup.zIndex = 1;
const bodyGroup = new PIXI.Container();
bodyGroup.zIndex = 2;

app.stage.addChild(backgroundGroup);
app.stage.addChild(tileGroup);
app.stage.addChild(bodyGroup);

container.backgroundGroup = backgroundGroup;
container.bodyGroup = bodyGroup;

container.emitterContainer = new PIXI.ParticleContainer();
container.emitterContainer.zIndex = 10;
container.addChild(container.emitterContainer);
app.stage.sortChildren();

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

const cache = new Cache(container);
let view: null | { camera?: any; time?: number; isAlive?: boolean } = null;
let serverTimeOffset = null;
let lastOffset = null;
let gameTime = null;
let lastPosition = null;
let worldSize = 1000;

let currentWorld = false;

Controls.registerCanvas(canvas);

const connection = new Connection();

window.Game.primaryConnection = connection;
window.Game.isBackgrounded = false;
window.Game.cache = cache;
window.Game.controls = Controls;

window.Game.reinitializeWorld = function () {
    if (currentWorld) Controls.initializeWorld(currentWorld);

    background.refreshSprite();
};

const bodyFromServer = (_cache: Cache, body) => {
    const originalPosition = body.originalPosition();
    const momentum = body.velocity();
    const groupID = body.group();
    const VELOCITY_SCALE_FACTOR = 5000.0;

    const spriteIndex = body.sprite();
    let spriteName = null;
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
        OriginalPosition: new Vector2(originalPosition.x(), originalPosition.y()),
    };

    return newBody;
};

const groupFromServer = (_cache, group) => {
    const newGroup = {
        ID: group.group(),
        Caption: group.caption(),
        Type: group.type(),
        ZIndex: group.zindex(),
        CustomData: group.customData(),
    };

    if (newGroup.CustomData) newGroup.CustomData = JSON.parse(newGroup.CustomData);

    return newGroup;
};

connection.onLeaderboard = (lb) => {
    leaderboard.update(lb, lastPosition, fleetID);
    minimap.update(lb, worldSize, fleetID);
};

let fleetID = 0;
let lastAliveState: boolean | null = null;
let aliveSince: number | null = null;
let joiningWorld = false;

connection.onConnected = () => {
    connection.sendAuthenticate(getToken());
};

connection.onView = (newView) => {
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
    } else if (!view.isAlive && lastAliveState) {
        lastAliveState = false;

        setTimeout(function () {
            document.body.classList.remove("alive");
            document.body.classList.add("spectating");
            document.body.classList.add("dead");
        }, 500);

        Events.Death((gameTime - aliveSince) / 1000);

        let countDown = 3;
        let interval = null;
        const updateButton = function () {
            const button = document.getElementById("spawn") as HTMLButtonElement;
            const buttonSpectate = document.getElementById("spawnSpectate") as HTMLButtonElement;

            if (countDown > 0) {
                buttonSpectate.value = button.value = `${countDown--} ...`;
                buttonSpectate.disabled = button.disabled = true;
            } else {
                buttonSpectate.value = button.value = `Launch!`;
                buttonSpectate.disabled = button.disabled = false;
                clearInterval(interval);
            }
        };
        updateButton();

        interval = setInterval(updateButton, 1000);
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
                const worldKey = announcement.text();

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
                    extraData: extra,
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
    chat: null,
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

        let customData = Controls.customData;

        if (message.time + 3000 > Date.now()) customData = JSON.stringify({ chat: message.txt });
        if ((window as any).magic) {
            customData = JSON.stringify(Object.assign(JSON.parse(customData || "{}"), { magic: (window as any).magic }));
            (window as any).magic = undefined;
        }

        connection.sendControl(angle, Controls.boost, Controls.shoot, aimTarget.x, aimTarget.y, spectateControl, customData);

        lastControl = {
            angle,
            aimTarget,
            boost: Controls.boost,
            shoot: Controls.shoot,
            chat: message.txt,
        };
    }
}, 10);

LobbyCallbacks.onLobbyClose = function () {
    clearLeaderboards();
};

let spawnOnView = false;
LobbyCallbacks.onWorldJoin = function (worldKey, world) {
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
    connection.sendSpawn(Controls.emoji + Controls.nick, Controls.color, Controls.ship, getToken());
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
    minimap.size(size);
    app.renderer.resize(width, height);
    container.scale.set(width / 5500, width / 5500);
};

sizeCanvas();

window.addEventListener("resize", () => {
    sizeCanvas();
});

let frameCounter = 0;
let viewCounter = 0;
let updateCounter = 0;
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

// Game Loop
app.ticker.add(() => {
    gameTime = performance.now() + serverTimeOffset;
    frameCounter++;

    let position = new Vector2(0, 0);

    if (view) {
        const positionA = interpolator.projectObject(view.camera, gameTime);
        position = new Vector2(positionA.x, positionA.y);
        position.x = Math.floor(position.x * 0.2 + lastCamera.x * 0.8);
        position.y = Math.floor(position.y * 0.2 + lastCamera.y * 0.8);

        lastCamera = position;

        camera.moveTo(position);
        camera.zoomTo(5500);
    }
    container.pivot.x = Math.floor(position.x - 5500 / 2);
    container.pivot.y = Math.floor(position.y - (5500 / 2) * (9 / 16));
    container.position.x = Math.floor(container.position.x);
    container.position.y = Math.floor(container.position.y);

    renderer.draw(cache, interpolator, gameTime, fleetID);
    background.updateFocus(new Vector2(position.x, position.y));
    background.draw();
    minimap.checkDisplay();

    lastPosition = position;

    log.check();

    if (Controls.mouseX) {
        const pos = camera.screenToWorld(new Vector2(Controls.mouseX, Controls.mouseY));
        angle = Controls.angle;
        aimTarget = new Vector2(Settings.mouseScale * (pos.x - position.x), Settings.mouseScale * (pos.y - position.y));
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
if ((query as any).spectate && (query as any).spectate !== "0") {
    startSpectate(true);
}

// clicking enter in nick causes fleet spawn
document.getElementById("nick").addEventListener("keyup", function (e) {
    if (e.keyCode === 13) {
        doSpawn();
    }
});

// clicking enter in spectate mode causes fleet spawn
document.body.addEventListener("keydown", function (e) {
    if (document.body.classList.contains("spectating") && e.keyCode === 13) {
        doSpawn();
    }
});

// toggle worlds with W
const worlds = document.getElementById("worlds");
document.body.addEventListener("keydown", function (e) {
    if (document.body.classList.contains("dead") && document.getElementById("nick") !== document.activeElement && e.keyCode === 87) {
        if (worlds.classList.contains("closed")) {
            worlds.classList.remove("closed");
        } else {
            worlds.classList.add("closed");
        }
    }
});

document.getElementById("wcancel").addEventListener("click", function () {
    worlds.classList.add("closed");
});
