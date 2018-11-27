export class Renderer {
    constructor(context, settings) {
        settings = settings || {};
        this.context = context;
        this.view = false;
        this.theme="default";

        var sprite = function(name, scale, scaleToSize) {
            var img = new Image();
            img.src = "img/" + name + ".png";

            return {
                image: img,
                scale: scale || 1.3,
                scaleToSize: scaleToSize || false
            };
        };

        var flagScale = 0.005;

        Renderer.sprites = {
            ship0: {default:sprite("variants/default/ship0"),bitty:sprite("variants/bitty/ship0",2)},
            ship_green: {default:sprite("variants/default/ship_green"),bitty:sprite("variants/bitty/ship_green",2)},
            ship_gray: {default:sprite("variants/default/ship_gray"),bitty:sprite("variants/bitty/ship_gray",2)},
            ship_orange: {default:sprite("variants/default/ship_orange"),bitty:sprite("variants/bitty/ship_orange",2)},
            ship_pink: {default:sprite("variants/default/ship_pink"),bitty:sprite("variants/bitty/ship_pink",2)},
            ship_red: {default:sprite("variants/default/ship_red"),bitty:sprite("variants/bitty/ship_red",2)},
            ship_cyan: {default:sprite("variants/default/ship_cyan"),bitty:sprite("variants/bitty/ship_cyan",2)},
            ship_yellow: {default:sprite("variants/default/ship_yellow"),bitty:sprite("variants/bitty/ship_yellow",2)},
            ship_flash: {default:sprite("variants/default/ship_flash"),bitty:sprite("variants/bitty/ship_flash",2)},
            bullet_green: {default:sprite("variants/default/bullet_green", 0.03, true),bitty:sprite("variants/bitty/bullet_green", 0.125, true)},
            bullet_orange: {default:sprite("variants/default/bullet_orange", 0.03, true),bitty:sprite("variants/bitty/bullet_orange", 0.125, true)},
            bullet_pink:  {default:sprite("variants/default/bullet_pink", 0.03, true),bitty:sprite("variants/bitty/bullet_pink", 0.125, true)},
            bullet_red: {default:sprite("variants/default/bullet_red", 0.03, true),bitty:sprite("variants/bitty/bullet_red", 0.125, true)},
            bullet_cyan: {default:sprite("variants/default/bullet_cyan", 0.03, true),bitty:sprite("variants/bitty/bullet_cyan", 0.125, true)},
            bullet_yellow: {default:sprite("variants/default/bullet_yellow", 0.03, true),bitty:sprite("variants/bitty/bullet_yellow", 0.125, true)},
            fish: {default:sprite("variants/default/ship0",0.005,true),bitty:sprite("variants/bitty/ship0",0.01,true)},
            bullet: {default:sprite("variants/default/bullet", 0.02, true),bitty:sprite("variants/bitty/bullet", 0.0815, true)},
            seeker: {default:sprite("variants/default/seeker", 0.02, true),bitty:sprite("variants/bitty/seeker", 0.02, true)},
            seeker_pickup: {default:sprite("variants/default/seeker_pickup", 0.02, true),bitty:sprite("variants/bitty/seeker_pickup", 0.02, true)},
            obstacle: {default:sprite("variants/default/obstacle", 0.0028, true),bitty:sprite("variants/bitty/obstacle", 0.0028, true)},
            arrow: sprite("arrow", 0.03),
            flag_blue_0: sprite("flag_blue_0", flagScale, true),
            flag_blue_1: sprite("flag_blue_1", flagScale, true),
            flag_blue_2: sprite("flag_blue_2", flagScale, true),
            flag_blue_3: sprite("flag_blue_3", flagScale, true),
            flag_blue_4: sprite("flag_blue_4", flagScale, true),
            flag_red_0: sprite("flag_red_0", flagScale, true),
            flag_red_1: sprite("flag_red_1", flagScale, true),
            flag_red_2: sprite("flag_red_2", flagScale, true),
            flag_red_3: sprite("flag_red_3", flagScale, true),
            flag_red_4: sprite("flag_red_4", flagScale, true),
            ctf_base: sprite("ctf_base")
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
            "arrow",
            "flag_blue_0",
            "flag_blue_1",
            "flag_blue_2",
            "flag_blue_3",
            "flag_blue_4",
            "flag_red_0",
            "flag_red_1",
            "flag_red_2",
            "flag_red_3",
            "flag_red_4",
            "ctf_base"
        ];
    }
    
    setTheme(theme){
        this.theme=theme;
    }

    getSprite(key) {
        var res=Renderer.sprites[key];
        if(res && res.default){
            return res[this.theme]?res[this.theme]:res.default;
        }
        return res;
    }

    draw(cache, interpolator, currentTime) {
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
            ctx.rect(-worldSize, -worldSize, 2 * worldSize, 2 * worldSize);
            ctx.stroke();

            ctx.beginPath();
            ctx.lineWidth = edgeWidth * 2;
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

            cache.foreach(function(body) {
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
                            group: cache.groups["g-" + object.Group],
                            points: []
                        };

                        groupsUsed.push(group);
                    }

                    group.points.push(position);
                }

                var ship = object.Sprite != null ? this.getSprite(object.Sprite) : false;

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
                    var shipWidth = ship.image.width;
                    var shipHeight = ship.image.height;

                    ctx.rotate(position.Angle);
                    ctx.scale(ship.scale, ship.scale);

                    if (ship.scaleToSize) ctx.scale(object.Size, object.Size);

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

            cache.foreach(function(body) {
                var position = interpolator.projectObject(body, currentTime);
                if (body.Caption) {
                    ctx.fillText(body.Caption, position.X, position.Y + 90);
                }
            }, this);
        }
    }

    colorValue(colorName) {
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
}
