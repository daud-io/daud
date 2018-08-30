(function () {
    var lastTime = 0,
        vendors = ['ms', 'moz', 'webkit', 'o'],
        x,
        length,
        currTime,
        timeToCall;

    for (x = 0, length = vendors.length; x < length && !window.requestAnimationFrame; ++x) {
        window.requestAnimationFrame = window[vendors[x] + 'RequestAnimationFrame'];
        window.cancelAnimationFrame =
            window[vendors[x] + 'CancelAnimationFrame'] || window[vendors[x] + 'CancelRequestAnimationFrame'];
    }

    if (!window.requestAnimationFrame)
        window.requestAnimationFrame = function (callback, element) {
            currTime = new Date().getTime();
            timeToCall = Math.max(0, 16 - (currTime - lastTime));
            lastTime = currTime + timeToCall;
            return window.setTimeout(function () { callback(currTime + timeToCall); },
                timeToCall);
        };

    if (!window.cancelAnimationFrame)
        window.cancelAnimationFrame = function (id) {
            clearTimeout(id);
        };
}());

(function () {
    var canvas = document.getElementById("gameCanvas");
    var context = canvas.getContext("2d");
    var renderer = new Game.Renderer(context, {});
    var background = new Game.Background(canvas, context, {});
    var camera = new Game.Camera(context);
    var interpolator = new Game.Interpolator();
    var leaderboard = new Game.Leaderboard(canvas, context);
    var angle = 0.0;

    Game.Controls.registerCanvas(canvas);

    var cache = new Game.Cache();
    var view = false;
    var lastFrameTime = false;
    var serverTimeOffset = 0;

    var log = function (message) {
        document.getElementById('log').prepend(document.createTextNode(message + '\n'));
    };

    var connection = new Game.Connection();
    window.Game.primaryConnection = connection;

    var bodyFromServer = function (body) {

        var originalPosition = body.originalPosition();
        var momentum = body.momentum();

        var newBody = {
            ID: body.id(),
            DefinitionTime: body.definitionTime().toFloat64(),
            Size: body.size(),
            Sprite: body.sprite(),
            Color: body.color(),
            Caption: body.caption(),
            Angle: body.angle(),
            Momentum: {
                X: momentum.x(),
                Y: momentum.y()
            },
            OriginalPosition: {

                X: originalPosition.x(),
                Y: originalPosition.y()
            }
        };

        return newBody;
    }

    connection.onLeaderboard = function (lb) {
        //console.log('new leaderboard');
        leaderboard.setData(lb);
    };

    connection.onView = function (newView) {
        viewCounter++;

        view = {};
        lastFrameTime = performance.now();
        view.time = newView.time().toFloat64()

        view.isAlive = newView.isAlive();

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

        serverTimeOffset = view.time - lastFrameTime;

        var updatesLength = newView.updatesLength();
        var updates = [];
        for (var i = 0; i < updatesLength; i++) {
            var update = newView.updates(i);

            updates.push(bodyFromServer(update));
        }

        updateCounter += updatesLength;

        var deletes = [];
        var deletesLength = newView.deletesLength();
        for (var i = 0; i < deletesLength; i++)
            deletes.push(newView.deletes(i));
            
        cache.update(updates, deletes);

        view.camera = bodyFromServer(newView.camera());
    };

    var lastControl = {};

    setInterval(function () {
        if (
            angle != lastControl.angle
            || Game.Controls.boost != lastControl.boost
            || Game.Controls.shoot != lastControl.shoot
            || Game.Controls.nick != lastControl.nick
            || Game.Controls.ship != lastControl.ship
            || Game.Controls.color != lastControl.color
        ) {
            connection.sendControl(
                angle,
                Game.Controls.boost,
                Game.Controls.shoot,
                Game.Controls.nick,
                Game.Controls.ship,
                Game.Controls.color
            );

            lastControl = {
                angle: angle,
                boost: Game.Controls.boost,
                shoot: Game.Controls.shoot,
                nick: Game.Controls.nick,
                ship: Game.Controls.ship,
                color: Game.Controls.color
            };
        }
    }, 10);

    document.getElementById('spawn').addEventListener("click", function () {
        connection.sendSpawn(Game.Controls.nick, Game.Controls.color, Game.Controls.ship);
    });


    var sizeCanvas = function () {
        var width = window.innerWidth;
        var height = width * 9 / 16;

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

    setInterval(function () {
        Game.Stats.framesPerSecond = frameCounter;
        Game.Stats.viewsPerSecond = viewCounter;
        Game.Stats.updatesPerSecond = updateCounter;

        if (frameCounter == 0) {
            console.log('backgrounded');
        }
        frameCounter = 0;
        viewCounter = 0;
        updateCounter = 0;
    }, 1000);

    // Game Loop
    function gameLoop() {
        requestAnimationFrame(gameLoop);
        var currentTime = performance.now();
        frameCounter++;

        if (view) {
            var position = interpolator.projectObject(view.camera, currentTime + serverTimeOffset);

            camera.moveTo(position.X, position.Y);
            camera.zoomTo(5000);
        }

        camera.begin();
        background.draw();

        renderer.view = view;
        renderer.draw(cache, interpolator, currentTime + serverTimeOffset);

        camera.end();

        leaderboard.draw();

        if (Game.Controls.mouseX) {
            var cx = canvas.width / 2;
            var cy = canvas.height / 2;
            var dy = Game.Controls.mouseY - cy
            var dx = Game.Controls.mouseX - cx

            angle = Math.atan2(dy, dx);
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
