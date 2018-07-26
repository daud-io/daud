(function () {
    var Renderer = function (context, settings) {
        settings = settings || {};
        this.context = context;
        this.view = false;
    };

    Renderer.prototype = {
        draw: function () {
            if (this.view && this.view.PlayerView) {
                var pv = this.view.PlayerView;
                var ctx = this.context;

                ctx.save();
                ctx.beginPath();
                ctx.lineWidth = 40;
                ctx.strokeStyle = "blue";
                ctx.rect(-1000, -1000, 2000, 2000);
                ctx.stroke();
                ctx.restore();

                for (var i = 0; i < pv.Objects.length; i++) {
                    var object = pv.Objects[i];

                    ctx.lineWidth = 1;
                    ctx.fillStyle = object.color || "red";

                    ctx.beginPath();
                    ctx.arc(object.Position.X, object.Position.Y, 20, 0, Math.PI * 2);
                    ctx.fill();
                }
            }
        }
    };

    this.Game.Renderer = Renderer;
}).call(this);
