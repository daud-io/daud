import * as PIXI from "pixi.js";

import { Settings } from "../settings";
import { textureCache } from "./textureCache";
import { textureMapRules } from "./textureMap";
import { spriteModeMapRules } from "./spriteModeMap";
import emitters from "./emitters.json";
import { CustomContainer } from "../CustomContainer";
import { queryProperties } from "../parser/parseTheme";
import { Emitter, Particle } from "pixi-particles";

declare module "pixi.js" {
    interface Sprite {
        baseScale: number;
        baseOffset: { x: number; y: number };
    }
}
declare module "pixi-particles" {
    interface Emitter {
        renderedObject: RenderedObject;
    }
}
class GroupParticle extends Particle {
    body: any;

    constructor(emitter: Emitter) {
        super(emitter);

        this.body = emitter.renderedObject.body;
    }

    update(delta: number) {
        const ret = super.update(delta);

        if (this.body) this.scaleMultiplier = this.body.Size;
        this.zIndex = 700;

        return ret;
    }
}

export class RenderedObject {
    container: CustomContainer;
    currentSpriteName: string | boolean;
    currentMode: number;
    currentZIndex: number;
    activeTextures: {};
    activeEmitters: {};
    body?: any;
    spriteLayers?: PIXI.Sprite[];
    emitterLayers?: Emitter[];
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

    static getTextureImage(textureName: string) {
        const textureDefinition = RenderedObject.getTextureDefinition(textureName);
        return RenderedObject.getImageFromTextureDefinition(textureDefinition);
    }

    static loadTexture(textureDefinition, textureName) {
        let textures = textureCache[textureName];

        if (!textures) {
            textures = [];

            const img = RenderedObject.getImageFromTextureDefinition(textureDefinition);

            const baseTexture = PIXI.BaseTexture.from(img);

            baseTexture.mipmap = Settings.mipmapping ? PIXI.MIPMAP_MODES.ON : PIXI.MIPMAP_MODES.OFF;

            if (textureDefinition.animated) {
                const tileSize = textureDefinition["tile-size"] || 32;
                const totalTiles = textureDefinition["tile-count"] || 1;

                for (let tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
                    const sx = tileSize * (tileIndex % totalTiles);
                    const sy = 0;
                    const sw = tileSize;
                    const sh = tileSize;
                    const tex = new PIXI.Texture(baseTexture, new PIXI.Rectangle(sx, sy, sw, sh), null, null, textureDefinition.rotate || 0);
                    (tex as any).daudScale = RenderedObject.getScaleWithHeight(textureDefinition, tileSize);
                    textures.push(tex);
                }
            } else if (textureDefinition.map) {
                const imageWidth = textureDefinition["image-width"];
                const imageHeight = textureDefinition["image-height"];
                const tileWidth = textureDefinition["tile-width"];
                const tileHeight = textureDefinition["tile-height"];

                const tilesWide = Math.floor(imageWidth / tileWidth);
                const tilesHigh = Math.floor(imageHeight / tileHeight);

                for (let row = 0; row < tilesHigh; row++)
                    for (let col = 0; col < tilesWide; col++) {
                        const x = Math.floor(col * tileWidth);
                        const y = Math.floor(row * tileHeight);

                        const texture = new PIXI.Texture(baseTexture, new PIXI.Rectangle(x, y, tileWidth, tileHeight));

                        texture.baseTexture.scaleMode = PIXI.SCALE_MODES.NEAREST;
                        (texture as any).daudScale = RenderedObject.getScaleWithHeight(textureDefinition, tileHeight);
                        //texture.scaleMode = PIXI.SCALE_MODES.LINEAR;
                        textures.push(texture);
                    }
            } else if (textureDefinition.emitter) {
            } else {
                const texture = new PIXI.Texture(baseTexture);
                (texture as any).daudScale = RenderedObject.getScaleWithHeight(textureDefinition, baseTexture.realHeight);
                textures.push(texture);
            }

            textureCache[textureName] = textures;
        }

        return textures;
    }

