import { EmitterConfig } from "pixi-particles";
import * as PIXI from "pixi.js";
import loaderJSON from "./loader.json";

let parsedJSON: Record<string, TextureDefinition> = {};
export type TextureDefinition = {
    extends: string;
    url: string;
    animated?: { size: number; count: number; speed: number };
    emitter: EmitterConfig;
    offset?: { x: number; y: number };
    rotate: number;
    tint?: number;
    size: number;
    abstract: boolean;
    textures?: PIXI.Texture[];
    modes?: Record<string, string>;
};
export function getDefinition(textureName: string): TextureDefinition {
    return parsedJSON[textureName];
}
export function setDefinitions(newJSON: Record<string, TextureDefinition>): void {
    parsedJSON = newJSON;
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
    await allProgress(
        Object.keys(loaderJSON)
            .map(async (key) => {
                let out: TextureDefinition;
                const defaultKey = loaderJSON[key].extends;
                if (defaultKey) {
                    out = merge(loaderJSON[defaultKey], loaderJSON[key]);
                    out.abstract = loaderJSON[key].abstract;
                } else {
                    out = loaderJSON[key];
                }
                if (!out.abstract) await loadTexture(out);
                parsedJSON[key] = out;
            })
            .filter((x) => !!x)
    );
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
