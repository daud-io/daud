(function () {
    // prepare our game canvas
    var canvas = document.getElementById("gameCanvas");
    var context = canvas.getContext("2d");
    var renderer = new Game.Renderer(context, {});
    var background = new Game.Background(canvas, context, {});
    var renderFrame = requestAnimationFrame;
    var camera = new Game.Camera(context);
    var interpolator = new Game.Interpolator();

    Game.Controls.registerCanvas(canvas);

    var view = false;
    var lastFrameTime = false;

    var connection = new Game.Connection();
    connection.onView = function (newView) {
        view = newView;
        interpolator.newFrame();
        lastFrameTime = performance.now();

        connection.sendControl(
            angle,
            Game.Controls.boost,
            Game.Controls.shoot,
            Game.Controls.nick,
            Game.Controls.ship
        );

    };

    var angle = 0.0;

    // Game Loop
    var gameLoop = function () {
        var currentTime = performance.now();

        //console.log('game');
        context.clearRect(0, 0, canvas.width, canvas.height);

        if (view && view.PlayerView) {
            var pv = view.PlayerView;

            var position = interpolator.projectObject(pv, currentTime);

            camera.moveTo(position.X, position.Y);
            camera.zoomTo(1000);
        }

        camera.begin();
        background.draw();

        renderer.view = view;
        renderer.draw(interpolator, currentTime);
        camera.end();


        if (Game.Controls.mouseX) {
            var cx = canvas.width / 2;
            var cy = canvas.height / 2;
            var dy = Game.Controls.mouseY - cy
            var dx = Game.Controls.mouseX - cx


            var theta = Math.atan2(dy, dx);
            //console.log([Game.Controls.mouseX, Game.Controls.mouseY, theta]);

            angle = theta;
        }

        /*
        if (Game.Controls.left || Game.Controls.up)
            angle -= 0.1;
        if (Game.Controls.right || Game.Controls.down)
            angle += 0.1;
        */


        renderFrame(gameLoop);
    }

    renderFrame(gameLoop);
})();
