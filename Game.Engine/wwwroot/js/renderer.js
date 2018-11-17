(function () {
    var Renderer = function (context, settings) {
        settings = settings || {};
        this.context = context;
        this.view = false;

        var sprite = function (name, scale, scaleToSize) {
            var img = new Image();
            img.src = "img/" + name + ".png";

            return {
                image: img,
                scale: scale || 1.3,
                scaleToSize: scaleToSize || false
            }
        }

        Renderer.sprites = {
            'ship0': sprite("ship0"),
            'ship_green': sprite("ship_green"),
            'ship_gray': sprite("ship_gray"),
            'ship_orange': sprite("ship_orange"),
            'ship_pink': sprite("ship_pink"),
            'ship_red': sprite("ship_red"),
            'ship_cyan': sprite("ship_cyan"),
            'ship_yellow': sprite("ship_yellow"),
            'ship_flash': sprite("ship_flash"),
            'bullet_green': sprite("bullet_green", 0.03, true),
            'bullet_orange': sprite("bullet_orange", 0.03, true),
            'bullet_pink': sprite("bullet_pink", 0.03, true),
            'bullet_red': sprite("bullet_red", 0.03, true),
            'bullet_cyan': sprite("bullet_cyan", 0.03, true),
            'bullet_yellow': sprite("bullet_yellow", 0.03, true),
            'fish': sprite("ship0", .005, true),
            'bullet': sprite("bullet", 0.02, true),
            'seeker': sprite("seeker", 0.02, true),
            'seeker_pickup': sprite("seeker_pickup", 0.02, true),
            'obstacle': sprite("obstacle", 0.0028, true),
            'arrow': sprite("arrow", 0.03)
        };

        Renderer.spriteIndices = [
            "none",
            "ship0",
            "ship_green",
            "ship_gray",
            "ship_orange",
            "ship_pink",
            "ship_red",
            "ship_cyan",
            "ship_yellow",
            "ship_flash",
            "bullet_green",
            "bullet_orange",
            "bullet_pink",
            "bullet_red",
            "bullet_cyan",
            "bullet_yellow",
            "fish",
            "bullet",
            "seeker",
            "seeker_pickup",
            "obstacle",
            "arrow"
        ];
    };

    Renderer.prototype = {
        draw: function (cache, interpolator, currentTime) {
            if (this.view) {
                var pv = this.view;
                var ctx = this.context;

                // edge of the universe
                ctx.save();

                var worldSize = 6000;
                var edgeWidth = 4000;

                ctx.beginPath();
                ctx.lineWidth = 40;
                ctx.strokeStyle = "blue";
                ctx.rect(-worldSize, -worldSize, 2*worldSize, 2*worldSize);
                ctx.stroke();

                ctx.beginPath();
                ctx.lineWidth = edgeWidth*2;
                ctx.strokeStyle = "rgba(255,0,0,0.1)";
                ctx.rect(-worldSize - edgeWidth, -worldSize - edgeWidth, 2 * worldSize + 2 * edgeWidth, 2 * worldSize + 2 * edgeWidth);
                ctx.stroke();

                ctx.restore();
                
                ctx.font = "48px sans-serif";
                ctx.fillStyle = "white";
                ctx.textAlign = "center";
                ctx.strokeStyle = "white";
                ctx.lineWidth = 6;

                var groupsUsed = [];

                cache.foreach(function (body) {
                    var object = body;

                    var position = interpolator.projectObject(object, currentTime);

                    if (object.Group) {

                        var group = false;
                        for (var i = 0; i < groupsUsed.length; i++)
                            if (groupsUsed[i].id == object.Group) {
                                group = groupsUsed[i];
                                break;
                            }

                        if (!group) {
                            group = {
                                id: object.Group,
                                group: cache.groups['g-' + object.Group],
                                points: []
                            };

                            groupsUsed.push(group);
                        }

                        group.points.push(position);
                    }

                    var ship = object.Sprite != null
                        ? Renderer.sprites[object.Sprite]
                        : false;
                    
                    /*if (object.Caption) {
                        ctx.fillText(object.Caption, position.X, position.Y + 90);
                    }*/

                    ctx.save();
                    ctx.fillStyle = "rgba(0,255,0,0.2)";

                    var health = object.Size;
                    if (health) {
                        var healthBar = false;
                        var healthRing = false;

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

                            ctx.fillStyle = this.colorValue(object.Color);

                            ctx.beginPath();
                            ctx.arc(position.X, position.Y, health, 0, 2 * Math.PI, false);
                            //ctx.arc(position.X, position.Y, 60, 0, 2 * Math.PI, true);
                            ctx.fill();
                        }
                    }
                    ctx.restore();

                    ctx.save();
                    ctx.translate(position.X, position.Y);

                    if (ship) {
                        shipWidth = ship.image.width;
                        shipHeight = ship.image.height;

                        ctx.rotate(position.Angle);
                        ctx.scale(ship.scale, ship.scale);

                        if (ship.scaleToSize)
                            ctx.scale(object.Size, object.Size);

                        ctx.drawImage(ship.image, -shipWidth / 2, -shipHeight / 2, shipWidth, shipHeight);
                    }

                    ctx.restore();

                }, this);

                for (var i = 0; i < groupsUsed.length; i++) {
                    var group = groupsUsed[i];

                    if (group && group.group) {
                        var pt = { X: 0, Y: 0 };

                        for (var x = 0; x < group.points.length; x++) {
                            pt.X += group.points[x].X;
                            pt.Y += group.points[x].Y;
                        }

                        pt.X /= group.points.length;
                        pt.Y /= group.points.length;
                    
                        ctx.fillText(group.group.Caption, pt.X, pt.Y + 90);
                    }

                }

                cache.foreach(function (body) {
                    var position = interpolator.projectObject(body, currentTime);
                    if (body.Caption) {
                        ctx.fillText(body.Caption, position.X, position.Y + 90);
                    }
                }, this);
            }
        },
        colorValue: function (colorName) {
            switch (colorName) {
                case "cyan":
                    return "rgba(0,255,255,.2)";
                case "gray":
                    return "rgba(128,128,128,.2)";
                case "green":
                    return "rgba(0,255,0,.2)";
                case "orange":
                    return "rgba(255,140,0,.2)";
                case "pink":
                    return "rgba(255,105,180,.2)";
                case "red":
                    return "rgba(255,0,0,.2)";
                case "yellow":
                    return "rgba(255,255,0,.2)";
            }
        }
    };

    this.Game.Renderer = Renderer;
}).call(this);
