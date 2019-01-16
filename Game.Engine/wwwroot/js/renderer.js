import { Settings } from "./settings";

export class Renderer {
    constructor(container) {
        this.container = container;
    }

    draw(cache, interpolator, currentTime, fleetID) {
        const groupsUsed = [];

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
