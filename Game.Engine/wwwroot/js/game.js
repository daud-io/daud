import "babel-polyfill";

import { Renderer, spriteIndices } from "./renderer";
import { Camera } from "./camera";
import { Cache } from "./cache";
import { Interpolator } from "./interpolator";
import { Leaderboard } from "./leaderboard";
import { HUD } from "./hud";
import { Log } from "./log";
import { Background } from "./background";
import { Controls } from "./controls";
import { Connection } from "./connection";
import { token } from "./discord";
import { Settings } from "./settings";
import { Events } from "./events";
import "./hintbox";

var canvas = document.getElementById("gameCanvas");
var context = canvas.getContext("2d");
var renderer = new Renderer(context, {});
var background = new Background(canvas, context, {});
var camera = new Camera(context);
var interpolator = new Interpolator();
var leaderboard = new Leaderboard(canvas, context);
var hud = new HUD(canvas, context);
var log = new Log(canvas, context);
var isSpectating = false;

var angle = 0.0;
var aimTarget = { X: 0, Y: 0 };

var cache = new Cache();
var view = false;
var serverTimeOffset = false;
var lastOffset = false;
var gameTime = false;
var lastPosition = false;

Controls.registerCanvas(canvas);

var connection = new Connection();
if (window.location.hash)
    connection.connect(window.location.hash.substring(1));
else
    connection.connect();

window.Game.primaryConnection = connection;
window.Game.isBackgrounded = false;

