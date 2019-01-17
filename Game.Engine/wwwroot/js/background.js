import { Settings } from "./settings";
import { RenderedObject } from "./models/renderedObject";

export class Background {
    constructor(container) {
        this.container = container;

        this.refreshSprite();
    }

    draw(cache, interpolator, currentTime) {
        if (this.backgroundSprite) {
            if (Settings.background == "none" && this.backgroundSprite.visible) this.backgroundSprite.visible = false;
            if (Settings.background == "on" && !this.backgroundSprite.visible) this.backgroundSprite.visible = true;
        }
    }

    refreshSprite() {
        var spriteDefinition = RenderedObject.getSpriteDefinition("bg");
        var textureName = spriteDefinition.texture;
        var textureDefinition = RenderedObject.getTextureDefinition(textureName);
        var textures = RenderedObject.loadTexture(textureDefinition, textureName);

        if (textures.length > 0) {
            if (!this.backgroundSprite) {
                this.backgroundSprite = new PIXI.extras.TilingSprite(textures[0], 200000, 200000);
                this.container.addChild(this.backgroundSprite);
                this.backgroundSprite.tileScale.set(spriteDefinition.scaleFactor || 10, spriteDefinition.scaleFactor || 10);
                this.backgroundSprite.position.x = -100000;
                this.backgroundSprite.position.y = -100000;
            } else this.backgroundSprite.texture = textures[0];
        }
    }

    destroy() {
        if (this.backgroundSprite) this.container.removeChild(this.backgroundSprite);

        this.backgroundSprite = false;
    }
}
