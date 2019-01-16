export class Renderer {
    constructor(container) {
        this.container = container;
    }

    draw(cache, interpolator, currentTime, fleetID) {
        const groupsUsed = [];

        cache.foreach(function(body) {
            if (body.Group)
            {
                var group = cache.getGroup(body.Group);
                if (group)
                    groupsUsed.push(group);
            }

            if (body.renderer)
                body.renderer.preRender(currentTime, interpolator);
        }, this);

        let ids = [];
        for (let i = 0; i < groupsUsed.length; i++) {
            const group = groupsUsed[i];

            if (group && group.group) {
                ids.push(`g-${group.group.ID}`);

                if (group.renderer)
                    group.renderer.preRender(currentTime, interpolator);
            }
        }
    }
}
