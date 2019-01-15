import { Settings } from "../settings";
import { sprites } from "../renderer";

export class FleetRenderer {
    constructor(context, settings = {}) {
        this.context = context;
        this.thrusterSpriteSize = 64;
        this.pickupSpriteSize = 72;
    }

    draw(cache, interpolator, currentTime, object, group, position) {
        // draw the sprite
        var ctx = this.context;
        const sprite = object.Sprite != null ? sprites[object.Sprite] : false;
        if (sprite) {
            ctx.save();
            ctx.translate(position.X, position.Y);

            const spriteWidth = sprite.image.width;
            const spriteHeight = sprite.image.height;

            ctx.rotate(position.Angle);

            ctx.scale(sprite.scale, sprite.scale);

            if (sprite.scaleToSize) ctx.scale(object.Size, object.Size);

            if (false && object.Mode == 2) {
                ctx.save();
                this.drawPickup(object, group);
                ctx.restore();
            }

            ctx.drawImage(sprite.image, -spriteWidth / 2, -spriteHeight / 2, spriteWidth, spriteHeight);
            if (object.Mode == 1 || group.group.boostAnimateUntil > performance.now())
                this.drawBooster(object, group, sprite);
            else
                group.group.index = 0;

            ctx.restore();
        }
    }
    drawPickup(object, group) {
        var index = group.group.pickupIndex || 0;
        var pickupAnimationTotal = 1000;
        var pickupSprite = false;

        if (index == 0) {
            group.group.pickupStarted = performance.now();
        }

        var timeIndex = performance.now() - group.group.pickupStarted;

        group.group.pickupIndex = index + 1;

        if (!pickupSprite)
            pickupSprite = sprites['circles']
        var spriteScale = 1.5;

        var totalTiles = (pickupSprite.image.width / this.pickupSpriteSize);
        var spriteIndex = (Math.floor(timeIndex / pickupAnimationTotal * totalTiles)
            + object.ID % 3) % totalTiles;

        var sx = this.pickupSpriteSize * (spriteIndex % totalTiles);
        var sy = 0;
        var sw = this.pickupSpriteSize;
        var sh = this.pickupSpriteSize;

        var dx = -0.5 * this.pickupSpriteSize * spriteScale;
        var dy = -0.5 * this.pickupSpriteSize * spriteScale;
        var dw = this.pickupSpriteSize * spriteScale;
        var dh = this.pickupSpriteSize * spriteScale;

        //this.context.translate(dx, dy);
        //this.context.rotate(Math.PI / 2);

        this.context.drawImage(pickupSprite.image, sx, sy, sw, sh, dx, dy, dw, dh);
    }

    drawBooster(object, group, baseSprite) {
        var index = group.group.index || 0;
        var boostDurationTotal = 600;
        if (index == 0) {
            group.group.boostStarted = performance.now();
            group.group.boostAnimateUntil = performance.now() + boostDurationTotal;
        }

        var boostDuration = performance.now() - group.group.boostStarted;

        group.group.index = index + 1;
        group.group.spriteScale = 1.5;

        var distance = -20;

        var thrusterSprite = group.group.thursterSprite;
        if (Settings.theme == '') {
            if (!thrusterSprite) {
                switch (baseSprite.name) {
                    case "ship_green":
                        thrusterSprite = sprites['thruster_default_green'];
                        break;
                    case "ship_orange":
                        thrusterSprite = sprites['thruster_default_orange'];
                        break;
                    case "ship_pink":
                        thrusterSprite = sprites['thruster_default_pink'];
                        break;
                    case "ship_red":
                        thrusterSprite = sprites['thruster_default_red'];
                        break;
                    case "ship_cyan":
                        thrusterSprite = sprites['thruster_default_cyan'];
                        break;
                    case "ship_yellow":
                        thrusterSprite = sprites['thruster_default_yellow'];
                        break;
                }
            }
            distance = -40;
        }

        if (Settings.theme == '3ds2agh4z76feci') {
            if (!thrusterSprite) {
                switch (baseSprite.name) {
                    case "ship_green":
                        thrusterSprite = sprites['thruster_retro_green'];
                        break;
                    case "ship_orange":
                        thrusterSprite = sprites['thruster_retro_orange'];
                        break;
                    case "ship_pink":
                        thrusterSprite = sprites['thruster_retro_pink'];
                        break;
                    case "ship_red":
                        thrusterSprite = sprites['thruster_retro_red'];
                        break;
                    case "ship_cyan":
                        thrusterSprite = sprites['thruster_retro_cyan'];
                        break;
                    case "ship_yellow":
                        thrusterSprite = sprites['thruster_retro_yellow'];
                        break;
                }
            }
            distance = -20;
            group.group.spriteScale = 1;
        }

        if (!thrusterSprite)
            thrusterSprite = sprites['thruster_default_cyan'];

        var totalTiles = (thrusterSprite.image.width / this.thrusterSpriteSize);
        var spriteIndex = Math.floor(boostDuration / boostDurationTotal * totalTiles) % totalTiles
            + object.ID % 3;

        group.group.thursterSprite = thrusterSprite;

        var sx = this.thrusterSpriteSize * (spriteIndex % totalTiles);
        var sy = 0;
        var sw = this.thrusterSpriteSize;
        var sh = this.thrusterSpriteSize;

        var dx = distance;
        var dy = -32 * group.group.spriteScale;
        var dw = this.thrusterSpriteSize * group.group.spriteScale;
        var dh = this.thrusterSpriteSize * group.group.spriteScale;

        this.context.translate(dx, dy);
        this.context.rotate(Math.PI / 2);

        this.context.drawImage(thrusterSprite.image, sx, sy, sw, sh, 0, 0, dw, dh);
        //console.log({sx, sy, sw, sh, dx, dy, dw, dh});
    }
}