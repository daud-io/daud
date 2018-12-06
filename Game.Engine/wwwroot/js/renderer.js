import { Settings } from "./settings";

function sprite(name, scale, scaleToSize) {
    var img = new Image();
    img.src = "img/" + name + ".png";

    return {
        image: img,
        scale: scale || 1.3,
        scaleToSize: scaleToSize || false
    };
}

var flagScale = 0.005;
export var sprites = {
    ship0: sprite("ship0"),
    ship_green: sprite("ship_green"),
    ship_gray: sprite("ship_gray"),
    ship_orange: sprite("ship_orange"),
    ship_pink: sprite("ship_pink"),
    ship_red: sprite("ship_red"),
    ship_cyan: sprite("ship_cyan"),
    ship_yellow: sprite("ship_yellow"),
    ship_flash: sprite("ship_flash"),
    ship_secret: sprite("ship_secret"),
    ship_zed: sprite("ship_zed"),
    bullet_green: sprite("bullet_green", 0.03, true),
    bullet_orange: sprite("bullet_orange", 0.03, true),
    bullet_pink: sprite("bullet_pink", 0.03, true),
    bullet_red: sprite("bullet_red", 0.03, true),
    bullet_cyan: sprite("bullet_cyan", 0.03, true),
    bullet_yellow: sprite("bullet_yellow", 0.03, true),
    fish: sprite("ship0", 0.005, true),
    bullet: sprite("bullet", 0.02, true),
    seeker: sprite("seeker", 0.02, true),
    seeker_pickup: sprite("seeker_pickup", 0.02, true),
    obstacle: sprite("obstacle", 0.0028, true),
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

export var spriteIndices = [
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
    "ship_secret",
    "ship_zed",
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
export class Renderer {
    constructor(context, settings) {
        settings = settings || {};
        this.context = context;
        this.view = false;
    }

    draw(cache, interpolator, currentTime) {
        if (this.view) {
            var pv = this.view;
            var ctx = this.context;

            // Draw the edge of the universe
            ctx.save();

            var worldSize = 6000;
            var edgeWidth = 4000;

            // draw blue border at the edge of the world
            ctx.beginPath();
            ctx.lineWidth = 40;
            ctx.strokeStyle = "blue";
            ctx.rect(-worldSize, -worldSize, 2 * worldSize, 2 * worldSize);
            ctx.stroke();


            // draw red transparent buffer outside the edge of the world
            /*  
             *  ________________
             * |       top      |
             * |________________|
             * |   |        |   |
             * |   |        |   |
             * | L |        | R |
             * |___|________|___|
             * |     bottom     |
             * |________________|
             * 
             */

            ctx.fillStyle = "rgba(255,0,0,0.1)";

            // top
            ctx.fillRect(-worldSize - edgeWidth, -worldSize - edgeWidth, 2*worldSize + 2*edgeWidth, edgeWidth);

            // left
            ctx.fillRect(-worldSize - edgeWidth, -worldSize, edgeWidth, 2 * worldSize);

            // right
            ctx.fillRect(+worldSize, -worldSize, edgeWidth, 2*worldSize);

            // bottom
            ctx.fillRect(-worldSize - edgeWidth, +worldSize, 2 * worldSize + 2 * edgeWidth, edgeWidth);

            // the bit below was causing 7fps on a firefox instance. seemed to be leaking a path?
            // `ctx.beginPath(); ctx.fill();` reset it and restored things to 60fps
            /*
              
            I'M BAD, DON'T USE ME!
                ctx.beginPath();
                ctx.lineWidth = edgeWidth * 2;
                ctx.strokeStyle = "rgba(255,0,0,0.1)";
                ctx.rect(-worldSize - edgeWidth, -worldSize - edgeWidth, 2 * worldSize + 2 * edgeWidth, 2 * worldSize + 2 * edgeWidth);
                ctx.stroke();
            I'M BAD, DON'T USE ME!
            */

            ctx.restore();

            // start drawing the objects in the world
            ctx.font = "48px " + Settings.font;
            ctx.fillStyle = "white";
            ctx.textAlign = "center";
            ctx.strokeStyle = "white";
            ctx.lineWidth = 6;

            var groupsUsed = [];

            cache.foreach(function(body) {
                var object = body;

                var position = interpolator.projectObject(object, currentTime);

                // keep track of which "groups" are used, and collect the points of all the objects
                // in the groups... we'll use this later to draw a label on the group (eg, fleet of ships)
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

                // if we're drawing hitboxes
                if (Settings.showHitboxes && object.Size > 0) {
                    ctx.save();
                    ctx.fillStyle = this.colorValue(object.Color);
                    ctx.beginPath();
                    ctx.arc(position.X, position.Y, object.Size, 0, 2 * Math.PI, false);
                    ctx.fill();
                    ctx.restore();
                }

                // draw the sprite
                var sprite = object.Sprite != null ? sprites[object.Sprite] : false;
                if (sprite) {
                    ctx.save();
                    ctx.translate(position.X, position.Y);

                    var spriteWidth = sprite.image.width;
                    var spriteHeight = sprite.image.height;

                    ctx.rotate(position.Angle);
                    ctx.scale(sprite.scale, sprite.scale);

                    if (sprite.scaleToSize)
                        ctx.scale(object.Size, object.Size);

                    ctx.drawImage(sprite.image, -spriteWidth / 2, -spriteHeight / 2, spriteWidth, spriteHeight);

                    ctx.restore();
                }
            }, this);

            // draw labels on groups
            if (Settings.namesEnabled) {
                for (var i = 0; i < groupsUsed.length; i++) {
                    var group = groupsUsed[i];

                    if (group && group.group) {
                        var pt = { X: 0, Y: 0 };

                        // average the location of all the points
                        // to find a "center"
                        for (var x = 0; x < group.points.length; x++) {
                            pt.X += group.points[x].X;
                            pt.Y += group.points[x].Y;
                        }
                        pt.X /= group.points.length;
                        pt.Y /= group.points.length;

                        // draw a caption relative to the average above
                        ctx.fillText(group.group.Caption, pt.X, pt.Y + 90);
                    }
                }
            }
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
