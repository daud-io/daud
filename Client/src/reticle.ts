import { Settings } from "./settings";
import { GameContainer } from "./gameContainer";
import { RenderedObject } from "./renderedObject";
import { ClientBody, ClientGroup } from "./cache";
import { Vector2 } from "@babylonjs/core";
import * as bus from "./bus";
import { Controls } from "./controls";

export class Reticle {
    container: GameContainer;
    reticle?: RenderedObject;
    reticleBody!: ClientBody;

    constructor(container: GameContainer) {
        this.container = container;

        bus.on('prerender', (time) => this.prerender(time));
    }

    prerender(time: number) {
        if (this.container.pointerLocked && !this.reticle) {
            this.reticleBody = this.setupReticleBody();
            this.reticle = new RenderedObject(this.container, this.reticleBody);
        }

        if (!this.container.pointerLocked && this.reticle) {
            this.reticle.destroy();
            this.reticle = undefined;
        }

        if (this.container.pointerLocked && this.reticle) {
            this.reticle.body.DefinitionTime = time;
            this.reticle.body.OriginalPosition.x = Controls.mouseX + this.container.cameraPosition.x;
            this.reticle.body.OriginalPosition.y = Controls.mouseY + this.container.cameraPosition.y;
            this.reticle.update();
            this.reticle.tick(time);
        }
    }

    setupReticleBody() {
        return {
            ID: 0,
            DefinitionTime: 0,
            Size: 100,
            Sprite: 'reticle',
            Mode: 0,
            Color: 'green',
            Group: 0,
            OriginalAngle: 0,
            AngularVelocity: 0,
            Velocity: Vector2.Zero(),
            OriginalPosition: Vector2.Zero(),
            Position: Vector2.Zero(),
            Angle: 0,
            zIndex: 300
        };
    }
}

