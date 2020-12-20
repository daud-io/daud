import { getDefinition } from "./loader";
import * as PIXI from "pixi.js";
import { Vector2 } from "./Vector2";
import bus from "./bus";

let container: PIXI.Container | undefined;
let focus: Vector2 = new Vector2(0, 0);
let speeds: number[] = [];
let backgroundSprites: PIXI.TilingSprite[];

export function setContainer(newContainer: PIXI.Container): void {
    container = newContainer;
}
export function draw(): void {
    if (backgroundSprites) {
        for (let i = 0; i < backgroundSprites.length; i++) {
            const backgroundSprite = backgroundSprites[i];
            backgroundSprite.position.x = focus.x * (1 - speeds[i]) - 100000;
            backgroundSprite.position.y = focus.y * (1 - speeds[i]) - 100000;
        }
    }
}

export function updateFocus(newFocus: Vector2): void {
    focus = newFocus;
}

bus.on("loaded", () => {
    const spriteDefinition = (getDefinition("bg") as unknown) as {
        layerSpeeds: number[];
        layerTextures: string[];
        scale: number[];
    };
    speeds = spriteDefinition.layerSpeeds;
    const allLayersTextures = spriteDefinition.layerTextures.map((x) => getDefinition(x));
    if (!backgroundSprites) {
        backgroundSprites = [];
    }
    for (let i = 0; i < allLayersTextures.length; i++) {
        const textures = allLayersTextures[i].textures!;
        if (textures.length > 0) {
            let backgroundSprite = backgroundSprites[i];
            if (!backgroundSprite) {
                backgroundSprite = new PIXI.TilingSprite(textures[0], 200000, 200000);
                if (container) container.addChild(backgroundSprite);
                backgroundSprite.tileScale.set(spriteDefinition.scale[i], spriteDefinition.scale[i]);
                backgroundSprite.rotation = Math.random() - 0.5;

                backgroundSprites[i] = backgroundSprite;
            } else backgroundSprite.texture = textures[0];
        }
    }
    draw();
});

export function destroy(): void {
    if (backgroundSprites) {
        for (let i = 0; i < backgroundSprites.length; i++) {
            const backgroundSprite = backgroundSprites[i];
            if (backgroundSprite && container) container.removeChild(backgroundSprite);
        }
    }

    backgroundSprites = [];
}
