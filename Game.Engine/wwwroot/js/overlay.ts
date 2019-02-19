import Plotly from "./plotly-subset";

export class Overlay {
    container: any;
    plotly: Plotly;
    canvas: HTMLCanvasElement;
    data: any;
    constructor(container, canvas, plotly) {
        this.container = container;
        this.plotly = plotly;
        this.canvas = canvas;
        this.data = false;
    }

    update(customData) {
        this.data = customData;

        if (this.plotly.used) this.container.plotly.style.visibility = "visible";
        else this.container.plotly.style.visibility = "hidden";
    }

    draw(cache, interpolator, currentTime, fleetID) {}
}