var bodyFromServer = function(cache, body) {
    var originalPosition = body.originalPosition();
    var momentum = body.velocity();
    var group = cache.getGroup(body.group());
    var groupID = (group && group.ID) || 0;
    var VELOCITY_SCALE_FACTOR = 5000.0;

    var newBody = {
        ID: body.id(),
        DefinitionTime: body.definitionTime(),
        Size: body.size() * 5,
        Sprite: spriteIndices[body.sprite()], //body.sprite(),
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

var groupFromServer = function(cache, group) {
    var newGroup = {
        ID: group.group(),
        Caption: group.caption(),
        Type: group.type(),
        ZIndex: group.zindex()
    };

    return newGroup;
};

connection.onLeaderboard = function(lb) {
    leaderboard.setData(lb);
    leaderboard.position = lastPosition;
};

var lastAliveState = true;
var aliveSince = false;
connection.onView = function(newView) {
    viewCounter++;

    view = {};
    view.time = newView.time();

    view.isAlive = newView.isAlive();

    if (view.isAlive && !lastAliveState) {
        lastAliveState = true;
        document.body.classList.remove("dead");
        document.body.classList.add("alive");
    } else if (!view.isAlive && lastAliveState) {
        lastAliveState = false;
        document.body.classList.remove("alive");
        document.body.classList.add("dead");

        Events.Death((gameTime - aliveSince) / 1000);
    }

    lastOffset = view.time - performance.now();
    if (lastOffset > serverTimeOffset) serverTimeOffset = lastOffset;

    if (serverTimeOffset === false) serverTimeOffset = lastOffset;

    var groupsLength = newView.groupsLength();
    var groups = [];
    for (var u = 0; u < groupsLength; u++) {
        var group = newView.groups(u);

        groups.push(groupFromServer(cache, group));
    }

    var updatesLength = newView.updatesLength();
    var updates = [];
    for (var u = 0; u < updatesLength; u++) {
        var update = newView.updates(u);

        updates.push(bodyFromServer(cache, update));
    }

    var announcementsLength = newView.announcementsLength();
    for (var u = 0; u < announcementsLength; u++) {
        var announcement = newView.announcements(u);
        log.addEntry(announcement.text());
    }

    updateCounter += updatesLength;

    var deletes = [];
    var deletesLength = newView.deletesLength();
    for (var d = 0; d < deletesLength; d++) deletes.push(newView.deletes(d));

    var groupDeletes = [];
    var groupDeletesLength = newView.groupDeletesLength();
    for (var d = 0; d < groupDeletesLength; d++) groupDeletes.push(newView.groupDeletes(d));

    cache.update(updates, deletes, groups, groupDeletes, gameTime);

    Game.Stats.playerCount = newView.playerCount();
    Game.Stats.spectatorCount = newView.spectatorCount();

    renderer.worldSize = newView.worldSize();
    
    /*console.log({
        playerCount: Game.Stats.playerCount,
        cooldownBoost: newView.cooldownBoost(),
        cooldownShoot: newView.cooldownShoot()
    })*/

    view.camera = bodyFromServer(cache, newView.camera());
};

var lastControl = {};

setInterval(function() {
    if (angle !== lastControl.angle || aimTarget.X !== aimTarget.X || aimTarget.Y !== aimTarget.Y || Controls.boost !== lastControl.boost || Controls.shoot !== lastControl.shoot) {

        var spectateControl = false;
        if (isSpectating) {
            if (Controls.shoot)
                spectateControl = "action:next";
            else
                spectateControl = "spectating";
        }

        connection.sendControl(angle, Controls.boost, Controls.shoot, aimTarget.X, aimTarget.Y, spectateControl);

        lastControl = {
            angle: angle,
            aimTarget: aimTarget,
            boost: Controls.boost,
            shoot: Controls.shoot
        };
    }
}, 10);

document.getElementById("worldSelector").addEventListener("change", function () {
    var world = document.getElementById("worldSelector").value;
    connection.connect(world);
    cache = new Cache();
    Events.ChangeRoom(world);

    switch (world) {
        case "ctf":
            // this is super hacky... 
            // intend to make a greeting message from the server on connection
            // that explains the allowed options in the room

            document.getElementById("shipSelector").innerHTML =
                '<option value="cyan">cyan</option>' +
                '<option value="red">red</option>';

            if (Controls.color != "cyan" && Controls.color != "red") {
                Controls.ship = "ship_cyan";
                Controls.color = "cyan";
            }

            break;
        default:
            document.getElementById("shipSelector").innerHTML =
                '<option value="green">green</option>' +
                '<option value="orange">orange</option>' +
                '<option value="pink">pink</option>' +
                '<option value="red">red</option>' +
                '<option value="cyan">cyan</option>' +
                '<option value="yellow">yellow</option>';
            break;
    }
});

document.getElementById("spawn").addEventListener("click", function () {
    Events.Spawn();
    aliveSince = gameTime;
    connection.sendSpawn(Controls.nick, Controls.color, Controls.ship, token);
});

function startSpectate() {
    isSpectating = true;
    Events.Spectate();
    document.body.classList.add("spectating");
}

document.getElementById("spectate").addEventListener("click", function () {
    startSpectate();
});

document.addEventListener("keydown", function(e) {
    if (e.keyCode == 27 || e.which == 27) {
        isSpectating = false;
        document.body.classList.remove("spectating");
    }
});

var sizeCanvas = function() {
    var width, height;
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

window.addEventListener("resize", function() {
    sizeCanvas();
});

window.Game.Stats = {
    framesPerSecond: 0,
    viewsPerSecond: 0,
    updatesPerSecond: 0
};

var frameCounter = 0;
var viewCounter = 0;
var updateCounter = 0;
var lastCamera = { X: 0, Y: 0 };

function doPing() {
    window.Game.Stats.framesPerSecond = frameCounter;
    window.Game.Stats.viewsPerSecond = viewCounter;
    window.Game.Stats.updatesPerSecond = updateCounter;

    if (frameCounter === 0) {
        console.log("backgrounded");
        Game.isBackgrounded = true;
    }
    else
        Game.isBackgrounded = false;
    frameCounter = 0;
    viewCounter = 0;
    updateCounter = 0;
}

doPing();
setInterval(doPing, 1000);

// Game Loop
function gameLoop() {
    requestAnimationFrame(gameLoop);
    var latency = connection.minLatency || 0;
    gameTime = performance.now() + serverTimeOffset - latency / 2;
    frameCounter++;

    var position = { X: 0, Y: 0 };

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
    renderer.draw(cache, interpolator, gameTime);

    camera.end();

    lastPosition = position;

    leaderboard.draw(leaderboard.position);
    hud.draw();
    log.draw();

    if (Controls.mouseX) {
        var pos = camera.screenToWorld(Controls.mouseX, Controls.mouseY);

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
    var query = {};
    var pairs = (queryString[0] === '?' ? queryString.substr(1) : queryString).split('&');
    for (var i = 0; i < pairs.length; i++) {
        var pair = pairs[i].split('=');
        query[decodeURIComponent(pair[0])] = decodeURIComponent(pair[1] || '');
    }
    return query;
}

var query = parseQuery(window.location.search);
if (query.spectate && query.spectate !== "0") {
    startSpectate();
}
