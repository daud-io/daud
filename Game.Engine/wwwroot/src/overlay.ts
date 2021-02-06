import Plotly from "./plotly-subset";
import { CustomContainer } from "./CustomContainer";

export class Overlay {
    container: CustomContainer;
    plotly: Plotly;
    canvas: HTMLCanvasElement;
    data: any;
    constructor(container: CustomContainer, canvas: HTMLCanvasElement, plotly: Plotly) {
        this.container = container;
        this.plotly = plotly;
        this.canvas = canvas;
        this.data = undefined;
    }

    update(customData: any): void {
        this.data = customData;

        if (this.plotly.used) this.container.plotly.style.visibility = "visible";
        else this.container.plotly.style.visibility = "hidden";
    }
}
