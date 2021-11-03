import { GameContainer } from "./gameContainer";
import { projectObject } from "./interpolator";
import { ClientBody, ClientGroup } from "./cache";
import { TextureLayer } from "./textureLayer";
import { SpriteDefinition } from "./loader";

export class ObjectMode {
    name: string;
    visible: boolean;
    layers?: TextureLayer[];

    constructor(name:string)
    {
        this.name = name;
        this.visible = false;
    }
};

export class RenderedObject {
    container: GameContainer;
    body: ClientBody;

    modes: ObjectMode[];
    textureLayers: Record<string, TextureLayer>;

    baseSpriteDefinition: SpriteDefinition;
    currentZIndex?: number;
    currentSpriteName?: string;
    currentMode?: number;

    constructor(container: GameContainer, clientBody: ClientBody) {
        this.textureLayers = {};

        if (!container) throw "bad container";

        this.container = container;
        this.body = clientBody;

        this.baseSpriteDefinition = this.container.loader.getSpriteDefinition(clientBody.Sprite);

        this.modes = this.setupModes();
        this.update();
    }

    setupModes() : ObjectMode[]
    {
        return [
            new ObjectMode("default")
        ];
    }

    updateMode(mode: number) {
        this.modes[0].visible = true;
        this.currentMode = mode;
    }

    resetTextureLayers(): void {
        for (let key in this.textureLayers) {
            let textureLayer = this.textureLayers[key];
            textureLayer.dispose();
        }

        this.textureLayers = {};
        this.currentMode = -1;
    }

    dispose(): void {
        this.resetTextureLayers();
    }

    tick(time: number): void {
        if (this.body) {
            projectObject(this.body, time);

            for (let key in this.textureLayers) {
                let textureLayer = this.textureLayers[key];
                textureLayer.prerender(time, this.body);
            }
        }
    }

    refresh(): void {
        this.currentMode = -1;
        this.currentSpriteName = "";
        this.update();
    }

    setupForSprite(spriteName: string)
    {
        this.modes = this.setupModes();
        this.currentSpriteName = spriteName;
        this.baseSpriteDefinition = this.container.loader.getSpriteDefinition(this.currentSpriteName);

        for(var i=0; i<this.modes.length; i++)
        {
            let mode = this.modes[i];
            if (mode && !mode.layers)
            {
                mode.layers = [];
                let textureList = this.baseSpriteDefinition.modes?.[mode.name]?.split(" ") ?? [];

                for(let textureIndex=0; textureIndex<textureList.length; textureIndex++)
                {
                    var textureName = textureList[textureIndex];
                    let textureLayer = this.textureLayers[textureName];
                    if (textureLayer == null)
                    {
                        textureLayer = new TextureLayer(this.container, textureName);
                        this.textureLayers[textureName] = textureLayer;
                    }
                    mode.layers.push(textureLayer);
                }
            }
        }
    }

    update(): void {
        if (this.currentSpriteName != this.body.Sprite)
        {
            this.resetTextureLayers();
            this.setupForSprite(this.body.Sprite);
        }

        this.updateTextureLayers();

        if (this.currentMode != this.body.Mode)
        {
            this.updateMode(this.body.Mode);
        }
        
        if (this.currentZIndex != this.body.zIndex)
        {
            this.currentZIndex = this.body.zIndex;
        }

    }

    updateTextureLayers() {
        for(let k in this.textureLayers)
            this.textureLayers[k].visible = false;

        for(var i=0; i<this.modes.length; i++)
        {
            let mode = this.modes[i];
            if (mode && mode.layers)
            {
                for(var layerIndex=0; layerIndex<mode.layers.length; layerIndex++)
                {
                    var layer = mode.layers[layerIndex];

                    if (mode.visible)
                    {
                        let zIndex = this.body.zIndex;
                        if (zIndex == 0)
                            zIndex = 250;
    
                        layer.setZIndex(zIndex - i + this.body.ID / 100000);
                        layer.visible = true;
                    }
                }
            }
        }
    }
}
