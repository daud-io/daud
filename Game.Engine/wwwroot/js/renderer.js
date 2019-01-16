import { Settings } from "./settings";
import images from "../img/*.png";

const background = new PIXI.Texture.fromImage(images["bg"]);
export const backgroundSprite = new PIXI.extras.TilingSprite(background, 200000, 200000);
backgroundSprite.tileScale.set(10, 10);
backgroundSprite.position.x = -100000;
backgroundSprite.position.y = -100000;

export class Renderer {
    constructor(container, settings = {}) {
        this.container = container;
        this.view = false;

        this.container.addChild(backgroundSprite);

        this.graphics = new PIXI.Graphics();
        this.updateWorldSize(6000);
        this.container.addChild(this.graphics);
    }

    updateWorldSize(size) {
        const edgeWidth = 4000;
        this.graphics.clear();
        this.graphics.beginFill(0xff0000, 0.1);
        this.graphics.drawRect(-size - edgeWidth, -size - edgeWidth, 2 * size + 2 * edgeWidth, edgeWidth);
        this.graphics.drawRect(-size - edgeWidth, -size, edgeWidth, 2 * size);
        this.graphics.drawRect(+size, -size, edgeWidth, 2 * size);
        this.graphics.drawRect(-size - edgeWidth, +size, 2 * size + 2 * edgeWidth, edgeWidth);
        this.graphics.endFill();

        this.graphics.lineStyle(40, 0x0000ff);
        this.graphics.drawRect(-size, -size, size * 2, size * 2);

        this.worldSize = size;
    }
    draw(cache, interpolator, currentTime, fleetID) {
        const groupsUsed = [];

        if (Settings.background == "none" && backgroundSprite.visible)
            backgroundSprite.visible = false;
        if (Settings.background == "on" && !backgroundSprite.visible)
            backgroundSprite.visible = true;

        cache.foreach(function(body) {
            if (body.renderer)
                body.renderer.preRender(currentTime, interpolator);
        }, this);

        // draw labels on groups
        let ids = [];
        if (Settings.namesEnabled) {
            for (let i = 0; i < groupsUsed.length; i++) {
                const group = groupsUsed[i];

                if (group && group.group) {
                    ids.push(`g-${group.group.ID}`);
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
                        /*const body = cache.bodies[`p-${group.group.ID}`];
                        body.position.x = pt.X;
                        body.position.y = pt.Y;
                        body.visible = Settings.namesEnabled;*/
                    }
                }
            }
        }
    }
}
