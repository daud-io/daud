import { CustomContainer } from "./CustomContainer";
import * as PIXI from "pixi.js";

import { Particle, Emitter } from "pixi-particles";
import { ClientBody, ClientGroup } from "./cache";
import { getDefinition, TextureDefinition } from "./loader";

export class TextureLayer {
    sprite: PIXI.Sprite | null;
    emitter: Emitter | null;

    baseSpriteDefinition: TextureDefinition;
    lastTime: number;
    scale: number;

    constructor(container: CustomContainer, clientBody: ClientBody) {
        this.emitter = null;
        this.sprite = null;
        this.scale = 0;
        this.lastTime = 0;

        this.baseSpriteDefinition = getDefinition(clientBody.Sprite);
        var textureDefinition = this.baseSpriteDefinition;
        const textures = textureDefinition.textures!;

        if (textureDefinition.animated) {
            const animatedSprite = new PIXI.AnimatedSprite(textures);
            animatedSprite.animationSpeed = textureDefinition.animated.speed;
            animatedSprite.play();
            this.sprite = animatedSprite;
        } else if (textureDefinition.emitter) {
            this.emitter = new Emitter(container.emitterContainer, textures, textureDefinition.emitter);
            this.emitter.emit = true;
            (<any>this.emitter).layer = this;
            this.emitter.particleConstructor = GroupParticle;
        } else {
            this.sprite = new PIXI.Sprite(textures[0]);
        }

        if (this.sprite && this.sprite instanceof PIXI.Sprite) {
            if (textureDefinition.tint) this.sprite.tint = textureDefinition.tint;

            //if (textureDefinition.alpha) pixiSprite.alpha = textureDefinition.alpha;
            //if (textureDefinition.blendMode) pixiSprite.blendMode = textureDefinition.blendMode;
            
            this.scale = this.baseSpriteDefinition.size / this.sprite.texture.baseTexture.realHeight;

            this.sprite.anchor.set(0.5); // sets origin to the center
            if (textureDefinition.offset) {
                this.sprite.x = textureDefinition.offset.x;
                this.sprite.y = textureDefinition.offset.y;
            }

            if (textureDefinition.rotate) {
                this.sprite.rotation = (textureDefinition.rotate * Math.PI) / 180;
            }

            container.bodyGroup.addChild(this.sprite);
        }
    }

    tick(time: number, body: ClientBody)
    {
        if (this.sprite)
        {
            this.sprite.x = body.Position.x;
            this.sprite.y = body.Position.y;
            this.sprite.rotation = body.Angle;
            this.sprite.scale.set(this.scale * body.Size, this.scale * body.Size);
        }
        
        if (this.emitter)
        {
            this.emitter.minimumScaleMultiplier = body.Size;
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
        this.sprite = null;
        this.emitter = null;
    }
}

class GroupParticle extends Particle {
    layer: TextureLayer;

    constructor(emitter: Emitter) {
        super(emitter);
        this.layer = (<any>emitter).layer;
    }

    update(delta: number): number {
        var ret = super.update(delta);
        this.scale

//        if (this.layer) this.scaleMultiplier = this.layer.scale;

        return ret;
    }
}
