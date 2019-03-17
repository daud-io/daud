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
import { parseScssIntoRules, parseCssIntoRules, queryProperties } from "../parser/parseTheme.js";
import { readFileSync } from 'fs';
var textureMapRules = parseScssIntoRules(readFileSync(__dirname + '/textureMap.scss', 'utf-8'));
var spriteModeMapRules = parseScssIntoRules(readFileSync(__dirname + '/spriteModeMap.scss', 'utf-8'));
export class RenderedObject {
    container: CustomContainer;
    currentSpriteName: boolean;
    currentMode: number;
    currentZIndex: number;
    activeTextures: {};
    body?: any;
    spriteLayers?: any;
    additionalClasses?:string[];
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

            if (textureDefinition.animated) {
                const tileSize = textureDefinition["tile-size"] || 32;
                const totalTiles = textureDefinition["tile-count"] || 1;

                for (let tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
                    const sx = tileSize * (tileIndex % totalTiles);
                    const sy = 0;
                    const sw = tileSize;
                    const sh = tileSize;
                    textures.push(new PIXI.Texture(baseTexture, new PIXI.Rectangle(sx, sy, sw, sh), null, null, textureDefinition.rotate || 0));
                }
            } else if (textureDefinition.map) {
                let imageWidth = textureDefinition["image-width"];
                let imageHeight = textureDefinition["image-height"];
                let tileWidth = textureDefinition["tile-width"];
                let tileHeight = textureDefinition["tile-height"];

                let tilesWide = Math.floor(imageWidth / tileWidth);
                let tilesHigh = Math.floor(imageHeight / tileHeight);

                for (var row = 0; row < tilesHigh; row++)
                    for (var col = 0; col < tilesWide; col++) {
                        let x = Math.floor(col * tileWidth);
                        let y = Math.floor(row * tileHeight);

                        var texture = new PIXI.Texture(baseTexture, new PIXI.Rectangle(x, y, tileWidth, tileHeight));
                        texture.baseTexture.scaleMode = PIXI.SCALE_MODES.NEAREST;
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

        var textureDefinition = null;
        try {
            textureDefinition = queryProperties({ element: textureName }, textureMapRules);
            for (var i in textureDefinition) {
                if (typeof textureDefinition[i] == "string") {
                    textureDefinition[i] = JSON.parse(textureDefinition[i]);
                }
            }
        } catch (e) {
            console.log("TEXTURE FAILED:", e);
        }
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
                pixiSprite.animationSpeed = textureDefinition["animation-speed"];
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
        if (textureDefinition["offset-x"]) {
            textureDefinition.offset = { x: textureDefinition["offset-x"], y: textureDefinition["offset-y"] };
        }
        pixiSprite.baseOffset = textureDefinition.offset || { x: 0, y: 0 };

        if (textureDefinition.animated && pixiSprite instanceof PIXI.extras.AnimatedSprite) pixiSprite.play();

        return pixiSprite;
    }
    
    static getSpriteDefinition(spriteName,additional?:string[]): any {
        let spriteDefinition = null;
        if(!additional){
            additional=[];
        }
        var mapKey = this.parseMapKey(spriteName);
        if (mapKey) spriteName = mapKey.name;
        try {
            spriteDefinition = queryProperties({ element: spriteName.split("_")[0] ,class:spriteName.split("_").join(" ")+" " +additional.join(" ")}, spriteModeMapRules);
            for (var i in spriteDefinition) {
                if (typeof spriteDefinition[i] == "string") {
                    if(spriteDefinition[i].indexOf(",")<0 && i!=="textures"){
                        spriteDefinition[i] = JSON.parse(spriteDefinition[i]);
                    }else{
                        try{
                            spriteDefinition[i] = spriteDefinition[i].split(",").map(q=>JSON.parse(q));
                        }catch (e) {
                            console.log("SPRITE PROP LIST DECODE FAILED:", e);
                        }
                    }
                }
            }
        } catch (e) {
            console.log("SPRITE FAILED:", e);
        }
        if (!spriteDefinition) console.log(`Cannot find sprite: ${spriteName}`);

        return spriteDefinition;
    }

    getModeMap(spriteName, mode) {
        let layers = [];
        const modes = this.decodeModes(mode);

        const spriteDefinition = RenderedObject.getSpriteDefinition(spriteName,modes);

        return spriteDefinition.textures;
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
                    let layer = this.activeTextures[key];
                    this.container.removeChild(layer);
                    layer.destroy();
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
                layer.destroy();
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

            this.foreachLayer(function (layer, index) {
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

        this.foreachLayer(function (layer, index) {
            layer.pivot.x = Math.floor(layer.texture.width / 2);
            layer.pivot.y = Math.floor(layer.texture.height / 2);

            layer.position.x = Math.floor(interpolatedPosition.x + (layer.baseOffset.x * Math.cos(angle) - layer.baseOffset.y * Math.sin(angle)));

            layer.position.y = Math.floor(interpolatedPosition.y + (layer.baseOffset.y * Math.cos(angle) + layer.baseOffset.x * Math.sin(angle)));

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
