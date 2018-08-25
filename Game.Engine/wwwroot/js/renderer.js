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
        draw: function (cache, interpolator, currentTime) {
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

                cache.foreach(function (body) {
                    var object = body;

                    var ship = object.Sprite != null
                        ? this.sprites[object.Sprite]
                        : false;

                    if (!ship)
                        ship = this.sprites["ship_gray"];

                    var width = ship.width;
                    var height = ship.height;

                    var position = interpolator.projectObject(object, currentTime);

                    if (object.Caption) {
                        ctx.fillText(object.Caption, position.X, position.Y + 90);
                    }

                    ctx.save();
                    ctx.fillStyle = "rgba(0,255,0,0.2)";

                    var health = (object.Size) / 150;
                    if (health) {
                        var healthBar = false;
                        var healthRing = true;

                        if (healthBar) {
                            var offset = { X: 0, Y: 100 };
                            var width = 200;
                            var height = 30;

                            ctx.beginPath();
                            ctx.rect(position.X + offset.X - width / 2, position.Y + offset.Y + height, width, height);
                            ctx.stroke();
                            ctx.fillRect(position.X + offset.X - width / 2, position.Y + offset.Y + height, width * health, height);
                        }

                        if (healthRing) {

                            /*if (health < 0.33)
                                ctx.fillStyle = "rgba(255, 128, 128, 0.2)";
                            else if (health < 0.66)
                                ctx.fillStyle = "rgba(128, 128, 255, 0.5)";
                            else 
                                ctx.fillStyle = "rgba(0, 255, 0, 0.2)";*/

                            ctx.fillStyle = object.Color;

                            ctx.beginPath();
                            ctx.arc(position.X, position.Y, 60 + 90.0 * health, 0, 2 * Math.PI, false);
                            ctx.arc(position.X, position.Y, 60, 0, 2 * Math.PI, true);
                            ctx.fill();
                        }
                    }
                    ctx.restore();

                    ctx.save();
                    ctx.translate(position.X, position.Y);
                    ctx.rotate(object.Angle);
                    ctx.scale(1.3, 1.3);
                    ctx.drawImage(ship, -width / 2, -height / 2, width, height);
                    ctx.restore();

                }, this);
            }
        }
    };

    this.Game.Renderer = Renderer;
}).call(this);
