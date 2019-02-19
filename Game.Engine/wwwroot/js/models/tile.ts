import { RenderedObject } from "./renderedObject";

export class Tile extends RenderedObject {
    constructor(container,cache) {
        super(container);
        (<any>this.container).tiles.isDirty = true;
    }

    destroy() {
        (<any>this.container).tiles.isDirty = true;
        super.destroy();
    }

    update(updateData) {
        this.body = updateData;
        //super.update(updateData);
    }

    preRender(currentTime, interpolator) {
        if (!this.body) return;

        if ((<any>this.container).tiles.isRefreshing) {
            var tiles = (<any>this.container).tiles;
            var mapKey = RenderedObject.parseMapKey(this.body.Sprite);

            if (!mapKey) console.log(`non-map key used to reference map texture: ${this.body.Sprite}`);
            else {
                const textureDefinition = RenderedObject.getTextureDefinition(this.body.Sprite);
                const textures = RenderedObject.loadTexture(textureDefinition, mapKey.name);
                var texture = textures[mapKey.mapID];

                tiles.addFrame(
                    texture,
                    (this.body.OriginalPosition.X / (this.body.Size * 2)) * textureDefinition.tileWidth - textureDefinition.tileWidth / 2,
                    (this.body.OriginalPosition.Y / (this.body.Size * 2)) * textureDefinition.tileHeight - textureDefinition.tileHeight / 2
                );

                tiles.scale.x = (this.body.Size * 2) / textureDefinition.tileWidth;
                tiles.scale.y = (this.body.Size * 2) / textureDefinition.tileHeight;
            }
        }
    }
}
