import { Settings } from "./settings";
import images from "../img/*.png";
import { FleetRenderer } from "./renderers/fleetRenderer";

function sprite(name, scale) {
    const img = new Image();
    img.src = images[name];

    return {
        image: img,
        name: name,
        scale: scale || 0.02
    };
}

export const sprites = {
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
    fish: sprite("ship0", 0.01, true),
    bullet: sprite("bullet", 0.02, true),
    seeker: sprite("seeker", 0.02, true),
    seeker_pickup: sprite("seeker_pickup", 0.02, true),
    obstacle: sprite("obstacle", 0.0028, true),
    arrow: sprite("arrow", 0.03)
};

export const spriteIndices = [
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
    "arrow"
];

function addSprite(name, size, file) {
    sprites[name] = sprite(file || name, size);
    spriteIndices.push(name);
}

const flagScale = 0.003;

addSprite("flag_blue_0", flagScale);
addSprite("flag_blue_1", flagScale);
addSprite("flag_blue_2", flagScale);
addSprite("flag_blue_3", flagScale);
addSprite("flag_blue_4", flagScale);
addSprite("flag_blue_5", flagScale);
addSprite("flag_red_0", flagScale);
addSprite("flag_red_1", flagScale);
addSprite("flag_red_2", flagScale);
addSprite("flag_red_3", flagScale);
addSprite("flag_red_4", flagScale);
addSprite("flag_red_5", flagScale);
addSprite("ctf_base", 0.0075);

addSprite("ctf_score_final");
addSprite("ctf_score_final_blue");
addSprite("ctf_score_final_red");
addSprite("ctf_score_left_0");
addSprite("ctf_score_left_1");
addSprite("ctf_score_left_2");
addSprite("ctf_score_left_3");
addSprite("ctf_score_left_4");
addSprite("ctf_score_right_0");
addSprite("ctf_score_right_1");
addSprite("ctf_score_right_2");
addSprite("ctf_score_right_3");
addSprite("ctf_score_right_4");
addSprite("ctf_score_stripes");
addSprite("ctf_arrow_red", 0.05);
addSprite("ctf_arrow_blue", 0.05);
addSprite("ctf_arrow_trans_flag", 0.1);
addSprite("thruster_default_green");
addSprite("thruster_default_orange");
addSprite("thruster_default_pink");
addSprite("thruster_default_red");
addSprite("thruster_default_cyan");
addSprite("thruster_default_yellow");
addSprite("thruster_retro_green");
addSprite("thruster_retro_orange");
addSprite("thruster_retro_pink");
addSprite("thruster_retro_red");
addSprite("thruster_retro_cyan");
addSprite("thruster_retro_yellow");
addSprite("circles");

export class Renderer {
    constructor(context, container, settings = {}) {
        this.context = context;
        this.container = container;
        this.view = false;
        this.worldSize = 6000;

        this.fleetRenderer = new FleetRenderer(context, settings);

        var graphics = new PIXI.Graphics();
        //

        // set the line style to have a width of 5 and set the color to red

        const edgeWidth = 4000;

        // draw a rectangle
        graphics.beginFill(0xff0000, 0.1);
        graphics.drawRect(-this.worldSize - edgeWidth, -this.worldSize - edgeWidth, 2 * this.worldSize + 2 * edgeWidth, edgeWidth);
        graphics.drawRect(-this.worldSize - edgeWidth, -this.worldSize, edgeWidth, 2 * this.worldSize);
        graphics.drawRect(+this.worldSize, -this.worldSize, edgeWidth, 2 * this.worldSize);
        graphics.drawRect(-this.worldSize - edgeWidth, +this.worldSize, 2 * this.worldSize + 2 * edgeWidth, edgeWidth);
        graphics.endFill();

        graphics.lineStyle(40, 0x0000ff);
        graphics.drawRect(-this.worldSize, -this.worldSize, this.worldSize * 2, this.worldSize * 2);

        this.container.addChild(graphics);
        console.log("HI");
    }

    draw(cache, interpolator, currentTime, fleetID) {
        // if (this.view) {
        const ctx = this.context;

        // Draw the edge of the universe
        ctx.save();

        // start drawing the objects in the world
        ctx.font = `48px ${Settings.font}`;
        ctx.fillStyle = "white";
        ctx.textAlign = "center";
        ctx.strokeStyle = "white";
        ctx.lineWidth = 6;

        const groupsUsed = [];

        cache.foreach(function(body) {
            const object = body;
            let group = false;

            const position = interpolator.projectObject(object, currentTime);

            const objec2 = cache.bodies[`p-${body.ID}`];
            objec2.position.x = position.X;
            objec2.position.y = position.Y;

            // keep track of which "groups" are used, and collect the points of all the objects
            // in the groups... we'll use this later to draw a label on the group (eg, fleet of ships)
            if (object.Group) {
                for (let i = 0; i < groupsUsed.length; i++)
                    if (groupsUsed[i].id == object.Group) {
                        group = groupsUsed[i];
                        break;
                    }

                if (!group) {
                    group = {
                        id: object.Group,
                        group: cache.groups[`g-${object.Group}`],
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
        }, this);

        // draw labels on groups
        if (Settings.namesEnabled) {
            for (let i = 0; i < groupsUsed.length; i++) {
                const group = groupsUsed[i];

                if (group && group.group) {
                    if (group.group.ID != fleetID || Settings.showOwnName) {
                        const pt = { X: 0, Y: 0 };

                        // average the location of all the points
                        // to find a "center"
                        for (let x = 0; x < group.points.length; x++) {
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
        // }
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
