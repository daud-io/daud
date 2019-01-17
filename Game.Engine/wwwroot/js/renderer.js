export class Renderer {
    constructor(container) {
        this.container = container;
    }

    draw(cache, interpolator, currentTime, fleetID) {
        const groupsUsed = [];

        cache.foreach(function(body) {
            if (body.Group) {
                var group = cache.getGroup(body.Group);
                if (group && groupsUsed.indexOf(group) == -1) groupsUsed.push(group);
            }

            if (body.renderer) body.renderer.preRender(currentTime, interpolator, fleetID);
        }, this);

        let ids = [];
        for (let i = 0; i < groupsUsed.length; i++) {
            const group = groupsUsed[i];

            if (group) {
                ids.push(`g-${group.ID}`);

                if (group.renderer) group.renderer.preRender(currentTime, interpolator, fleetID);
            }
        }
    }
}
