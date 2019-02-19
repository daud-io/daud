import { Settings } from "./settings";
import { RenderedObject } from "./models/renderedObject";
import { Container } from "pixi.js";
import { Vector2 } from "./Vector2";

export class Background extends RenderedObject {
    container : Container;
    focus: Vector2;
    speeds: any[];
    backgroundSprites: any;
    constructor(container: Container) {
        super(container);
        this.container = container;
        this.focus = new Vector2(0,0);
        this.speeds = [];
        this.refreshSprite();
    }

    draw(cache, interpolator, currentTime) {
        if (this.backgroundSprites) {
            for (var i = 0; i < this.backgroundSprites.length; i++) {
                var backgroundSprite = this.backgroundSprites[i];
                if (this.speeds && this.speeds.length > i) {
                    backgroundSprite.position.x = -100000 * (Math.cos(backgroundSprite.rotation) - Math.sin(backgroundSprite.rotation)) - this.focus.x * (this.speeds[i] - 1);
                    backgroundSprite.position.y = -100000 * (Math.sin(backgroundSprite.rotation) + Math.cos(backgroundSprite.rotation)) - this.focus.y * (this.speeds[i] - 1);
                    if (Settings.background == "none" && backgroundSprite.visible) backgroundSprite.visible = false;
                    if (Settings.background == "on" && !backgroundSprite.visible) backgroundSprite.visible = true;
                } else {
                    backgroundSprite.visible = false;
                }
            }
        }
    }
    updateFocus(x, y) {
        this.focus = { x: x, y: y };
    }

    refreshSprite() {
        const spriteDefinition = RenderedObject.getSpriteDefinition("bg");

        var additionalLayers = spriteDefinition.additionalLayers;
        if (!additionalLayers) {
            additionalLayers = [];
        }
        var speeds = [spriteDefinition.speed ? spriteDefinition.speed : 1].concat(additionalLayers.map(x => x.speed));
        this.speeds = speeds;
        var layers = [spriteDefinition].concat(additionalLayers);
        var allLayersTextureNames = layers.map(x => x.texture);
        var allLayersTextures = allLayersTextureNames.map(x => RenderedObject.getTextureDefinition(x));
        if (!this.backgroundSprites) {
            this.backgroundSprites = [];
        }
        for (var i = 0; i < allLayersTextures.length; i++) {
            if (i >= this.backgroundSprites.length) {
                this.backgroundSprites.push(null);
            }
            var textures = RenderedObject.loadTexture(allLayersTextures[i], allLayersTextureNames[i]);
            if (textures.length > 0) {
                var backgroundSprite = this.backgroundSprites[i];
                if (!backgroundSprite) {
                    backgroundSprite = new PIXI.extras.TilingSprite(textures[0], 200000, 200000);
                    backgroundSprite.parentGroup = (<any>this.container).backgroundGroup;
                    this.container.addChild(backgroundSprite);
                    backgroundSprite.tileScale.set(allLayersTextures[i].scale, allLayersTextures[i].scale);
                    backgroundSprite.rotation = Math.random() - 0.5;
                    backgroundSprite.position.x = -100000 * (Math.cos(backgroundSprite.rotation) - Math.sin(backgroundSprite.rotation));
                    backgroundSprite.position.y = -100000 * (Math.sin(backgroundSprite.rotation) + Math.cos(backgroundSprite.rotation));

                    this.backgroundSprites[i] = backgroundSprite;
                } else backgroundSprite.texture = textures[0];
            } else {
            }
        }
    }

    destroy() {
        if (this.backgroundSprites) {
            for (var i = 0; i < this.backgroundSprites.length; i++) {
                var backgroundSprite = this.backgroundSprites[i];
                if (backgroundSprite) this.container.removeChild(backgroundSprite);
            }
        }

        this.backgroundSprites = [];
    }
    update(updateData) {
        super.update(updateData);
    }
}
