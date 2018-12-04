import "babel-polyfill";

import { Renderer, spriteIndices } from "./renderer";
import { Camera } from "./camera";
import { Cache } from "./cache";
import { Interpolator } from "./interpolator";
import { Leaderboard } from "./leaderboard";
import { Log } from "./log";
import { Background } from "./background";
import { Controls } from "./controls";
import { Connection } from "./connection";
import { token } from "./discord";
import { Settings } from "./settings";

var canvas = document.getElementById("gameCanvas");
var context = canvas.getContext("2d");
var renderer = new Renderer(context, {});
var background = new Background(canvas, context, {});
var camera = new Camera(context);
var interpolator = new Interpolator();
var leaderboard = new Leaderboard(canvas, context);
var log = new Log(canvas, context);

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
window.Game.primaryConnection = connection;

var bodyFromServer = function(cache, body) {
    var originalPosition = body.originalPosition();
    var momentum = body.velocity();
    var group = cache.getGroup(body.group());
    var groupID = (group && group.ID) || 0;

    var newBody = {
        ID: body.id(),
        DefinitionTime: body.definitionTime(),
        Size: body.size() * 5,
        Sprite: spriteIndices[body.sprite()], //body.sprite(),
        Color: "red", //body.color(),
        Group: groupID,
        OriginalAngle: (body.originalAngle() / 127) * Math.PI,
        AngularVelocity: ((body.angularVelocity() / 127) * Math.PI) / 1000,
        Momentum: {
            X: momentum.x() / 10000,
            Y: momentum.y() / 10000
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

connection.onView = function(newView) {
    viewCounter++;

    view = {};
    view.time = newView.time();

    view.isAlive = newView.isAlive();

    // this is probably very slow and should be optimized
    document.body.classList.remove("loading");
    if (view.isAlive) {
        document.body.classList.remove("dead");
        document.body.classList.add("alive");
    } else {
        document.body.classList.remove("alive");
        document.body.classList.add("dead");
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

    view.camera = bodyFromServer(cache, newView.camera());
};

var lastControl = {};

setInterval(function() {
    if (angle !== lastControl.angle || aimTarget.X !== aimTarget.X || aimTarget.Y !== aimTarget.Y || Controls.boost !== lastControl.boost || Controls.shoot !== lastControl.shoot) {
        connection.sendControl(angle, Controls.boost, Controls.shoot, aimTarget.X, aimTarget.Y);

        lastControl = {
            angle: angle,
            aimTarget: aimTarget,
            boost: Controls.boost,
            shoot: Controls.shoot
        };
    }
}, 10);

document.getElementById("spawn").addEventListener("click", function() {
    connection.sendSpawn(Controls.nick, Controls.color, Controls.ship, token);
});

document.getElementById("spectate").addEventListener("click", function() {
    document.body.classList.add("spectating");
});

document.addEventListener("keydown", function(e) {
    if (e.keyCode == 27 || e.which == 27) document.body.classList.remove("spectating");
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

setInterval(function() {
    window.Game.Stats.framesPerSecond = frameCounter;
    window.Game.Stats.viewsPerSecond = viewCounter;
    window.Game.Stats.updatesPerSecond = updateCounter;

    if (frameCounter === 0) {
        console.log("backgrounded");
    }
    frameCounter = 0;
    viewCounter = 0;
    updateCounter = 0;
}, 1000);

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
        camera.zoomTo(5000);
    }

    camera.begin();
    background.draw(position.X, position.Y);
    renderer.view = view;
    renderer.draw(cache, interpolator, gameTime);

    camera.end();

    lastPosition = position;

    leaderboard.draw(leaderboard.position);
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
