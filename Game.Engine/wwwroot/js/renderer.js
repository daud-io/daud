(function () {
    var Renderer = function (context, settings) {
        settings = settings || {};
        this.context = context;
        this.view = false;

        var sprite = function (name) {
            var img = new Image();
            img.src = "img/" + name + ".png";

            return img;
        }

        this.sprites = {
            'ship1': sprite("ship1"),
            'ship2': sprite("ship2")
        };

        this.ship = new Image();
        this.ship.src = "img/ship2.png";
    };

    Renderer.prototype = {
        draw: function (interpolator, currentTime) {
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

                    var ship = this.sprites[object.Sprite]
                    if (!ship)
                        ship = this.sprites["ship1"];

                    var width = ship.width;
                    var height = ship.height;

                    var position = interpolator.projectObject(object, currentTime);

                    ctx.save();
                    ctx.translate(position.X, position.Y);
                    ctx.rotate(object.Angle);
                    ctx.drawImage(ship, -width / 2, -height / 2, width, height);
                    ctx.restore();
                }
            }
        }
    };

    this.Game.Renderer = Renderer;
}).call(this);
