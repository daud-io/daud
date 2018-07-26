(function () {
    // prepare our game canvas
    var canvas = document.getElementById("gameCanvas");
    var context = canvas.getContext("2d");
    var renderer = new Game.Renderer(canvas, context, {});
    var renderFrame = requestAnimationFrame;
    var camera = new Game.Camera(context);

    var view = false;

    var connection = new Game.Connection();
    connection.onView = function (newView) {
        view = newView;
    };

    // Game Loop
    var gameLoop = function () {
        //console.log('game');
        context.clearRect(0, 0, canvas.width, canvas.height);

        if (view && view.PlayerView) {
            var pv = view.PlayerView;

            camera.moveTo(pv.Position.X, pv.Position.Y);
            camera.zoomTo(5000);
        }

        camera.begin();
        renderer.view = view;
        renderer.draw();
        camera.end();

        renderFrame(gameLoop);
    }

    renderFrame(gameLoop);
})();
