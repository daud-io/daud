import { Container } from "pixi.js";

export class CustomContainer extends Container {
    bodyGroup: PIXI.Container;
    plotly: any;
    tiles: any;
    backgroundGroup: PIXI.Container;
    emitterContainer: PIXI.ParticleContainer;
}
