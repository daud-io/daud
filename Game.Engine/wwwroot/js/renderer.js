(function () {
    var Renderer = function (canvas, context, settings) {
        settings = settings || {};
        this.canvas = canvas;
        this.context = context;
        this.view = false;
    };

    Renderer.prototype = {
        draw: function () {
            if (this.view && this.view.PlayerView) {
                var pv = this.view.PlayerView;
                var ctx = this.context;

                ctx.save();
                ctx.lineWidth = "40";
                ctx.strokeStyle = "green";
                ctx.rect(-1000, -1000, 2000, 2000);
                ctx.stroke();
                ctx.restore();

                for (var i = 0; i < pv.Objects.length; i++) {
                    var object = pv.Objects[i];

                    ctx.fillStyle = object.color || "green";

                    ctx.beginPath();
                    ctx.arc(object.Position.X, object.Position.Y, 10, 0, Math.PI * 2);
                    ctx.fill();
                }
            }
        }
    };

    this.Game.Renderer = Renderer;
}).call(this);
