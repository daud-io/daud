import { GameContainer } from "./gameContainer";
import { projectObject } from "./interpolator";
import { ClientBody, ClientGroup } from "./cache";
import { TextureLayer } from "./textureLayer";
import { SpriteDefinition } from "./loader";
export class RenderedObject {
    container: GameContainer;
    body: ClientBody;
    currentSpriteName: string;
    currentMode: number;

    textureLayers: Record<string, TextureLayer>;

    baseSpriteDefinition: SpriteDefinition;

    constructor(container: GameContainer, clientBody: ClientBody) {
        this.textureLayers = {};

        if (!container) throw "bad container";

        this.container = container;
        this.body = clientBody;
        this.currentSpriteName = "";
        this.currentMode = -1;

        this.baseSpriteDefinition = this.container.loader.getSpriteDefinition(clientBody.Sprite);
    }

    decodeOrderedModes(mode: number) {
        return ["default"];
    }

    destroy(): void {
        for (let key in this.textureLayers) {
            let textureLayer = this.textureLayers[key];
            textureLayer.destroy();
        }

        this.textureLayers = {};
    }

    tick(time: number): void {
        if (this.body) {
            projectObject(this.body, time);

            for (let key in this.textureLayers) {
                let textureLayer = this.textureLayers[key];
                textureLayer.tick(time, this.body);
            }
        }
    }

    refresh(): void {
        this.currentMode = -1;
        this.currentSpriteName = "";
        this.update();
    }

    update(): void {
        let dirty = false;

        if (this.currentSpriteName != this.body.Sprite) {
            this.destroy();
            this.currentSpriteName = this.body.Sprite;
            this.baseSpriteDefinition = this.container.loader.getSpriteDefinition(this.currentSpriteName);
            dirty = true;
        }

        if (this.currentMode != this.body.Mode) {
            this.currentMode = this.body.Mode;
            dirty = true;
        }

        if (dirty) this.updateTextureLayers();
    }

    getOrderedTextures(): string[] {
        const modes = this.decodeOrderedModes(this.currentMode);
        const textures = new Array<string>();
        for (let i = 0; i < modes.length; i++) {
            let mode = modes[i];
            let textureList = <string[]>[];
            let namedMode = this.baseSpriteDefinition.modes?.[mode];
            textureList = namedMode?.split(" ") ?? [];

            for (let t in textureList) if (textures.indexOf(textureList[t]) == -1) textures.push(textureList[t]);
        }

        return textures;
    }

    updateTextureLayers() {
        const textures = this.getOrderedTextures();

        for (let i = 0; i < textures.length; i++) {
            var textureName = textures[i];

            let textureLayer = this.textureLayers[textureName] ?? new TextureLayer(this.container, this.body, textureName);

            if (textureLayer != null) {
                let zIndex = this.body.zIndex;
                if (zIndex == 0) zIndex = 250;

                textureLayer.setZIndex(zIndex - i + this.body.ID / 100000);

                this.textureLayers[textureName] = textureLayer;
            }
        }

        // remove any layers that are now unused
        for (var key in this.textureLayers) {
            if (textures.indexOf(key) == -1) {
                let layer = this.textureLayers[key];
                layer.destroy();
                delete this.textureLayers[key];
            }
        }
    }
}
