import { Settings } from "../settings";
import { textureCache } from "./textureCache";
import { textureMapRules } from "./textureMap";
import { spriteModeMapRules } from "./spriteModeMap";
import emitters from "../../emitters.json";
import "pixi.js";
import "pixi-layers";
import * as particles from "pixi-particles";
import { CustomContainer } from "../CustomContainer";
import { queryProperties } from "../parser/parseTheme.js";
import { Sprite } from "pixi.js";

class GroupParticle extends particles.Particle {
    body: any;

    constructor(emitter: particles.Emitter) {
        super(emitter);
        this.parentGroup = emitter.parent.parentGroup;
        this.body = (<any>emitter).renderedObject.body;
    }

    update(delta: number): number {
        var ret = super.update(delta);

        if (this.body) this.scaleMultiplier = this.body.Size;

        return ret;
    }
}

export class RenderedObject {
    container: CustomContainer;
    currentSpriteName: boolean;
    currentMode: number;
    currentZIndex: number;
    activeTextures: {};
    activeEmitters: {};
    body?: any;
    spriteLayers?: any;
    emitterLayers?: any;
    lastTime: number;

    additionalClasses?: string[];
    constructor(container: CustomContainer) {
        this.container = container;
        this.currentSpriteName = false;
        this.currentMode = 0;
        this.currentZIndex = 0;

        this.lastTime = 0;
        this.activeTextures = {};
        this.activeEmitters = {};
    }

    decodeModes(mode) {
        return ["default"];
    }

    static getImageFromTextureDefinition(textureDefinition) {
        const img = new Image();
        if (textureDefinition.url) img.src = textureDefinition.url;
        else {
            const src = `/img/${textureDefinition.file}.png`;
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
                    var tex = new PIXI.Texture(baseTexture, new PIXI.Rectangle(sx, sy, sw, sh), null, null, textureDefinition.rotate || 0);
                    (<any>tex).daudScale = RenderedObject.getScaleWithHeight(textureDefinition, tileSize);
                    textures.push(tex);
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
                        (<any>texture).daudScale = RenderedObject.getScaleWithHeight(textureDefinition, tileHeight);
                        //texture.scaleMode = PIXI.SCALE_MODES.LINEAR;
                        textures.push(texture);
                    }
            } else if (textureDefinition.emitter) {
            } else {
                var texture = new PIXI.Texture(baseTexture);
                (<any>texture).daudScale = RenderedObject.getScaleWithHeight(textureDefinition, baseTexture.realHeight);
                textures.push(texture);
            }