    static getTextureDefinition(textureName: string) {
        const mapKey = this.parseMapKey(textureName);
        if (mapKey) textureName = mapKey.name;

        let textureDefinition = null;
        try {
            textureDefinition = queryProperties({ element: textureName }, textureMapRules[0]);
            for (const i in textureDefinition) {
                textureDefinition[i] = textureDefinition[i].map(function (x) {
                    let k = x;
                    try {
                        const m = JSON.parse(x);
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

    static parseMapKey(mapKey: string) {
        if (!mapKey) return false;

        const mapKeyMatches = mapKey.match(/^(.*)\[(\d*)\]/);

        if (mapKeyMatches)
            return {
                name: mapKeyMatches[1],
                mapID: mapKeyMatches[2],
            };
        else return false;
    }

    buildSprite(textureName: string, spriteName: string): PIXI.Sprite {
        const textureDefinition = RenderedObject.getTextureDefinition(textureName);
        const textures = RenderedObject.loadTexture(textureDefinition, textureName);
        let pixiSprite = null;

        if (textureDefinition.animated) {
            pixiSprite = new PIXI.AnimatedSprite(textures);
            if (pixiSprite instanceof PIXI.AnimatedSprite) {
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

        pixiSprite.baseScale = (textures as any)[0].daudScale;
        pixiSprite.scale = (textures as any)[0].daudScale;
        (pixiSprite as any).textureDefinition = textureDefinition;

        pixiSprite.baseOffset = textureDefinition["offset-x"] ? { x: textureDefinition["offset-x"], y: textureDefinition["offset-y"] } : { x: 0, y: 0 };

        if (textureDefinition.animated && pixiSprite instanceof PIXI.AnimatedSprite) pixiSprite.play();

        return pixiSprite;
    }
    static getScale(textureDefinition, pixiTex): number {
        let spriteSize = 1;
        if (textureDefinition["size"]) {
            const spriteSizeIsPercent = typeof textureDefinition["size"] == "string" && textureDefinition["size"][textureDefinition["size"].length - 1] == "%";
            spriteSize = spriteSizeIsPercent ? parseFloat(textureDefinition["size"].slice(0, textureDefinition["size"].length - 1)) / 100 : parseFloat(textureDefinition["size"]) / pixiTex.height;
        }
        if (textureDefinition["scale"]) {
            spriteSize = parseFloat(textureDefinition["scale"]);
        }
        return spriteSize;
    }
    static getScaleWithHeight(textureDefinition, height: number): number {
        let spriteSize = 1;
        if (textureDefinition["size"]) {
            const spriteSizeIsPercent = typeof textureDefinition["size"] == "string" && textureDefinition["size"][textureDefinition["size"].length - 1] == "%";
            spriteSize = spriteSizeIsPercent ? parseFloat(textureDefinition["size"].slice(0, textureDefinition["size"].length - 1)) / 100 : parseFloat(textureDefinition["size"]) / height;
        }
        if (textureDefinition["scale"]) {
            spriteSize = parseFloat(textureDefinition["scale"]);
        }
        return spriteSize;
    }
    static getSpriteDefinition(spriteName: string, additional?: string[]): any {
        let spriteDefinition = null;
        if (!additional) {
            additional = [];
        }
        const mapKey = this.parseMapKey(spriteName);
        if (mapKey) spriteName = mapKey.name;
        try {
            spriteDefinition = queryProperties({ element: spriteName.split("_")[0], class: spriteName.split("_").join(" ") + " " + additional.join(" ") }, spriteModeMapRules[0]);
            for (const i in spriteDefinition) {
                spriteDefinition[i] = spriteDefinition[i].map(function (x) {
                    let k = x;
                    try {
                        const m = JSON.parse(x);
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

    getModeMap(spriteName: string, mode) {
        const layers = [];
        const modes = this.decodeModes(mode);

        const spriteDefinition = RenderedObject.getSpriteDefinition(spriteName, modes);

        return spriteDefinition.textures;
    }

    buildSpriteLayers(spriteName: string, mode, zIndex: number) {
        const layers = this.getModeMap(spriteName, mode);

        if (layers) {
            const spriteLayers = [];
            for (let i = 0; i < layers.length; i++) {
                let spriteLayer = null;
                const textureName = layers[i];

                if (this.activeTextures[textureName]) spriteLayer = this.activeTextures[textureName];
                else {
                    //console.log('building sprite for ' + textureName);
                    spriteLayer = this.buildSprite(textureName, spriteName);
                }

                if (spriteLayer != null) {
                    if (zIndex == 0) zIndex = 250;

                    spriteLayer.zIndex = 1000 + i - this.body.ID / 100000 - zIndex;

                    spriteLayers.push(spriteLayer);
                    this.activeTextures[textureName] = spriteLayer;
                }
            }

            for (const key in this.activeTextures) {
                if (layers.indexOf(key) == -1) {
                    const layer = this.activeTextures[key];
                    this.container.removeChild(layer);
                    layer.destroy();
                    //console.log(`delete sprite layer ${spriteName}:${key}`);
                    delete this.activeTextures[key];
                }
            }

            return spriteLayers;
        } else return undefined;
    }

    buildEmitterLayers(spriteName: string, mode, zIndex: number) {
        const layers = this.getModeMap(spriteName, mode);

        if (layers) {
            const emitterLayers: Emitter[] = [];
            for (let i = 0; i < layers.length; i++) {
                let emitterLayer: Emitter | null = null;
                const textureName = layers[i];

                if (this.activeEmitters[textureName]) emitterLayer = this.activeEmitters[textureName];
                else {
                    const textureDefinition = RenderedObject.getTextureDefinition(textureName);

                    if (textureDefinition.emitter) {
                        const particleTextureName = textureDefinition.particle;
                        const particleTextures = RenderedObject.loadTexture(RenderedObject.getTextureDefinition(particleTextureName), particleTextureName);

                        if (typeof textureDefinition.emitter == "string") textureDefinition.emitter = emitters[textureDefinition.emitter];

                        emitterLayer = new Emitter(this.container.emitterContainer, particleTextures, textureDefinition.emitter);
                        emitterLayer.emit = true;
                        emitterLayer.renderedObject = this;

                        emitterLayer.particleConstructor = GroupParticle;
                    }
                }

                if (emitterLayer != null) {
                    if (zIndex == 0) zIndex = 250;

                    // emitterLayer.zIndex = 1000 + i - zIndex - this.body.ID / 100000;

                    emitterLayers.push(emitterLayer);
                    this.activeEmitters[textureName] = emitterLayer;
                }
            }

            for (const key in this.activeEmitters) {
                if (layers.indexOf(key) == -1) {
                    const layer = this.activeEmitters[key];
                    this.container.removeChild(layer);
                    layer.destroy();
                    delete this.activeEmitters[key];
                }
            }

            return emitterLayers;
        } else return undefined;
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

            this.spriteLayers = undefined;
            this.activeTextures = {};
        }

        if (this.emitterLayers) {
            for (const layer of this.emitterLayers) {
                // this.container.removeChild(layer);
                layer.destroy();
            }

            this.emitterLayers = undefined;
        }
    }

    refreshSprite() {
        this.setSprite(this.currentSpriteName as string, this.currentMode, this.currentZIndex, true);
    }

    setSprite(spriteName: string, mode, zIndex: number, reload = false) {
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

            // also adds them to the container
            this.emitterLayers = this.buildEmitterLayers(spriteName, mode, zIndex);
        }
    }
    fixLoadingTextureScales() {
        this.foreachLayer(function (layer, index) {
            if ((layer as any).textureDefinition) layer.baseScale = RenderedObject.getScaleWithHeight((layer as any).textureDefinition, layer.texture.height);
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

            this.foreachEmitter((e) => {
                e.update((time - this.lastTime) * 0.001);
            });
        }

        this.lastTime = time;
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

        this.foreachEmitter(function (emitter) {
            //console.log(`updating emitter ${interpolatedPosition.x},${interpolatedPosition.y}`);
            emitter.updateOwnerPos(interpolatedPosition.x, interpolatedPosition.y);
        });
    }

    update(updateData) {
        this.body = updateData;

        this.setSprite(updateData.Sprite, updateData.Mode, updateData.zIndex);
    }

    foreachLayer(action: (layer: PIXI.Sprite, index: number) => void) {
        if (this.spriteLayers && this.spriteLayers.length)
            this.spriteLayers.forEach((layer, i) => {
                action.apply(this, [layer, i]);
            });
    }

    foreachEmitter(action: (layer: Emitter, index: number) => void) {
        //console.log(`enumerating this.emitterLayers.length ${this.emitterLayers.length}`);
        if (this.emitterLayers && this.emitterLayers.length)
            this.emitterLayers.forEach((layer, i) => {
                action.apply(this, [layer, i]);
            });
    }
}
