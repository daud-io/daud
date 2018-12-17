import { Settings } from "../settings";
import { sprites } from "../renderer";

export class FleetRenderer {
    constructor(context, settings = {}) {
        this.context = context;
        this.thrusterSpriteSize = 64;
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

            ctx.drawImage(sprite.image, -spriteWidth / 2, -spriteHeight / 2, spriteWidth, spriteHeight);
            if (object.Mode == 1)
                this.drawBooster(object, group, sprite);
            else
                group.group.index = 0;

            ctx.restore();
        }
    }

    drawBooster(object, group, baseSprite)
    {
        var index = group.group.index || 0;
        group.group.index = index + 1;


        var spriteIndex = Math.floor(index/2);
        //console.log(index, spriteIndex, group);

        var distance = -20;
        
        var thrusterSprite = group.group.thursterSprite;
        if (Settings.theme == '')
        {
            if (!thrusterSprite)
            {
                switch (baseSprite.name)
                {
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
            group.group.thursterSprite = thrusterSprite;
            distance = -40;
        }

        if (Settings.theme == '3ds2agh4z76feci')
        {
            if (!thrusterSprite)
            {
                switch (baseSprite.name)
                {
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
            group.group.thursterSprite = thrusterSprite;
            distance = -20;
        }

        var totalTiles = (thrusterSprite.image.width / this.thrusterSpriteSize);
        var sx = this.thrusterSpriteSize * (spriteIndex % totalTiles);
        var sy = 0;
        var sw = this.thrusterSpriteSize;
        var sh = this.thrusterSpriteSize;

        var dx = distance;
        var dy = -32*1.5;
        var dw = this.thrusterSpriteSize*1.5;
        var dh = this.thrusterSpriteSize*1.5;

        this.context.translate(dx, dy);
        this.context.rotate(Math.PI / 2);

        this.context.drawImage(thrusterSprite.image, sx, sy, sw, sh, 0, 0, dw, dh);
        //console.log({sx, sy, sw, sh, dx, dy, dw, dh});
    }
}
