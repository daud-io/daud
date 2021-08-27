import * as PIXI from "pixi.js";

export class CustomContainer extends PIXI.Container {
    bodyGroup: PIXI.Container;
    backgroundGroup: PIXI.Container;
    emitterContainer: PIXI.ParticleContainer;
    plotly: any;
    constructor() {
        super();
        this.sortableChildren = true;

        this.backgroundGroup = new PIXI.Container();
        this.bodyGroup = new PIXI.Container();
        this.emitterContainer = new PIXI.ParticleContainer();

        this.bodyGroup.sortableChildren = true;

        this.backgroundGroup.zIndex = 0;
        this.bodyGroup.zIndex = 2;
        this.emitterContainer.zIndex = 10;
        
        this.addChild(this.backgroundGroup);
        this.addChild(this.bodyGroup);
        this.addChild(this.emitterContainer);
    }
}
