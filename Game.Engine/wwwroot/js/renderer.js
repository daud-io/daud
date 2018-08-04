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
            'ship0': sprite("ship0"),
            'ship_gray': sprite("ship_gray"),
            'ship_orange': sprite("ship_orange"),
            'ship_pink': sprite("ship_pink"),
            'ship_red': sprite("ship_red"),
            'ship_cyan': sprite("ship_cyan"),
            'ship_yellow': sprite("ship_yellow"),
            'bullet': sprite("torpedo")
        };
    };

    Renderer.prototype = {
        draw: function (interpolator, currentTime) {
            if (this.view && this.view.PlayerView) {
                var pv = this.view.PlayerView;
                var ctx = this.context;

                // edge of the universe
                ctx.save();
                ctx.beginPath();
                ctx.lineWidth = 40;
                ctx.strokeStyle = "blue";
                ctx.rect(-3000, -3000, 6000, 6000);
                ctx.stroke();
                ctx.restore();

                
                ctx.font = "24px sans-serif";
                ctx.fillStyle = "white";
                ctx.textAlign = "center";
                ctx.strokeStyle = "white";
                ctx.lineWidth = 6;
                
                for (var i = 0; i < pv.Objects.length; i++) {
                    var object = pv.Objects[i];

                    var ship = this.sprites[object.Sprite]
                    if (!ship)
                        ship = this.sprites["ship_gray"];

                    var width = ship.width;
                    var height = ship.height;

                    var position = interpolator.projectObject(object, currentTime);

                    ctx.save();
                    ctx.translate(position.X, position.Y);
                    ctx.rotate(object.Angle);
                    ctx.scale(1.3, 1.3);
                    ctx.drawImage(ship, -width / 2, -height / 2, width, height);
                    ctx.restore();

                    if (object.Caption) {
                        ctx.fillText(object.Caption, position.X, position.Y + 90);
                    }
                    if (object.Health) {
                        var offset = { X: 0, Y: 100 };
                        var width = 200;
                        var height = 30;

                        ctx.beginPath();
                        ctx.rect(position.X + offset.X - width / 2, position.Y + offset.Y + height, width, height);
                        ctx.stroke();
                        ctx.fillRect(position.X + offset.X - width / 2, position.Y + offset.Y + height, width * object.Health, height);
                    }
                }
            }
        }
    };

    this.Game.Renderer = Renderer;
}).call(this);
