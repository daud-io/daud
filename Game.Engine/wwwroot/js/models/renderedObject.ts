import images from "../../img/*.png";
import { Settings } from "../settings";
import { textureCache } from "./textureCache";
import { textureMap } from "./textureMap";
import { spriteModeMap } from "./spriteModeMap";
import "pixi.js";
import "pixi-layers";
import { compressionOptions } from "jszip/lib/defaults";
import { Container, Sprite } from "pixi.js";
import { CustomContainer } from "../CustomContainer";

export class RenderedObject {
    container: CustomContainer;
    currentSpriteName: boolean;
    currentMode: number;
    currentZIndex: number;
    activeTextures: {};
    body?: any;
    spriteLayers?: any;
    constructor(container: CustomContainer) {
        this.container = container;
        this.currentSpriteName = false;
        this.currentMode = 0;
        this.currentZIndex = 0;

        this.activeTextures = {};
    }

    decodeModes(mode) {
        return ["default"];
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
        return RenderedObject.getImageFromTextureDefinition(textureDefinition);
    }

    static loadTexture(textureDefinition, textureName) {
        let textures = textureCache[textureName];

        if (!textures) {
            textures = [];

            const img = RenderedObject.getImageFromTextureDefinition(textureDefinition);

            const baseTexture = PIXI.BaseTexture.from(img);

            baseTexture.mipmap = Settings.mipmapping;
            (<any>baseTexture).defaultAnchor = new PIXI.Point(0, 0);

            if (textureDefinition.animated) {
                const tileSize = textureDefinition.tileSize || 32;
                const totalTiles = textureDefinition.tileCount || 1;

                for (let tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
                    const sx = tileSize * (tileIndex % totalTiles);
                    const sy = 0;
                    const sw = tileSize;
                    const sh = tileSize;
                    textures.push(new PIXI.Texture(baseTexture, new PIXI.Rectangle(sx, sy, sw, sh), null, null, textureDefinition.rotate || 0));
                }
            } else if (textureDefinition.map) {
                let imageWidth = textureDefinition.imageWidth;
                let imageHeight = textureDefinition.imageHeight;
                let tileWidth = textureDefinition.tileWidth;
                let tileHeight = textureDefinition.tileHeight;

                let tilesWide = Math.floor(imageWidth / tileWidth);
                let tilesHigh = Math.floor(imageHeight / tileHeight);

                for (var row = 0; row < tilesHigh; row++)
                    for (var col = 0; col < tilesWide; col++) {
                        let x = Math.floor(col * tileWidth);
                        let y = Math.floor(row * tileHeight);

                        var texture = new PIXI.Texture(baseTexture, new PIXI.Rectangle(x, y, tileWidth, tileHeight));
                        (<any>texture).row = row;
                        (<any>texture).col = col;
                        (<any>texture).scaleMode = PIXI.SCALE_MODES.NEAREST;
                        //texture.scaleMode = PIXI.SCALE_MODES.LINEAR;
                        textures.push(texture);
                    }
            } else textures.push(new PIXI.Texture(baseTexture));

            textureCache[textureName] = textures;
        }

        return textures;
    }

    static getTextureDefinition(textureName) {
        var mapKey = this.parseMapKey(textureName);
        if (mapKey) textureName = mapKey.name;

        var textureDefinition = textureMap[textureName];
        if (!textureDefinition) console.log(`cannot load texture '${textureName}'`);

        return textureDefinition;
    }

    static parseMapKey(mapKey) {
        if (!mapKey) return false;

        var mapKeyMatches = mapKey.match(/^(.*)\[(\d*)\]/);

        if (mapKeyMatches)
            return {
                name: mapKeyMatches[1],
                mapID: mapKeyMatches[2]
            };
        else return false;
    }

