import { GameContainer } from "./gameContainer";

import { ClientBody } from "./cache";
import { getTextureDefinition, TextureDefinition } from "./loader";
import { Sprite } from "@babylonjs/core";
import { projectObject } from "./interpolator";

export class TextureLayer {
    private sprite: Sprite | undefined;
    emitter: any | undefined;

    lastTime: number;

    textureDefinition: TextureDefinition;

    offset: { x: number; y: number };
    aspectRatio: number;

    constructor(container: GameContainer, clientBody: ClientBody, textureName: string) {
        this.lastTime = 0;
        this.offset = { x: 0, y: 0 };
        this.aspectRatio = 1;

        let textureDefinition = getTextureDefinition(textureName);
        this.textureDefinition = textureDefinition;

        if (textureDefinition.emitter) {
            /*this.emitter = new Emitter(container.bodyGroup, textures, textureDefinition.emitter);
            this.emitter.emit = true;
            if (textureDefinition.size)
                this.scale = textureDefinition.size;*/
        } else {
            if (textureDefinition.spriteManager)
            {
                this.sprite = new Sprite(textureName, textureDefinition.spriteManager);
                this.sprite.position.y = 0;

                projectObject(clientBody, clientBody.DefinitionTime);
                this.updateFromBody(clientBody);

                if (textureDefinition.animated) {
                    this.sprite.playAnimation(0, textureDefinition.animated.count, true, textureDefinition.animated.speed);
                }

                if (this.textureDefinition.width == undefined || this.textureDefinition.height == undefined)
                    console.log(`TextureDefinition[${textureName}] is missing height/width`);
                else
                    this.aspectRatio = this.textureDefinition.width/this.textureDefinition.height;
            }
        }

        if (this.sprite && this.sprite instanceof Sprite) {
            /*if (textureDefinition.tint)
                this.sprite.tint = textureDefinition.tint;
            */

            if (textureDefinition.offset) {
                this.offset.x = textureDefinition.offset.x;
                this.offset.y = textureDefinition.offset.y;
            }
        }
    }

    updateFromBody(body: ClientBody) {
        if (this.sprite) {
            if (this.offset.x != 0 || this.offset.y != 0) {
                this.sprite.position.x = (body.Position.x + (this.offset.x * Math.cos(body.Angle) - this.offset.y * Math.sin(body.Angle)));
                this.sprite.position.z = (body.Position.y + (this.offset.y * Math.cos(body.Angle) + this.offset.x * Math.sin(body.Angle)));
            }
            else {
                this.sprite.position.x = body.Position.x;
                this.sprite.position.z = body.Position.y;
            }

            let extraRotation = 0;
            if (this.textureDefinition.rotate)
                extraRotation = -Math.PI/2;

            this.sprite.angle = body.Angle + Math.PI + extraRotation;
            this.sprite.height = this.textureDefinition.size * body.Size; 
            this.sprite.width = this.sprite.height * this.aspectRatio;
        }
    }

    tick(time: number, body: ClientBody) {
        this.updateFromBody(body);

        if (this.emitter) {
            /*let scale = this.textureDefinition.emitter.scale;
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
                this.emitter.update((time - this.lastTime) * 0.001);*/
        }

        this.lastTime = time;
    }

    setZIndex(z: number): void {
        if (this.sprite) this.sprite.position.y = z;
    }

    destroy(): void {
        this.sprite?.dispose();
        this.emitter?.dispose();
        this.sprite = undefined;
        this.emitter = undefined;
    }
}
