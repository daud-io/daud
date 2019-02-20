import { RenderedObject } from "./renderedObject";
import { CustomContainer } from "../CustomContainer";

export class Bullet extends RenderedObject {
    constructor(container:CustomContainer, cache) {
        super(container);
    }
}