            textureCache[textureName] = textures;
        }

        return textures;
    }

    static getTextureDefinition(textureName) {
        var mapKey = this.parseMapKey(textureName);
        if (mapKey) textureName = mapKey.name;

        var textureDefinition = null;
        try {
            textureDefinition = queryProperties({ element: textureName }, textureMapRules[0]);
            for (var i in textureDefinition) {
                textureDefinition[i] = textureDefinition[i].map(function(x) {
                    var k = x;
                    try {
                        var m = JSON.parse(x);
                        k = m;
                    } finally {
                        return k;
                    }
                });
                if (textureDefinition[i].length < 2) {
                    textureDefinition[i] = textureDefinition[i][0];
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
        } else if (textureDefinition.emitter) {
            return null;
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

        pixiSprite.baseScale = (<any>textures[0]).daudScale;
        pixiSprite.scale = (<any>textures[0]).daudScale;
        (<any>pixiSprite).textureDefinition = textureDefinition;

        pixiSprite.baseOffset = textureDefinition["offset-x"] ? { x: textureDefinition["offset-x"], y: textureDefinition["offset-y"] } : { x: 0, y: 0 };

        if (textureDefinition.animated && pixiSprite instanceof PIXI.extras.AnimatedSprite) pixiSprite.play();

        return pixiSprite;
    }
    static getScale(textureDefinition, pixiTex): number {
        var spriteSize = 1;
        if (textureDefinition["size"]) {
            var spriteSizeIsPercent = typeof textureDefinition["size"] == "string" && textureDefinition["size"][textureDefinition["size"].length - 1] == "%";
            spriteSize = spriteSizeIsPercent ? parseFloat(textureDefinition["size"].slice(0, textureDefinition["size"].length - 1)) / 100 : parseFloat(textureDefinition["size"]) / pixiTex.height;
        }
        if (textureDefinition["scale"]) {
            spriteSize = parseFloat(textureDefinition["scale"]);
        }
        return spriteSize;
    }
    static getScaleWithHeight(textureDefinition, height): number {
        var spriteSize = 1;
        if (textureDefinition["size"]) {
            var spriteSizeIsPercent = typeof textureDefinition["size"] == "string" && textureDefinition["size"][textureDefinition["size"].length - 1] == "%";
            spriteSize = spriteSizeIsPercent ? parseFloat(textureDefinition["size"].slice(0, textureDefinition["size"].length - 1)) / 100 : parseFloat(textureDefinition["size"]) / height;
        }
        if (textureDefinition["scale"]) {
            spriteSize = parseFloat(textureDefinition["scale"]);
        }
        return spriteSize;
    }
    static getSpriteDefinition(spriteName, additional?: string[]): any {
        let spriteDefinition = null;
        if (!additional) {
            additional = [];
        }
        var mapKey = this.parseMapKey(spriteName);
        if (mapKey) spriteName = mapKey.name;
        try {
            spriteDefinition = queryProperties({ element: spriteName.split("_")[0], class: spriteName.split("_").join(" ") + " " + additional.join(" ") }, spriteModeMapRules[0]);
            for (var i in spriteDefinition) {
                spriteDefinition[i] = spriteDefinition[i].map(function(x) {
                    var k = x;
                    try {
                        var m = JSON.parse(x);
                        k = m;
                    } finally {
                        return k;
                    }
                });
                if (i !== "textures" && i !== "layer-textures" && i !== "layer-cpu-levels" && i !== "layer-speeds" && spriteDefinition[i].length < 2) {
                    spriteDefinition[i] = spriteDefinition[i][0];
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

        const spriteDefinition = RenderedObject.getSpriteDefinition(spriteName, modes);

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

                if (spriteLayer != null) {
                    if (zIndex == 0) zIndex = 250;

                    spriteLayer.zOrder = zIndex - i + this.body.ID / 100000;

                    spriteLayers.push(spriteLayer);
                    this.activeTextures[textureName] = spriteLayer;
                }
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

    buildEmitterLayers(spriteName, mode, zIndex) {
        const layers = this.getModeMap(spriteName, mode);

        if (layers) {
            const emitterLayers = [];
            for (let i = 0; i < layers.length; i++) {
                let emitterLayer = null;
                var textureName = layers[i];

                if (this.activeEmitters[textureName]) emitterLayer = this.activeEmitters[textureName];
                else {
                    const textureDefinition = RenderedObject.getTextureDefinition(textureName);

                    if (textureDefinition.emitter) {
                        let particleTextureName = textureDefinition.particle;
                        const particleTextures = RenderedObject.loadTexture(RenderedObject.getTextureDefinition(particleTextureName), particleTextureName);

                        if (typeof textureDefinition.emitter == "string") textureDefinition.emitter = emitters[textureDefinition.emitter];

                        emitterLayer = new particles.Emitter(this.container.emitterContainer, particleTextures, textureDefinition.emitter);
                        emitterLayer.emit = true;
                        emitterLayer.renderedObject = this;

                        let self = this;
                        emitterLayer.particleConstructor = GroupParticle;
                    }
                }

                if (emitterLayer != null) {
                    if (zIndex == 0) zIndex = 250;

                    emitterLayer.zOrder = zIndex - i + this.body.ID / 100000;

                    emitterLayers.push(emitterLayer);
                    this.activeEmitters[textureName] = emitterLayer;
                }
            }

            for (var key in this.activeEmitters) {
                if (layers.indexOf(key) == -1) {
                    let layer = this.activeEmitters[key];
                    this.container.removeChild(layer);
                    layer.destroy();
                    delete this.activeEmitters[key];
                }
            }

            return emitterLayers;
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

        if (this.emitterLayers) {
            for (const layer of this.emitterLayers) {
                this.container.removeChild(layer);
                layer.destroy();
            }

            this.emitterLayers = false;
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

            // also adds them to the container
            this.emitterLayers = this.buildEmitterLayers(spriteName, mode, zIndex);
        }
    }
    fixLoadingTextureScales() {
        this.foreachLayer(function(layer, index) {
            if ((<any>layer).textureDefinition) layer.baseScale = RenderedObject.getScaleWithHeight((<any>layer).textureDefinition, layer.texture.height);
        });
    }
    preRender(time, interpolator) {
        this.fixLoadingTextureScales();
        if (this.body) {
            const newPosition = interpolator.projectObject(this.body, time);
            this.moveSprites(newPosition, this.body.Size);
        }

        if (this.lastTime > 0) {
            //console.log(`update emitters (${time}-${this.lastTime} = ${time - this.lastTime}) * 0.001 = ${(time - this.lastTime) * 0.001}) `);

            this.foreachEmitter(e => {
                e.update((time - this.lastTime) * 0.001);
            });
        }

        this.lastTime = time;
    }

    moveSprites(interpolatedPosition, size) {
        const angle = interpolatedPosition.Angle;

        this.foreachLayer(function(layer, index) {
            layer.pivot.x = Math.floor(layer.texture.width / 2);
            layer.pivot.y = Math.floor(layer.texture.height / 2);

            layer.position.x = Math.floor(interpolatedPosition.x + (layer.baseOffset.x * Math.cos(angle) - layer.baseOffset.y * Math.sin(angle)));

            layer.position.y = Math.floor(interpolatedPosition.y + (layer.baseOffset.y * Math.cos(angle) + layer.baseOffset.x * Math.sin(angle)));

            layer.rotation = angle;

            layer.scale.set(size * layer.baseScale, size * layer.baseScale);
        });

        this.foreachEmitter(function(emitter) {
            //console.log(`updating emitter ${interpolatedPosition.x},${interpolatedPosition.y}`);
            emitter.updateOwnerPos(interpolatedPosition.x, interpolatedPosition.y);
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

    foreachEmitter(action) {
        //console.log(`enumerating this.emitterLayers.length ${this.emitterLayers.length}`);
        if (this.emitterLayers && this.emitterLayers.length)
            this.emitterLayers.forEach((layer, i) => {
                action.apply(this, [layer, i]);
            });
    }
}
