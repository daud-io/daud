import images from "../../img/*.png";
import { textureCache } from "./textureCache";
import { textureMap } from "./textureMap";
import { spriteModeMap } from "./spriteModeMap";

export class RenderedObject {
    constructor(container) {
        this.container = container;
        this.currentSpriteName = false;
        this.currentMode = 0;
    }

    getMode(mode) {
        return "default";
    }

    buildSprite(spriteName) {
        var textureDefinition = textureMap[spriteName];
        var textures = textureCache[spriteName];

        if (!textures) {
            textures = [];

            var img = new Image();
            img.src = images[textureDefinition.file];

            var baseTexture = new PIXI.Texture.fromLoader(img);

            if (textureDefinition.animated) {

                var tileSize = img.height;
                var totalTiles = (img.width / tileSize);

                for (var tileIndex = 0; tileIndex < totalTiles; tileIndex++) {

                    var sx = tileSize * (tileIndex % totalTiles);
                    var sy = 0;
                    var sw = tileSize;
                    var sh = tileSize;

                    textures.push(
                        new PIXI.Texture(
                            baseTexture, 
                            new PIXI.Rectangle(sx, sy, sw, sh), 
                            false,
                            false, 
                            textureDefinition.rotate || 0
                        )
                    );
                }
            }
            else
                textures.push(baseTexture);

            textureCache[spriteName] = textures;
        }

        var pixiSprite = false;

        if (textureDefinition.animated) {
            pixiSprite = new PIXI.extras.AnimatedSprite(textures);
            pixiSprite.loop = textureDefinition.loop;
            pixiSprite.animationSpeed = textureDefinition.animationSpeed;
        } else {
            pixiSprite = new PIXI.Sprite(textures[0]);
        }

        pixiSprite.pivot.x = pixiSprite.width / 2;
        pixiSprite.pivot.y = pixiSprite.height / 2;
        pixiSprite.position.x = 0;
        pixiSprite.position.y = 0
        pixiSprite.baseScale = textureDefinition.scale;
        pixiSprite.baseOffset = textureDefinition.offset || {x:0,y:0};


        if (textureDefinition.animated)
            pixiSprite.play();

        return pixiSprite;
    }

    getModeMap(spriteModeMap, spriteName, mode) {
        var layers = false;
        var modeName = this.getMode(mode);

        if (spriteModeMap[spriteName])
            layers = spriteModeMap[spriteName].modes[modeName];

        if (!layers && spriteModeMap["default"])
            layers = spriteModeMap[spriteName].modes["default"];

        return layers;
    }

    buildSpriteLayers(spriteModeMap, spriteName, mode) {
        var layers = this.getModeMap(spriteModeMap, spriteName, mode);

        if (layers) {
            var spriteLayers = [];
            for (var i = 0; i < layers.length; i++) {
                var spriteLayer = this.buildSprite(layers[i]);
                spriteLayers.push(spriteLayer);
            }

            return spriteLayers;
        }
        else
            return false;
    }

    destroy() {
        this.destroySprites();
    }

    destroySprites() {
        if (this.spriteLayers) {
            for (var i = 0; i < this.spriteLayers.length; i++) {
                var layer = this.spriteLayers[i];
                this.container.removeChild(layer);
            }

            this.spriteLayers = false;
        }
    }

    setSprite(spriteName, mode, reload) {
        // check that we really need to change anything
        if (reload || spriteName != this.currentSpriteName || mode != this.currentMode) {
            this.currentSpriteName = spriteName;
            this.currentMode = mode;

            //console.log(mode);

            // if we have any existing sprites, destroy them
            this.destroySprites();
            this.spriteLayers = this.buildSpriteLayers(spriteModeMap, spriteName, mode);

            this.foreachLayer(function(layer, index) {
                this.container.addChildAt(layer, 2);
            });
        }
    }

    preRender(time, interpolator)
    {
        if (this.body)
        {
            var newPosition = interpolator.projectObject(this.body, time);
            this.moveSprites(newPosition, this.body.Size);
        }
    }

    moveSprites(interpolatedPosition, size)
    {
        var angle = interpolatedPosition.Angle;

        this.foreachLayer(function(layer, index) {

            layer.position.x = interpolatedPosition.X
                + (layer.baseOffset.x * Math.cos(angle) - layer.baseOffset.y * Math.sin(angle));

            layer.position.y = interpolatedPosition.Y
                + (layer.baseOffset.y * Math.cos(angle) + layer.baseOffset.x * Math.sin(angle));

            layer.rotation = angle;
            layer.scale.set(size * layer.baseScale, size * layer.baseScale);
        });
    }

    update(updateData) {
        this.body = updateData;
        this.setSprite(updateData.Sprite, updateData.Mode);
        //console.log(updateData.Mode);
    }

    foreachLayer(action) {
        if (this.spriteLayers && this.spriteLayers.length)
        for (var i = 0; i < this.spriteLayers.length; i++) {
            var layer = this.spriteLayers[i];
            action.apply(this, [layer, i]);
        }
    }
}
