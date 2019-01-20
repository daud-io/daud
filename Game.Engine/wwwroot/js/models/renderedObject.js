import images from "../../img/*.png";
import { textureCache } from "./textureCache";
import { textureMap } from "./textureMap";
import { spriteModeMap } from "./spriteModeMap";

export class RenderedObject {
    constructor(container) {
        this.container = container;
        this.currentSpriteName = false;
        this.currentMode = 0;
        this.currentZIndex = 0;
    }

    getMode(mode) {
        return "default";
    }

    static getImageFromTextureDefinition(textureDefinition) {
        const img = new Image();
        if (textureDefinition.url) img.src = textureDefinition.url;
        else {
            const src = images[textureDefinition.file];
            if (src) img.src = src;
        }

        return img;
    }

    static getTextureImage(textureName) {
        const textureDefinition = RenderedObject.getTextureDefinition(textureName);

        if (textureDefinition === false) console.log(`cannot load texture '${textureName}'`);

        return RenderedObject.getImageFromTextureDefinition(textureDefinition);
    }

    static loadTexture(textureDefinition, textureName) {
        let textures = textureCache[textureName];

        if (!textures) {
            textures = [];

            const img = RenderedObject.getImageFromTextureDefinition(textureDefinition);

            const baseTexture = new PIXI.Texture.fromLoader(img);

            if (textureDefinition.animated) {
                const tileSize = textureDefinition.tileSize || 32;
                const totalTiles = textureDefinition.tileCount || 1;

                for (let tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
                    const sx = tileSize * (tileIndex % totalTiles);
                    const sy = 0;
                    const sw = tileSize;
                    const sh = tileSize;

                    textures.push(new PIXI.Texture(baseTexture, new PIXI.Rectangle(sx, sy, sw, sh), false, false, textureDefinition.rotate || 0));
                }
            } else textures.push(baseTexture);

            textureCache[textureName] = textures;
        }

        return textures;
    }

    static getTextureDefinition(textureName) {
        return textureMap[textureName];
    }

    buildSprite(textureName) {
        const textureDefinition = RenderedObject.getTextureDefinition(textureName);
        const textures = RenderedObject.loadTexture(textureDefinition, textureName);
        let pixiSprite = false;

        if (textureDefinition.animated) {
            pixiSprite = new PIXI.extras.AnimatedSprite(textures);
            pixiSprite.loop = textureDefinition.loop;
            pixiSprite.animationSpeed = textureDefinition.animationSpeed;
        } else {
            pixiSprite = new PIXI.Sprite(textures[0]);
        }

        if (textureDefinition.tint) pixiSprite.tint = textureDefinition.tint;

        pixiSprite.pivot.x = pixiSprite.width / 2;
        pixiSprite.pivot.y = pixiSprite.height / 2;
        pixiSprite.position.x = 0;
        pixiSprite.position.y = 0;
        pixiSprite.baseScale = textureDefinition.scale;
        pixiSprite.baseOffset = textureDefinition.offset || { x: 0, y: 0 };

        if (textureDefinition.animated) pixiSprite.play();

        return pixiSprite;
    }

    static getSpriteDefinition(spriteName) {
        let spriteDefinition = false;
        if (spriteModeMap[spriteName]) spriteDefinition = spriteModeMap[spriteName];

        return spriteDefinition;
    }

    getModeMap(spriteName, mode) {
        let layers = false;
        const modeName = this.getMode(mode);
        const spriteDefinition = RenderedObject.getSpriteDefinition(spriteName);

        if (spriteDefinition.modes[modeName]) layers = spriteModeMap[spriteName].modes[modeName];

        if (!layers && spriteDefinition.modes["default"]) layers = spriteModeMap[spriteName].modes["default"];

        return layers;
    }

    buildSpriteLayers(spriteName, mode, zIndex) {
        const layers = this.getModeMap(spriteName, mode);

        if (layers) {
            const spriteLayers = [];
            for (let i = 0; i < layers.length; i++) {
                const spriteLayer = this.buildSprite(layers[i]);
                spriteLayer.zIndex = zIndex;

                spriteLayers.push(spriteLayer);
            }

            return spriteLayers;
        } else return false;
    }

    destroy() {
        this.destroySprites();
    }

    destroySprites() {
        if (this.spriteLayers) {
            for (const layer of this.spriteLayers) {
                this.container.removeChild(layer);
            }

            this.spriteLayers = false;
        }
    }

    refreshSprite() {
        this.setSprite(this.currentSpriteName, this.currentMode, true);
    }

    setSprite(spriteName, mode, zIndex, reload) {
        // check that we really need to change anything
        if (reload || spriteName != this.currentSpriteName || mode != this.currentMode || zIndex != this.currentZIndex) {
            this.currentSpriteName = spriteName;
            this.currentMode = mode;
            this.currentZIndex = zIndex;

            //console.log(mode);

            // if we have any existing sprites, destroy them
            this.destroySprites();
            this.spriteLayers = this.buildSpriteLayers(spriteName, mode, zIndex);

            this.foreachLayer(function(layer, index) {
                this.container.addChildAt(layer, 2);
            });
        }
    }

    preRender(time, interpolator) {
        if (this.body) {
            const newPosition = interpolator.projectObject(this.body, time);
            this.moveSprites(newPosition, this.body.Size);
        }
    }

    moveSprites(interpolatedPosition, size) {
        const angle = interpolatedPosition.Angle;

        this.foreachLayer(function(layer, index) {
            layer.pivot.x = layer.texture.width / 2;
            layer.pivot.y = layer.texture.height / 2;

            layer.position.x = interpolatedPosition.X + (layer.baseOffset.x * Math.cos(angle) - layer.baseOffset.y * Math.sin(angle));

            layer.position.y = interpolatedPosition.Y + (layer.baseOffset.y * Math.cos(angle) + layer.baseOffset.x * Math.sin(angle));

            layer.rotation = angle;
            layer.scale.set(size * layer.baseScale, size * layer.baseScale);
        });
    }

    update(updateData) {
        this.body = updateData;

        this.setSprite(updateData.Sprite, updateData.Mode, false, updateData.zIndex);
    }

    foreachLayer(action) {
        if (this.spriteLayers && this.spriteLayers.length)
            this.spriteLayers.forEach((layer, i) => {
                action.apply(this, [layer, i]);
            });
    }
}
