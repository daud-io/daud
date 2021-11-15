import { GameContainer } from "./gameContainer";

import { ClientBody } from "./cache";
import { TextureDefinition } from "./loader";
import { Color4, Sprite } from "@babylonjs/core";

export class TextureLayer {
    private readonly sprite: Sprite | undefined;
    private readonly textureDefinition: TextureDefinition;

    private readonly aspectRatio: number = 1;
    lastBodySize: number = -1;
    visible: boolean = false;

    constructor(container: GameContainer, textureName: string) {
        this.aspectRatio = 1;
        
        this.textureDefinition = container.loader.getTextureDefinition(textureName);
        
        if (!this.textureDefinition.particleTexture) {
            if (this.textureDefinition.spriteManager) {
                this.sprite = new Sprite(textureName, this.textureDefinition.spriteManager);
                this.sprite.isVisible = false;
                this.sprite.position.y = 0;

                if (this.textureDefinition.animated)
                    this.startAnimation();

                if (this.textureDefinition.tint)
                    this.sprite.color = Color4.FromHexString(this.textureDefinition.tint);

                if (this.textureDefinition.width == undefined || this.textureDefinition.height == undefined)
                    console.log(`TextureDefinition[${textureName}] is missing height/width`);
                else
                    this.aspectRatio = this.textureDefinition.width / this.textureDefinition.height;

            }
        }
    }

    startAnimation()
    {
        if (this.sprite && this.textureDefinition.animated)
            this.sprite.playAnimation(
                0,
                this.textureDefinition.animated.count - 1,
                this.textureDefinition.animated.loop ?? false,
                this.textureDefinition.animated.speed
            );
    }

    prerender(time: number, body: ClientBody) {
        if (this.sprite) {
            if (this.textureDefinition.offset) {
                this.sprite.position.x = body.Position.x + (this.textureDefinition.offset.x * Math.cos(body.Angle) - this.textureDefinition.offset.y * Math.sin(body.Angle));
                this.sprite.position.z = body.Position.y + (this.textureDefinition.offset.y * Math.cos(body.Angle) + this.textureDefinition.offset.x * Math.sin(body.Angle));
            } else {
                this.sprite.position.x = body.Position.x;
                this.sprite.position.z = body.Position.y;
            }

            let extraRotation = 0;
            if (this.textureDefinition.rotate)
                extraRotation = -Math.PI / 2;

            this.sprite.angle = body.Angle + extraRotation;

            if (this.lastBodySize != body.Size)
            {
                this.lastBodySize = body.Size;
                
                if (this.textureDefinition.size)
                    this.sprite.height = this.textureDefinition.size * body.Size;
                    
                this.sprite.width = this.sprite.height * this.aspectRatio;
            }
        }

        if (this.sprite && this.sprite?.isVisible != this.visible)
        {
            this.sprite.isVisible = this.visible;

            if (this.sprite.isVisible && this.textureDefinition.animated)
                this.startAnimation();
        }
    }

    setZIndex(z: number): void {
        if (this.sprite) this.sprite.position.y = z;
    }

    dispose(): void {
        this.sprite?.dispose();
    }
}

