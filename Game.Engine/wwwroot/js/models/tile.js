import { RenderedObject } from "./renderedObject";

export class Tile extends RenderedObject {
    constructor(container) {
        super(container);
        this.container.tiles.isDirty = true;
    }

    destroy() {
        this.container.tiles.isDirty = true;
        super.destroy();
    }

    update(updateData) {
        this.body = updateData;
        //super.update(updateData);
    }

    preRender(currentTime, interpolator, fleetID) {
        if (!this.body) return;

        if (this.container.tiles.isRefreshing) {
            var tiles = this.container.tiles;
            var mapKey = RenderedObject.parseMapKey(this.body.Sprite);

            if (!mapKey) console.log("non-map key used to reference map texture");
            else {
                const textureDefinition = RenderedObject.getTextureDefinition(this.body.Sprite);
                const textures = RenderedObject.loadTexture(textureDefinition, mapKey.name);
                var texture = textures[mapKey.mapID];

                tiles.addFrame(
                    texture,
                    (this.body.OriginalPosition.X / textureDefinition.tileSpaceWidth) * textureDefinition.tileWidth,
                    (this.body.OriginalPosition.Y / textureDefinition.tileSpaceHeight) * textureDefinition.tileHeight
                );

                tiles.scale.x = this.body.Size * textureDefinition.scale;
                tiles.scale.y = this.body.Size * textureDefinition.scale;
            }
        }
    }
}
