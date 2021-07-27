import JSZip from "jszip";
import { EmitterConfig } from "pixi-particles";
import * as PIXI from "pixi.js";
import loaderJSON from "./loader.json";
import { Settings } from "./settings";

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
    const definition = parsedJSON[textureName];
    if (definition == null)
        console.log(`trying to load unknown texture: ${textureName}`);

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

    var textures = await loadBase();

    if (Settings.theme)
        addLayer(textures, await loadTheme(Settings.theme));

    parsedJSON = textures;
}

function addLayer(base:Record<string,TextureDefinition>, patch:Record<string,TextureDefinition>): void
{
    for (let k in patch)
        base[k] = patch[k];
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

async function loadBase(): Promise<Record<string, TextureDefinition>> {
    const textures: any = {};
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
                if (out.url && out.url.indexOf('/') == -1)
                    out.url = "/img/" + out.url;

                if (!out.abstract) await loadTexture(out);
                textures[key] = out;
            })
            .filter((x) => !!x)
    );
    return textures;
}

async function loadTheme(themeName:string): Promise<Record<string, TextureDefinition>> {
    
    const zip = await window
        .fetch(themeName)
        .then((response) => response.blob())
        .then(JSZip.loadAsync);

    const text = await zip.file("daudmod/info.json")!.async("string");
    const info = JSON.parse(text);

    const textures: any = {};

    const all = Object.keys(info).map(async (key) => {
        const defaultKey = info[key].extends;
        if (defaultKey) {
            textures[key] = merge(info[defaultKey], info[key]);
            textures[key].abstract = info[key].abstract;
        } else {
            textures[key] = info[key];
        }

        if (!textures[key].abstract) {
            const file = zip.file(`daudmod/${textures[key].url}`);
            if (!file) throw new Error("Missing file: " + textures[key].url);
            const ab = await file.async("arraybuffer");
            const arrayBufferView = new Uint8Array(ab);
            const blob = new Blob([arrayBufferView], { type: "image/png" });
            const url = URL.createObjectURL(blob);
            textures[key].url = url;
            return await loadTexture(textures[key]);
        }
    });
    await allProgress(all);
    return textures;
}
