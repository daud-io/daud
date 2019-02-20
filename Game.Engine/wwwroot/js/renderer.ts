import { CustomContainer } from "./CustomContainer";
import { Interpolator } from "./interpolator";
import {Cache} from "./cache";

export class Renderer {
    container: CustomContainer;
    constructor(container: CustomContainer) {
        this.container = container;
    }

    draw(cache:Cache, interpolator:Interpolator, currentTime:number, fleetID:number) {
        const groupsUsed = [];

        if (this.container.tiles.isDirty) {
            this.container.tiles.clear();
            this.container.tiles.isRefreshing = true;
        } else this.container.tiles.isRefreshing = false;

        cache.foreach(function(body) {
            if (body.Group) {
                const group = cache.getGroup(body.Group);
                if (group && groupsUsed.indexOf(group) == -1) groupsUsed.push(group);
            }

            if (body.renderer) body.renderer.preRender(currentTime, interpolator, fleetID);
        }, this);

        this.container.tiles.isDirty = false;

        let ids = [];

        for (const group of groupsUsed) {
            if (group) {
                ids.push(`g-${group.ID}`);

                if (group.renderer) group.renderer.preRender(currentTime, interpolator, fleetID);
            }
        }
    }
}
