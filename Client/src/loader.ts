import { SpriteManager, Texture } from "@babylonjs/core";
import { GameContainer } from "./gameContainer";
import { Settings } from "./settings";
import { SpriteLibrary } from "./spriteLibrary";
import * as bus from "./bus";

export type TextureDefinition = {
    extends?: string;
    url?: string;
    animated?: { size: number; count: number; speed: number; loop?: boolean };
    emitter?: any;
    offset?: { x: number; y: number };
    rotate?: number;
    tint?: string;
    size?: number;
    width?: number;
    height?: number;
    abstract?: boolean;
    spriteManager?: SpriteManager;
    particleTexture?: Texture;
};
export type SpriteDefinition = {
    extends?: string;
    modes?: Record<string, string>;
    textures?: TextureDefinition[];
};

export class Loader {
    container: GameContainer;
    //progressEl: HTMLProgressElement;

    spriteDefinitions: Record<string, SpriteDefinition> = {};
    textureDefinitions: Record<string, TextureDefinition> = {};

    constructor(container: GameContainer) {
        this.container = container;
        ///this.progressEl = document.getElementById("loader") as HTMLProgressElement;
    }

    getTextureDefinition(textureName: string): TextureDefinition {
        const definition = this.textureDefinitions[textureName];
        if (definition == null) console.log(`trying to load unknown texture: ${textureName}`);

        return definition;
    }
    getSpriteDefinition(spriteName: string): SpriteDefinition {
        const definition = this.spriteDefinitions[spriteName];
        if (definition == null) console.log(`trying to load unknown sprite: ${spriteName}`);

        return definition;
    }

    loadTexture(container: GameContainer, textureKey: string): Promise<void> {
        return new Promise((resolve) => {
            var def = this.textureDefinitions[textureKey];
            if (def.abstract) resolve();
            else {
                if (def.animated && def.url) {
                    const cellSize = def.animated.size;
                    const totalCells = def.animated.count;

                    def.spriteManager = new SpriteManager(
                        `texture-${textureKey}`,
                        def.url,
                        1000,
                        {
                            width: cellSize,
                            height: cellSize,
                        },
                        container.scene
                    );

                    /*if (def.rotate)
                        tex.rotate = def.rotate;*/
                } else if (def.emitter && def.url) {
                    def.particleTexture = new Texture(def.url, container.scene);
                    def.particleTexture.hasAlpha = true;
                } else {
                    if (def.url)
                        def.spriteManager = new SpriteManager(
                            `texture-${textureKey}`,
                            def.url,
                            1000,
                            {
                                width: def.width,
                                height: def.height,
                            },
                            container.scene
                        );
                }

                // preferrably wait :/
                resolve();
            }
        });
    }

    async load(): Promise<void> {
        this.spriteDefinitions = {};
        this.textureDefinitions = {};

        // load all the libraries
        let library = await SpriteLibrary.load("assets/base");
        //this.addLayer(library, await SpriteLibrary.load("assets/ctf"));
        this.addLayer(library, await SpriteLibrary.load("assets/themes/" + Settings.theme));

        this.spriteDefinitions = library.sprites;
        this.textureDefinitions = library.textures;

        // load the textures
        await this.allProgress(
            Object.keys(this.textureDefinitions)
                .map(async (textureKey) => {
                    await this.loadTexture(this.container, textureKey);
                })
                .filter((x) => !!x)
        );

        bus.emit("loaded");
    }

    addLayer(base: SpriteLibrary, patch: SpriteLibrary): void {
        for (let k in patch.sprites) base.sprites[k] = patch.sprites[k];
        for (let k in patch.textures) base.textures[k] = patch.textures[k];
    }

    static merge<T extends IObject[]>(...objects: T): TUnionToIntersection<T[number]> {
        return objects.reduce((result, current) => {
            Object.keys(current).forEach((key) => {
                if (Array.isArray(result[key]) && Array.isArray(current[key])) {
                    result[key] = Array.from(new Set(result[key].concat(current[key])));
                } else if (isObject(result[key]) && isObject(current[key])) {
                    result[key] = this.merge(result[key], current[key]);
                } else {
                    result[key] = current[key];
                }
            });

            return result;
        }, {}) as any;
    }

    async allProgress<T>(proms: Promise<T>[]): Promise<void> {
        let d = 0;
        //this.progressEl.style.display = "";
        //this.progressEl.value = 0;
        for (const p of proms) {
            p.then(() => {
                d++;
                //this.progressEl.value = 255 * (d / proms.length);
            });
        }
        await Promise.all(proms);
        //this.progressEl.style.display = "none";
    }
}

interface IObject {
    [key: string]: any;
}

type TUnionToIntersection<U> = (U extends any ? (k: U) => void : never) extends (k: infer I) => void ? I : never;

const isObject = (obj: any) => {
    const type = typeof obj;
    return type === "function" || (type === "object" && !!obj);
};


