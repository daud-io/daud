import { CustomContainer } from "./CustomContainer";
import * as PIXI from "pixi.js";

import { Emitter } from "pixi-particles";
import { ClientBody } from "./cache";
import { getTextureDefinition, TextureDefinition } from "./loader";

export class TextureLayer {
    sprite: PIXI.Sprite | undefined;
    emitter: Emitter | undefined;

    lastTime: number;
    scale: number;

    textureDefinition: TextureDefinition;

    baseRotation: number;

    offset: { x: number; y: number };


    constructor(container: CustomContainer, clientBody: ClientBody, textureName: string) {
        this.scale = 0;
        this.lastTime = 0;
        this.baseRotation = 0;
        this.offset = { x: 0, y: 0 };

        let textureDefinition = getTextureDefinition(textureName);
        this.textureDefinition = textureDefinition;

        const textures = textureDefinition.textures!;

        if (textureDefinition.animated) {
            const animatedSprite = new PIXI.AnimatedSprite(textures);
            animatedSprite.animationSpeed = textureDefinition.animated.speed;
            animatedSprite.play();
            this.sprite = animatedSprite;
            this.scale = textureDefinition.size / textureDefinition.animated.size;
        } else if (textureDefinition.emitter) {
            this.emitter = new Emitter(container.bodyGroup, textures, textureDefinition.emitter);
            this.emitter.emit = true;
            if (textureDefinition.size)
                this.scale = textureDefinition.size;
        } else {
            this.sprite = new PIXI.Sprite(textures[0]);
            this.scale = textureDefinition.size / this.sprite.texture.baseTexture.realHeight;
        }

        if (this.sprite && this.sprite instanceof PIXI.Sprite) {
            if (textureDefinition.tint) this.sprite.tint = textureDefinition.tint;


            if (textureDefinition.offset) {
                this.offset.x = textureDefinition.offset.x;
                this.offset.y = textureDefinition.offset.y;
            }
            this.sprite.anchor.set(0.5);

            container.bodyGroup.addChild(this.sprite);
        }
    }

    tick(time: number, body: ClientBody) {
        if (this.sprite) {

            if (this.offset.x != 0 || this.offset.y != 0) {
                this.sprite.x = (body.Position.x + (this.offset.x * Math.cos(body.Angle) - this.offset.y * Math.sin(body.Angle)));
                this.sprite.y = (body.Position.y + (this.offset.y * Math.cos(body.Angle) + this.offset.x * Math.sin(body.Angle)));
            }
            else {
                this.sprite.x = body.Position.x;
                this.sprite.y = body.Position.y;
            }
            this.sprite.rotation = body.Angle
            this.sprite.scale.set(this.scale * body.Size, this.scale * body.Size);
        }

        if (this.emitter) {
            let scale = this.textureDefinition.emitter.scale;
            let speed = this.textureDefinition.emitter.speed;

            if (scale && speed)
            {
                var startScale = this.emitter.startScale;
                startScale.value = scale.start * this.scale * body.Size;
                startScale.next.value = scale.end * this.scale * body.Size;
                var startSpeed = this.emitter.startSpeed;
                startSpeed.value = speed.start * this.scale * body.Size;
                startSpeed.next.value = speed.end * this.scale * body.Size;
            }
            this.emitter.updateOwnerPos(body.Position.x, body.Position.y);

            if (this.lastTime > 0)
                this.emitter.update((time - this.lastTime) * 0.001);
        }

        this.lastTime = time;
    }

    setZIndex(z: number): void {
        if (this.sprite) this.sprite.zIndex = z;
    }

    destroy(): void {
        (this.sprite) && this.sprite.destroy();
        (this.emitter) && this.emitter.destroy();
        this.sprite = undefined;
        this.emitter = undefined;
    }
}
