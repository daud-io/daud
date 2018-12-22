import "babel-polyfill";

import { Renderer, spriteIndices } from "./renderer";
import { Camera } from "./camera";
import { Cache } from "./cache";
import { Interpolator } from "./interpolator";
import { Leaderboard } from "./leaderboard";
import { HUD } from "./hud";
import { Log } from "./log";
import { Cooldown } from "./cooldown";
import { Background } from "./background";
import { Controls } from "./controls";
import { Connection } from "./connection";
import { token } from "./discord";
import { Settings } from "./settings";
import { Events } from "./events";
import "./hintbox";
import { blur } from "./lobby";

const canvas = document.getElementById("gameCanvas");
const context = canvas.getContext("2d");
const renderer = new Renderer(context, {});
const background = new Background(canvas, context, {});
const camera = new Camera(context);
const interpolator = new Interpolator();
const leaderboard = new Leaderboard(canvas, context);
const hud = new HUD(canvas, context);
const log = new Log(canvas, context);
const cooldown = new Cooldown(canvas, context);
let isSpectating = false;

let angle = 0.0;
let aimTarget = { X: 0, Y: 0 };

let cache = new Cache();
let view = false;
let serverTimeOffset = false;
let lastOffset = false;
let gameTime = false;
let lastPosition = false;

Controls.registerCanvas(canvas);

const connection = new Connection();
if (window.location.hash) connection.connect(window.location.hash.substring(1));
else connection.connect();

window.Game.primaryConnection = connection;
window.Game.isBackgrounded = false;

const bodyFromServer = (cache, body) => {
    const originalPosition = body.originalPosition();
    const momentum = body.velocity();
    const group = cache.getGroup(body.group());
    const groupID = (group && group.ID) || 0;
    const VELOCITY_SCALE_FACTOR = 5000.0;

    const newBody = {
        ID: body.id(),
        DefinitionTime: body.definitionTime(),
        Size: body.size() * 5,
        Sprite: spriteIndices[body.sprite()], //body.sprite(),
        Mode: body.mode(),
        Color: "red", //body.color(),
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
    leaderboard.setData(lb);
    leaderboard.position = lastPosition;
};

var fleetID = 0;
let lastAliveState = false;
let aliveSince = false;
connection.onView = newView => {
    viewCounter++;

    view = {};
    view.time = newView.time();

    view.isAlive = newView.isAlive();

    if (view.isAlive && !lastAliveState) {
        lastAliveState = true;
        fleetID = newView.fleetID();
        document.body.classList.remove("dead");
        document.body.classList.add("alive");
    } else if (!view.isAlive && lastAliveState) {
        lastAliveState = false;

        setTimeout(function() {
            document.body.classList.remove("alive");
            document.body.classList.add("dead");
        }, 500);

        Events.Death((gameTime - aliveSince) / 1000);

        var countDown = 3;
        var interval = false;
        var updateButton = function() {
            var button = document.getElementById("spawn");
            console.log(`cooldown: ${countDown}`);

            if (countDown > 0) {
                console.log("hold");
                button.value = `${countDown--} ...`;
                button.disabled = true;
            } else {
                console.log("Launch!");
                button.value = `LAUNCH!`;
                button.disabled = false;
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
    for (var u = 0; u < groupsLength; u++) {
        const group = newView.groups(u);

        groups.push(groupFromServer(cache, group));
    }

    const updatesLength = newView.updatesLength();
    const updates = [];
    for (var u = 0; u < updatesLength; u++) {
        const update = newView.updates(u);

        updates.push(bodyFromServer(cache, update));
    }

    const announcementsLength = newView.announcementsLength();
    for (var u = 0; u < announcementsLength; u++) {
        const announcement = newView.announcements(u);
        log.addEntry(announcement.text());
    }

    updateCounter += updatesLength;

    const deletes = [];
    const deletesLength = newView.deletesLength();
    for (var d = 0; d < deletesLength; d++) deletes.push(newView.deletes(d));

    const groupDeletes = [];
    const groupDeletesLength = newView.groupDeletesLength();
    for (var d = 0; d < groupDeletesLength; d++) groupDeletes.push(newView.groupDeletes(d));

    cache.update(updates, deletes, groups, groupDeletes, gameTime);

    Game.Stats.playerCount = newView.playerCount();
    Game.Stats.spectatorCount = newView.spectatorCount();

    renderer.worldSize = newView.worldSize();

    cooldown.setCooldown(newView.cooldownShoot());
    /*console.log({
        playerCount: Game.Stats.playerCount,
        cooldownBoost: newView.cooldownBoost(),
        cooldownShoot: newView.cooldownShoot()
    })*/

    view.camera = bodyFromServer(cache, newView.camera());
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

document.getElementById("wcancel").addEventListener("click", () => {
    worlds.classList.add("closed");
    blur();
    cache = new Cache();
});

document.getElementById("spawn").addEventListener("click", () => {
    Events.Spawn();
    aliveSince = gameTime;
    connection.sendSpawn(Controls.nick, Controls.color, Controls.ship, token);
});

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
            console.log("sending exit");
        } else if (isSpectating) {
            stopSpectate();
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

    canvas.width = width;
    canvas.height = height;
};

sizeCanvas();

window.addEventListener("resize", () => {
    sizeCanvas();
});

window.Game.Stats = {
    framesPerSecond: 0,
    viewsPerSecond: 0,
    updatesPerSecond: 0
};

let frameCounter = 0;
var viewCounter = 0;
var updateCounter = 0;
let lastCamera = { X: 0, Y: 0 };

function doPing() {
    window.Game.Stats.framesPerSecond = frameCounter;
    window.Game.Stats.viewsPerSecond = viewCounter;
    window.Game.Stats.updatesPerSecond = updateCounter;
    hud.update();

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

// Game Loop
function gameLoop() {
    requestAnimationFrame(gameLoop);
    const latency = connection.minLatency || 0;
    gameTime = performance.now() + serverTimeOffset - latency / 2;
    frameCounter++;

    let position = { X: 0, Y: 0 };

    if (view) {
        position = interpolator.projectObject(view.camera, gameTime);
        position.X = position.X * 0.2 + lastCamera.X * 0.8;
        position.Y = position.Y * 0.2 + lastCamera.Y * 0.8;

        lastCamera = position;

        camera.moveTo(position.X, position.Y);
        camera.zoomTo(5500);
    }

    camera.begin();
    background.draw(position.X, position.Y);
    renderer.view = view;
    renderer.draw(cache, interpolator, gameTime, fleetID);

    camera.end();

    lastPosition = position;

    leaderboard.draw(leaderboard.position);
    log.draw();
    cooldown.draw();

    if (Controls.mouseX) {
        const pos = camera.screenToWorld(Controls.mouseX, Controls.mouseY);

        angle = Controls.angle;
        aimTarget = {
            X: Settings.mouseScale * (pos.x - position.X),
            Y: Settings.mouseScale * (pos.y - position.Y)
        };
    }

    /*
    if (Game.Controls.left || Game.Controls.up)
        angle -= 0.1;
    if (Game.Controls.right || Game.Controls.down)
        angle += 0.1;
    */
}

requestAnimationFrame(gameLoop);

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
