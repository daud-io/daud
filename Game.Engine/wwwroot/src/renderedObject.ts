import { CustomContainer } from "./CustomContainer";
import * as PIXI from "pixi.js";

import { Emitter } from "pixi-particles";
import { projectObject } from "./interpolator";
import { ClientBody, ClientGroup } from "./cache";
import { getDefinition, TextureDefinition } from "./loader";
import { Settings } from "./settings";

export class RenderedObject {
    container: CustomContainer;
    body: ClientBody;
    currentSpriteName: string;
    currentModes: number;
    base: PIXI.Container;
    sprite?: PIXI.Sprite | Emitter;
    lastTime = 0;
    layers: Record<string, PIXI.Sprite>;
    baseSpriteDefinition: TextureDefinition;
    caption: string | null;

    constructor(container: CustomContainer, clientBody: ClientBody) {
        this.layers = {};
        this.container = container;
        this.base = new PIXI.Container();
        this.body = clientBody;
        this.currentSpriteName = clientBody.Sprite;
        this.currentModes = 0;
        this.baseSpriteDefinition = getDefinition(clientBody.Sprite);
        this.sprite = this.buildSprite(this.baseSpriteDefinition);
        // this.base.addChild(this.buildSprite(getDefinition("circle")) as PIXI.Sprite);

        this.caption = null;

        if (this.baseSpriteDefinition?.modes?.['default'] != null)
            this.addMode("default");

        if (this.sprite instanceof PIXI.Sprite) {
            this.sprite.zIndex = clientBody.zIndex;
            this.base.zIndex = this.sprite.zIndex;
            this.base.addChild(this.sprite);
        }


        this.container.bodyGroup.addChild(this.base);
    }

    addMode(name: string): void {
        if (this.baseSpriteDefinition == null)
        {
            console.log(`missing mode: ${name} in sprite ${this.currentSpriteName}`);
        }
        else
        {
            const namedMode = this.baseSpriteDefinition?.modes?.[name] || null;
            const layerTextures = namedMode?.split(' ') ?? [];
            for (let textureName of layerTextures) {
                this.layers[textureName] = this.buildSprite(getDefinition(textureName)) as PIXI.Sprite;
                this.base.addChild(this.layers[textureName]);
            }
        }
    }
    deleteMode(name: string): void {

        const namedMode = this.baseSpriteDefinition?.modes?.[name] || null;
        const layerTextures = namedMode?.split(' ') ?? [];
        for (let textureName of layerTextures) {
            this.base.removeChild(this.layers[textureName]);
            delete this.layers[textureName];
            }
    }
    updateModes(): void {
        const mode = this.body.Mode;
        const curr = this.currentModes;
        if ((curr & 1) == 0 && (mode & 1) != 0) this.addMode("boost");
        if ((curr & 1) != 0 && (mode & 1) == 0) this.deleteMode("boost");
        if ((curr & 2) == 0 && (mode & 2) != 0) this.addMode("invulnerable");
        if ((curr & 2) != 0 && (mode & 2) == 0) this.deleteMode("invulnerable");
        if ((curr & 4) == 0 && (mode & 4) != 0) this.addMode("defenseupgrade");
        if ((curr & 4) != 0 && (mode & 4) == 0) this.deleteMode("defenseupgrade");
        if ((curr & 8) == 0 && (mode & 8) != 0) this.addMode("offenseupgrade");
        if ((curr & 8) != 0 && (mode & 8) == 0) this.deleteMode("offenseupgrade");
        if ((curr & 16) == 0 && (mode & 16) != 0) this.addMode("shield");
        if ((curr & 16) != 0 && (mode & 16) == 0) this.deleteMode("shield");
        this.currentModes = mode;
    }
    buildSprite(textureDefinition: TextureDefinition): PIXI.Sprite | Emitter {
        const textures = textureDefinition.textures!;
        let pixiSprite: PIXI.Sprite | Emitter;

        if (textureDefinition.animated) {
            const animatedSprite = new PIXI.AnimatedSprite(textures);
            animatedSprite.animationSpeed = textureDefinition.animated.speed;
            animatedSprite.play();
            pixiSprite = animatedSprite;
        } else if (textureDefinition.emitter) {
            pixiSprite = new Emitter(this.container.emitterContainer, textures, textureDefinition.emitter);
            pixiSprite.emit = true;
        } else {
            pixiSprite = new PIXI.Sprite(textures[0]);
        }

        if (pixiSprite instanceof PIXI.Sprite) {
            if (textureDefinition.tint) pixiSprite.tint = textureDefinition.tint;
            pixiSprite.height = textureDefinition.size;
            pixiSprite.scale.x = pixiSprite.scale.y;
            pixiSprite.anchor.set(0.5, 0.5);
            if (textureDefinition.offset) {
                pixiSprite.x = textureDefinition.offset.x;
                pixiSprite.y = textureDefinition.offset.y;
            }

            if (textureDefinition.rotate) {
                pixiSprite.rotation = (textureDefinition.rotate * Math.PI) / 180;
            }
        }
        return pixiSprite;
    }

    destroy(): void {
        this.container.bodyGroup.removeChild(this.base);
        this.base.destroy({ children: true });

        if (this.sprite instanceof Emitter) {
            this.sprite.destroy();
        }
    }

    tick(time: number): void {
        if (this.body) {
            projectObject(this.body, time);

            this.base.x = this.body.Position.x;
            this.base.y = this.body.Position.y;
            this.base.rotation = this.body.Angle;
            this.base.scale.set(this.body.Size, this.body.Size);

            if (this.sprite instanceof Emitter) {
                this.sprite.minimumScaleMultiplier = this.body.Size;
                this.sprite.updateOwnerPos(this.body.Position.x, this.body.Position.y);
            }
        }

        if (this.lastTime > 0 && this.sprite instanceof Emitter) {
            this.sprite.update((time - this.lastTime) * 0.001);
        }

        this.lastTime = time;
    }

    refresh(): void {
        if (this.sprite instanceof PIXI.Sprite) this.base.removeChild(this.sprite);
        this.currentSpriteName = this.body.Sprite;
        this.sprite = this.buildSprite(getDefinition(this.body.Sprite));
        if (this.sprite instanceof PIXI.Sprite) this.base.addChild(this.sprite);
        this.currentModes = 0;
        this.updateModes();
    }
    update(): void {
        if (this.currentSpriteName != this.body.Sprite) {
            if (this.sprite instanceof PIXI.Sprite) this.base.removeChild(this.sprite);
            this.currentSpriteName = this.body.Sprite;
            this.sprite = this.buildSprite(getDefinition(this.body.Sprite));
            if (this.sprite instanceof PIXI.Sprite) this.base.addChild(this.sprite);
        }
        if (this.currentModes != this.body.Mode) {
            this.updateModes();
        }
    }
}