    buildSprite(textureName, spriteName): Sprite {
        const textureDefinition = RenderedObject.getTextureDefinition(textureName);
        const textures = RenderedObject.loadTexture(textureDefinition, textureName);
        var pixiSprite = null;

        if (textureDefinition.animated) {
            pixiSprite = new PIXI.extras.AnimatedSprite(textures);
            if (pixiSprite instanceof PIXI.extras.AnimatedSprite) {
                pixiSprite.loop = textureDefinition.loop;
                pixiSprite.animationSpeed = textureDefinition.animationSpeed;
            }
            pixiSprite.parentGroup = this.container.bodyGroup;
        } else if (textureDefinition.map) {
            console.log("warning: requested tile from RenderedObject");
        } else {
            pixiSprite = new PIXI.Sprite(textures[0]);
            pixiSprite.parentGroup = this.container.bodyGroup;
        }

        if (textureDefinition.tint) {
            if (typeof textureDefinition.tint == "string") pixiSprite.tint = parseInt(textureDefinition.tint);
            else pixiSprite.tint = textureDefinition.tint;
        }

        if (textureDefinition.blendMode) pixiSprite.alpha = textureDefinition.alpha;

        if (textureDefinition.blendMode) pixiSprite.blendMode = textureDefinition.blendMode;

        pixiSprite.pivot.x = pixiSprite.width / 2;
        pixiSprite.pivot.y = pixiSprite.height / 2;
        pixiSprite.x = 0;
        pixiSprite.y = 0;
        pixiSprite.baseScale = textureDefinition.scale;
        pixiSprite.scale = textureDefinition.scale;
        pixiSprite.baseOffset = textureDefinition.offset || { x: 0, y: 0 };

        if (textureDefinition.animated && pixiSprite instanceof PIXI.extras.AnimatedSprite) pixiSprite.play();

        return pixiSprite;
    }

    static getSpriteDefinition(spriteName): any {
        let spriteDefinition = null;

        var mapKey = this.parseMapKey(spriteName);
        if (mapKey) spriteName = mapKey.name;

        if (spriteModeMap[spriteName]) spriteDefinition = spriteModeMap[spriteName];

        return spriteDefinition;
    }

    getModeMap(spriteName, mode) {
        let layers = [];

        const spriteDefinition = RenderedObject.getSpriteDefinition(spriteName);

        if (!spriteDefinition) console.log(`Cannot find sprite: ${spriteName}`);

        const modes = this.decodeModes(mode);

        modes.forEach(modeName => {
            var modeLayers = spriteDefinition.modes[modeName];
            if (modeLayers) modeLayers.forEach(layer => layers.push(layer));
        });

        return layers;
    }

    buildSpriteLayers(spriteName, mode, zIndex) {
        const layers = this.getModeMap(spriteName, mode);

        if (layers) {
            const spriteLayers = [];
            for (let i = 0; i < layers.length; i++) {
                let spriteLayer = null;
                var textureName = layers[i];

                if (this.activeTextures[textureName]) spriteLayer = this.activeTextures[textureName];
                else {
                    //console.log('building sprite for ' + textureName);
                    spriteLayer = this.buildSprite(textureName, spriteName);
                }

                if (zIndex == 0) zIndex = 250;

                spriteLayer.zOrder = zIndex - i + this.body.ID / 100000;

                spriteLayers.push(spriteLayer);
                this.activeTextures[textureName] = spriteLayer;
            }

            for (var key in this.activeTextures) {
                if (layers.indexOf(key) == -1) {
                    this.container.removeChild(this.activeTextures[key]);
                    //console.log(`delete sprite layer ${spriteName}:${key}`);
                    delete this.activeTextures[key];
                }
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
            this.activeTextures = {};
        }
    }

    refreshSprite() {
        this.setSprite(this.currentSpriteName, this.currentMode, this.currentZIndex, true);
    }

    setSprite(spriteName, mode, zIndex, reload = false) {
        // check that we really need to change anything
        if (reload || spriteName != this.currentSpriteName || mode != this.currentMode || zIndex != this.currentZIndex) {
            this.currentSpriteName = spriteName;
            this.currentMode = mode;
            this.currentZIndex = zIndex;

            // if we have any existing sprites, destroy them
            if (reload) this.destroySprites();

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
            layer.pivot.x = Math.floor(layer.texture.width / 2);
            layer.pivot.y = Math.floor(layer.texture.height / 2);

            layer.position.x = Math.floor(interpolatedPosition.X + (layer.baseOffset.x * Math.cos(angle) - layer.baseOffset.y * Math.sin(angle)));

            layer.position.y = Math.floor(interpolatedPosition.Y + (layer.baseOffset.y * Math.cos(angle) + layer.baseOffset.x * Math.sin(angle)));

            layer.rotation = angle;

            layer.scale.set(size * layer.baseScale, size * layer.baseScale);
        });
    }

    update(updateData) {
        this.body = updateData;

        this.setSprite(updateData.Sprite, updateData.Mode, updateData.zIndex);
    }

    foreachLayer(action) {
        if (this.spriteLayers && this.spriteLayers.length)
            this.spriteLayers.forEach((layer, i) => {
                action.apply(this, [layer, i]);
            });
    }
}
