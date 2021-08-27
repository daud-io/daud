import JSZip from "jszip";
import { OldEmitterConfig } from "pixi-particles";
import * as PIXI from "pixi.js";
import { Settings } from "./settings";
import { SpriteLibrary } from "./spriteLibrary";

let textureDefinitions: Record<string, TextureDefinition> = {};
export type TextureDefinition = {
    extends: string;
    url: string;
    animated?: { size: number; count: number; speed: number };
    emitter: OldEmitterConfig;
    offset?: { x: number; y: number };
    rotate?: number;
    tint?: number;
    size: number;
    abstract: boolean;
    textures?: PIXI.Texture[];
};
let spriteDefinitions: Record<string, SpriteDefinition> = {};
export type SpriteDefinition = {
    extends: string;
    modes?: Record<string, string>;
};
export function getTextureDefinition(textureName: string): TextureDefinition {
    const definition = textureDefinitions[textureName];
    if (definition == null)
        console.log(`trying to load unknown texture: ${textureName}`);

    return definition;
}
export function getSpriteDefinition(spriteName: string): SpriteDefinition {
    const definition = spriteDefinitions[spriteName];
    if (definition == null)
        console.log(`trying to load unknown sprite: ${spriteName}`);

    return definition;
}

export function loadTexture(textureDefinition: TextureDefinition): Promise<void> {
    const textures: PIXI.Texture[] = [];
    const baseTexture = PIXI.BaseTexture.from(textureDefinition.url);

    return new Promise((resolve) => {
        const cb = () => {
            if (textureDefinition.animated) {
                const tileSize = textureDefinition.animated.size;
                const totalTiles = textureDefinition.animated.count;

                for (let tileIndex = 0; tileIndex < totalTiles; tileIndex++) {
                    const sx = (tileSize * tileIndex) % baseTexture.realWidth;
                    const sy = tileSize * Math.floor((tileSize * tileIndex) / baseTexture.realWidth);
                    const sw = tileSize;
                    const sh = tileSize;
                    const tex = new PIXI.Texture(baseTexture, new PIXI.Rectangle(sx, sy, sw, sh));
                    if (textureDefinition.rotate)
                        tex.rotate = textureDefinition.rotate;
                        
                    textures.push(tex);
                }
            } else {
                const texture = new PIXI.Texture(baseTexture);
                textures.push(texture);
            }
            textureDefinition.textures = textures;
            resolve();
        };
        if (baseTexture.valid) cb();
        else baseTexture.once("loaded", cb);
    });
}

interface IObject {
    [key: string]: any;
}

type TUnionToIntersection<U> = (U extends any ? (k: U) => void : never) extends (k: infer I) => void ? I : never;

const isObject = (obj: any) => {
    const type = typeof obj;
    return type === "function" || (type === "object" && !!obj);
};

export const merge = <T extends IObject[]>(...objects: T): TUnionToIntersection<T[number]> =>
    objects.reduce((result, current) => {
        Object.keys(current).forEach((key) => {
            if (Array.isArray(result[key]) && Array.isArray(current[key])) {
                result[key] = Array.from(new Set(result[key].concat(current[key])));
            } else if (isObject(result[key]) && isObject(current[key])) {
                result[key] = merge(result[key], current[key]);
            } else {
                result[key] = current[key];
            }
        });

        return result;
    }, {}) as any;

const progressEl = document.getElementById("loader") as HTMLProgressElement;
export async function load(): Promise<void> {

    // load all the libraries
    let library = await SpriteLibrary.load("assets/base");
    addLayer(library, await SpriteLibrary.load("assets/ctf"));
    addLayer(library, await SpriteLibrary.load("assets/themes/" + Settings.theme));

    spriteDefinitions = library.sprites;
    textureDefinitions = library.textures;

    // load the textures
    await allProgress(
        Object.keys(textureDefinitions)
            .map(async (key) => {
                var texture = textureDefinitions[key];
                if (!texture.abstract)
                {
                    await loadTexture(texture);
                }
            })
            .filter((x) => !!x)
        );
}

function addLayer(base:SpriteLibrary, patch:SpriteLibrary): void
{
    for (let k in patch.sprites)
        base.sprites[k] = patch.sprites[k];
    for (let k in patch.textures)
        base.textures[k] = patch.textures[k];
}

export async function allProgress<T>(proms: Promise<T>[]): Promise<void> {
    let d = 0;
    progressEl.style.display = "";
    progressEl.value = 0;
    for (const p of proms) {
        p.then(() => {
            d++;
            progressEl.value = 255 * (d / proms.length);
        });
    }
    await Promise.all(proms);
    progressEl.style.display = "none";
}
