(function () {
    var canvas = document.getElementById("gameCanvas");
    var context = canvas.getContext("2d");
    var renderer = new Game.Renderer(context, {});
    var background = new Game.Background(canvas, context, {});
    var camera = new Game.Camera(context);
    var interpolator = new Game.Interpolator();
    var leaderboard = new Game.Leaderboard(canvas, context);
    var angle = 0.0;
    var aimTarget = { X: 0, Y: 0 };

    var cache = new Game.Cache();
    var view = false;
    var serverTimeOffset = false;
    var lastOffset = false;
    var gameTime = false;
    var lastPosition = false;

    /* var lagDetectionTimes = [1000, 3000, 10000];
    var scheduleLagCheck = function (delay) {
        setTimeout(function () { serverTimeOffset = lastOffset; }, delay);
    };*/

    Game.Controls.registerCanvas(canvas);

    var log = function (message) {
        document.getElementById('log').prepend(document.createTextNode(message + '\n'));
    };

    var connection = new Game.Connection();
    window.Game.primaryConnection = connection;

    var bodyFromServer = function (cache, body) {

        var originalPosition = body.originalPosition();
        var momentum = body.velocity();
        var group = cache.getGroup(body.group());
        var groupID = (group && group.ID) || 0;

        var newBody = {
            ID: body.id(),
            DefinitionTime: body.definitionTime(),
            Size: body.size() * 5,
            Sprite: Game.Renderer.spriteIndices[body.sprite()], //body.sprite(),
            Color: 'red', //body.color(),
            Group: groupID,
            OriginalAngle: body.originalAngle() / 127 * Math.PI,
            AngularVelocity: body.angularVelocity() / 127 * Math.PI / 1000,
            Momentum: {
                X: momentum.x() / 1000,
                Y: momentum.y() / 1000
            },
            OriginalPosition: {
                X: originalPosition.x(),
                Y: originalPosition.y()
            }
        };

        if (newBody.Sprite == "seeker_pickup")
            console.log(newBody);

        return newBody;
    };

    var groupFromServer = function (cache, group) {

        var newGroup = {
            ID: group.group(),
            Caption: group.caption(),
            Type: group.type(),
        };

        return newGroup;
    };

    connection.onConnected = function () {
        /*for (var i = 0; i < lagDetectionTimes.length; i++)
            scheduleLagCheck(lagDetectionTimes[i]);*/
    };

    connection.onLeaderboard = function (lb) {
        //console.log('new leaderboard');
        leaderboard.setData(lb);
        leaderboard.position = lastPosition;
    };

    connection.onView = function (newView) {
        viewCounter++;

        view = {};
        view.time = newView.time();

        view.isAlive = newView.isAlive();

        // this is probably very slow and should be optimized
        $(document.body).remove("loading");
        if (view.isAlive) {
            $(document.body)
                .removeClass('dead')
                .addClass('alive');
        } else {
            $(document.body)
                .removeClass('alive')
                .addClass('dead');
        }

        lastOffset = view.time - performance.now();
        if (lastOffset > serverTimeOffset)
            serverTimeOffset = lastOffset;

        if (serverTimeOffset === false)
            serverTimeOffset = lastOffset;

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


        updateCounter += updatesLength;

        var deletes = [];
        var deletesLength = newView.deletesLength();
        for (var d = 0; d < deletesLength; d++)
            deletes.push(newView.deletes(d));

        var groupDeletes = [];
        var groupDeletesLength = newView.groupDeletesLength();
        for (var d = 0; d < groupDeletesLength; d++)
            groupDeletes.push(newView.groupDeletes(d));

        cache.update(updates, deletes, groups, groupDeletes, gameTime);

        view.camera = bodyFromServer(cache, newView.camera());
    };

    var lastControl = {};

    setInterval(function () {

        if (
            angle !== lastControl.angle
            || aimTarget.X !== aimTarget.X
            || aimTarget.Y !== aimTarget.Y
            || Game.Controls.boost !== lastControl.boost
            || Game.Controls.shoot !== lastControl.shoot
        ) {
            connection.sendControl(
                angle,
                Game.Controls.boost,
                Game.Controls.shoot,
                aimTarget.X,
                aimTarget.Y
            );

            lastControl = {
                angle: angle,
                aimTarget: aimTarget,
                boost: Game.Controls.boost,
                shoot: Game.Controls.shoot
            };
        }
    }, 10);

    document.getElementById('spawn').addEventListener("click", function () {
        connection.sendSpawn(Game.Controls.nick, Game.Controls.color, Game.Controls.ship);
    });

    document.getElementById('spectate').addEventListener("click", function () {
        $(document.body).addClass('spectating');
    });

    $(document).on('keydown', function (e) {
        if (e.keyCode == 27 || e.which == 27)
            $(document.body).removeClass('spectating');
    });



    var sizeCanvas = function () {
        var width, height
        if (window.innerWidth * 9 / 16 < window.innerHeight) {
            width = window.innerWidth;
            height = width * 9 / 16;
        } else {
            height = window.innerHeight;
            width = height * 16 / 9;
        }

        canvas.width = width;
        canvas.height = height;

        /*$('#panel').css('right', canvas.width - 10);
        $('#panel').css('left', canvas.width - 10 - 300);
        $('#panel').css('bottom', canvas.height - 10);*/

    };

    sizeCanvas();

    window.addEventListener("resize", function () {
        sizeCanvas();
    });

    Game.Stats = {
        framesPerSecond: 0,
        viewsPerSecond: 0,
        updatesPerSecond: 0
    };

    var frameCounter = 0;
    var viewCounter = 0;
    var updateCounter = 0;
    var lastCamera = { X: 0, Y: 0 };

    setInterval(function () {
        Game.Stats.framesPerSecond = frameCounter;
        Game.Stats.viewsPerSecond = viewCounter;
        Game.Stats.updatesPerSecond = updateCounter;

        if (frameCounter === 0) {
            console.log('backgrounded');
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
            position.X = (position.X * 0.2 + lastCamera.X * 0.8);
            position.Y = (position.Y * 0.2 + lastCamera.Y * 0.8);

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

        if (Game.Controls.mouseX) {

            var cx = canvas.width / 2;
            var cy = canvas.height / 2;
            var dy = Game.Controls.mouseY - cy;
            var dx = Game.Controls.mouseX - cx;

            var pos = camera.screenToWorld(Game.Controls.mouseX, Game.Controls.mouseY);
            /*console.log({
                X: position.X - pos.x,
                Y: position.Y - pos.y
            });*/

            if (isNaN(pos.x) || isNaN(position.X)) {
                var i = 0;
            }

            angle = Math.atan2(dy, dx);
            aimTarget = {
                X: pos.x - position.X,
                Y: pos.y - position.Y
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

})();
