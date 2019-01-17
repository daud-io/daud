import { Settings } from "./settings";
import { RenderedObject } from "./models/renderedObject";

export class Background {
    constructor(container) {
        this.container = container;

        var spriteDefinition = RenderedObject.getSpriteDefinition("bg");
        var textureName = spriteDefinition.texture;
        var textureDefinition = RenderedObject.getTextureDefinition(textureName);
        var textures = RenderedObject.loadTexture(textureDefinition, textureName);

        if (textures.length > 0) {
            this.backgroundSprite = new PIXI.extras.TilingSprite(textures[0], 200000, 200000);
            this.backgroundSprite.tileScale.set(spriteDefinition.scaleFactor || 10, spriteDefinition.scaleFactor || 10);
            this.backgroundSprite.position.x = -100000;
            this.backgroundSprite.position.y = -100000;

            this.container.addChild(this.backgroundSprite);
        }
    }

    draw(cache, interpolator, currentTime) {
        if (Settings.background == "none" && this.backgroundSprite.visible) this.backgroundSprite.visible = false;
        if (Settings.background == "on" && !this.backgroundSprite.visible) this.backgroundSprite.visible = true;
    }
}

/*
window.onkeydown = function(e) {
	if (e.key === "w" && Settings.displayMinimap === "onkeypress") { alert("test1") };
}

window.onkeyup = function(e) {
	if (e.key === "w" && Settings.displayMinimap === "onkeypress" ) { alert("test2") };
}
*/