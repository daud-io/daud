import images from "../../img/*.png";
import { textureCache } from "./textureCache";
import { textureMap } from "./textureMap";
import { spriteModeMap } from "./spriteModeMap";

export class RenderedObject {
    constructor(container) {
        this.container = container;
        this.currentSpriteName = false;
        this.currentMode = 0;
    }

    getMode(mode) {
        return "default";
    }

    static getImageFromTextureDefinition(textureDefinition) {
        var img = new Image();
        if (textureDefinition.url) img.src = textureDefinition.url;
        else {
            var src = images[textureDefinition.file];
            if (src) img.src = src;
        }

        return img;
    }

    static getTextureImage(textureName) {
        var textureDefinition = RenderedObject.getTextureDefinition(textureName);

        if (textureDefinition === false) console.log(`cannot load texture '${textureName}'`);

        return RenderedObject.getImageFromTextureDefinition(textureDefinition);
    }

    static loadTexture(textureDefinition, textureName) {
        var textures = textureCache[textureName];

        if (!textures) {
            textures = [];

            var img = RenderedObject.getImageFromTextureDefinition(textureDefinition);

            var baseTexture = new PIXI.Texture.fromLoader(img);

            if (textureDefinition.animated) {
                var tileSize = textureDefinition.tileSize || 32;
                var totalTiles = textureDefinition.tileCount || 1;

                for (var tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
                    var sx = tileSize * (tileIndex % totalTiles);
                    var sy = 0;
                    var sw = tileSize;
                    var sh = tileSize;

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
        var textureDefinition = RenderedObject.getTextureDefinition(textureName);
        var textures = RenderedObject.loadTexture(textureDefinition, textureName);
        var pixiSprite = false;

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
        var spriteDefinition = false;
        if (spriteModeMap[spriteName]) spriteDefinition = spriteModeMap[spriteName];

        return spriteDefinition;
    }

    getModeMap(spriteName, mode) {
        var layers = false;
        var modeName = this.getMode(mode);
        var spriteDefinition = RenderedObject.getSpriteDefinition(spriteName);

        if (spriteDefinition.modes[modeName]) layers = spriteModeMap[spriteName].modes[modeName];

        if (!layers && spriteDefinition.modes["default"]) layers = spriteModeMap[spriteName].modes["default"];

        return layers;
    }

    buildSpriteLayers(spriteName, mode) {
        var layers = this.getModeMap(spriteName, mode);

        if (layers) {
            var spriteLayers = [];
            for (var i = 0; i < layers.length; i++) {
                var spriteLayer = this.buildSprite(layers[i]);
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
            for (var i = 0; i < this.spriteLayers.length; i++) {
                var layer = this.spriteLayers[i];
                this.container.removeChild(layer);
            }

            this.spriteLayers = false;
        }
    }

    refreshSprite() {
        this.setSprite(this.currentSpriteName, this.currentMode, true);
    }

    setSprite(spriteName, mode, reload) {
        // check that we really need to change anything
        if (reload || spriteName != this.currentSpriteName || mode != this.currentMode) {
            this.currentSpriteName = spriteName;
            this.currentMode = mode;

            //console.log(mode);

            // if we have any existing sprites, destroy them
            this.destroySprites();
            this.spriteLayers = this.buildSpriteLayers(spriteName, mode);

            this.foreachLayer(function(layer, index) {
                this.container.addChildAt(layer, 2);
            });
        }
    }

    preRender(time, interpolator) {
        if (this.body) {
            var newPosition = interpolator.projectObject(this.body, time);
            this.moveSprites(newPosition, this.body.Size);
        }
    }

    moveSprites(interpolatedPosition, size) {
        var angle = interpolatedPosition.Angle;

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
        this.setSprite(updateData.Sprite, updateData.Mode);
        //console.log(updateData.Mode);
    }

    foreachLayer(action) {
        if (this.spriteLayers && this.spriteLayers.length)
            for (var i = 0; i < this.spriteLayers.length; i++) {
                var layer = this.spriteLayers[i];
                action.apply(this, [layer, i]);
            }
    }
}
