import { Settings } from "./settings";

export class Ship {
    constructor() {
        this.size = 0;

        var sprites = {
            "sip_red": {
                file = "ship_red.png",
                scale = 0.03,

            },
            "ship_red_flash": {
                file = "ship_red_flash.png",
                animated = true,
                loop = true,
                animationSpeed: 1.0
            }
        }

        this.spriteData = {
            modes: {
                default: { sprite: "ship_red" },
                invulnerable: { sprite: "ship_red_flash" }
            },
        };
    }

    getTexture() {
        if (this.texture)
            return texture;
        else {
            var sprite = sprites[this.spriteName];

            return new PIXI.Texture.fromLoader(sprite.image);
        }
    }

    createSprite() {
        
    }

    create(container, update) {
        this.container = container;
        this.spriteName = update.Sprite;

        this.spriteObject = this.bodies[`p-${update.ID}`];

        let texture = textures[update.Sprite];

        if (object.texture != texture && !sprite.animated) {
            if (!texture) {
                texture = textures[update.Sprite] = new PIXI.Texture.fromLoader(sprite.image);
            }
            object.pivot.x = sprite.image.width / 2;
            object.pivot.y = sprite.image.height / 2;
            object.texture = texture;
        }
        object.position.x = update.OriginalPosition.X;
        object.position.y = update.OriginalPosition.Y;
        object.rotation = update.OriginalAngle;
        object.scale.set(sprite.scale * update.Size, sprite.scale * update.Size);
    }

    update() {
        hudh.innerHTML =
            `fps: ${window.Game.Stats.framesPerSecond || 0}` +
            ` - players: ${window.Game.Stats.playerCount || 0}` +
            ` - spectators: ${window.Game.Stats.spectatorCount || 0}` +
            ` - ping: ${Math.floor(window.Game.primaryConnection.latency || 0)}`;
        hudh.style.fontFamily = Settings.font;
    }
}
