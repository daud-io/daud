import images from "../../img/*.png";

var textureCache = {};

export class Ship {
    constructor(container) {
        this.container = container;
        this.currentSpriteName = false;
        this.currentMode = 0;

        this.textureMap = {
            "ship_red": {
                file: "ship_red",
                scale: 0.02
            },
            "obstacle": {
                file: "obstacle",
                scale: 0.004
            },
            "thruster_default_red": {
                file: "thruster_default_red",
                animated: true,
                loop: false,
                animationSpeed: 1.0,
                offset: {x: -145, y: 0 },
                scale: .04,
                rotate: 6
            },
            "circles": {
                file: "circles",
                animated: true,
                loop: true,
                animationSpeed: 0.2,
                scale: 0.05
            }
        }

        this.spriteModeMap = {
            "ship_red": {
                modes: {
                    default: [ "ship_red" ],
                    weaponupgrade: ["ship_red", "circles"],
                    boost: [ "ship_red", "thruster_default_red"],
                    invulnerable: ["ship_red", "obstacle"]
                }
            }
        };
    }

    getMode(mode) {
        switch (mode) {
            case 0: return "default";
            case 1: return "boost";
            case 2: return "weaponupgrade";
            case 3: return "invulnerable";
            default: return "default";
        }
    }

    buildSprite(spriteName) {
        var spriteDefinition = this.textureMap[spriteName];
        var textures = textureCache[spriteName];

        if (!textures) {
            textures = [];

            var img = new Image();
            img.src = images[spriteDefinition.file];

            var baseTexture = new PIXI.Texture.fromLoader(img);

            if (spriteDefinition.animated) {

                var tileSize = img.height;
                var totalTiles = (img.width / tileSize);

                for (var tileIndex = 0; tileIndex < totalTiles; tileIndex++) {

                    var sx = tileSize * (tileIndex % totalTiles);
                    var sy = 0;
                    var sw = tileSize;
                    var sh = tileSize;

                    textures.push(new PIXI.Texture(baseTexture, new PIXI.Rectangle(sx, sy, sw, sh), false, false, spriteDefinition.rotate || 0));
                }
            }
            else
                textures.push(baseTexture);

            textureCache[spriteName] = textures;
        }

        var pixiSprite = false;

        if (spriteDefinition.animated) {
            pixiSprite = new PIXI.extras.AnimatedSprite(textures);
            pixiSprite.loop = spriteDefinition.loop;
            pixiSprite.animationSpeed = spriteDefinition.animationSpeed;
        } else {
            pixiSprite = new PIXI.Sprite(textures[0]);
        }

        pixiSprite.pivot.x = pixiSprite.width / 2;
        pixiSprite.pivot.y = pixiSprite.height / 2;
        pixiSprite.position.x = 0;
        pixiSprite.position.y = 0
        pixiSprite.baseScale = spriteDefinition.scale;
        pixiSprite.baseOffset = spriteDefinition.offset || {x:0,y:0};


        if (spriteDefinition.animated)
            pixiSprite.play();
            
        return pixiSprite;
    }

    getModeMap(spriteModeMap, spriteName, mode) {
        var layers = false;
        var modeName = this.getMode(mode);

        if (spriteModeMap[spriteName])
            layers = spriteModeMap[spriteName].modes[modeName];

        if (!layers && this.spriteModeMap["default"])
            layers = this.spriteModeMap[spriteName].modes["default"];

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

    setSprite(spriteModeMap, spriteName, mode, reload) {
        // check that we really need to change anything
        if (reload || spriteName != this.currentSpriteName || mode != this.currentMode) {
            this.currentSpriteName = spriteName;
            this.currentMode = mode;

            console.log(mode);

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
        this.setSprite(this.spriteModeMap, updateData.Sprite, updateData.Mode);
        console.log(updateData.Mode);
    }

    foreachLayer(action) {
        if (this.spriteLayers && this.spriteLayers.length)
        for (var i = 0; i < this.spriteLayers.length; i++) {
            var layer = this.spriteLayers[i];
            action.apply(this, [layer, i]);
        }
    }
}
