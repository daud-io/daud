// Inspired by https://github.com/jeromeetienne/virtualjoystick.js
export class VirtualJoystick {
    private container: HTMLElement;
    private strokeStyle: string;
    private stickEl: HTMLCanvasElement;
    private baseEl: HTMLCanvasElement;
    private baseX: number;
    private baseY: number;
    private stickX: number;
    private stickY: number;
    private limitStickTravel: boolean;
    private stickRadius: number;
    private pressed: boolean;
    private touchIdx?: number;
    private moved: (() => void)[] = [];
    constructor(
        opts: {
            container?: HTMLElement;
            strokeStyle?: string;
            limitStickTravel?: boolean;
            stickRadius?: number;
        } = {}
    ) {
        this.container = opts.container || document.body;
        this.strokeStyle = opts.strokeStyle || "white";
        this.stickEl = this.buildJoystickStick();
        this.baseEl = this.buildJoystickBase();
        this.baseX = this.stickX = 0;
        this.baseY = this.stickY = 0;
        this.limitStickTravel = opts.limitStickTravel || true;
        this.stickRadius = opts.stickRadius !== undefined ? opts.stickRadius : 100;

        this.container.style.position = "relative";

        this.container.appendChild(this.baseEl);
        this.baseEl.style.position = "absolute";
        this.baseEl.style.display = "none";
        this.container.appendChild(this.stickEl);
        this.stickEl.style.position = "absolute";
        this.stickEl.style.display = "none";

        this.pressed = false;
        this.touchIdx = undefined;

        this.container.addEventListener("touchstart", this.onTouchStart, false);
        this.container.addEventListener("touchend", this.onTouchEnd, false);
        this.container.addEventListener("touchcancel", this.onTouchEnd, false);

        this.container.addEventListener("touchmove", this.onTouchMove, false);
    }

    destroy(): void {
        this.container.removeChild(this.baseEl);
        this.container.removeChild(this.stickEl);

        this.container.removeEventListener("touchstart", this.onTouchStart, false);
        this.container.removeEventListener("touchend", this.onTouchEnd, false);
        this.container.removeEventListener("touchmove", this.onTouchMove, false);
    }

    deltaX(): number {
        return this.stickX - this.baseX;
    }

    deltaY(): number {
        return this.stickY - this.baseY;
    }

    private onUp() {
        this.pressed = false;
        this.stickEl.style.display = "none";

        this.baseEl.style.display = "none";

        this.baseX = this.baseY = 0;
        this.stickX = this.stickY = 0;
    }

    private onDown(x: number, y: number) {
        this.pressed = true;

        this.baseX = x;
        this.baseY = y;
        this.baseEl.style.display = "";
        this.move(this.baseEl.style, this.baseX - this.baseEl.width / 2, this.baseY - this.baseEl.height / 2);

        this.stickX = x;
        this.stickY = y;

        this.stickEl.style.display = "";
        this.move(this.stickEl.style, this.stickX - this.stickEl.width / 2, this.stickY - this.stickEl.height / 2);
        this.dispatchMoved();
    }

    private onMove(x: number, y: number) {
        if (this.pressed === true) {
            this.stickX = x;
            this.stickY = y;

            if (this.limitStickTravel === true) {
                const deltaX = this.deltaX();
                const deltaY = this.deltaY();
                const stickDistance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
                if (stickDistance > this.stickRadius) {
                    const stickNormalizedX = deltaX / stickDistance;
                    const stickNormalizedY = deltaY / stickDistance;

                    this.stickX = stickNormalizedX * this.stickRadius + this.baseX;
                    this.stickY = stickNormalizedY * this.stickRadius + this.baseY;
                }
            }

            this.move(this.stickEl.style, this.stickX - this.stickEl.width / 2, this.stickY - this.stickEl.height / 2);
            this.dispatchMoved();
        }
    }

    private onTouchStart = (event: TouchEvent) => {
        // if there is already a touch inprogress do nothing
        if (this.touchIdx !== undefined) return;

        event.preventDefault();
        // get the first who changed
        const touch = event.changedTouches[0];
        // set the touchIdx of this joystick
        this.touchIdx = touch.identifier;
        return this.onDown(touch.clientX, touch.clientY);
    };

    private onTouchEnd = (event: TouchEvent) => {
        // if there is no touch in progress, do nothing
        if (this.touchIdx === undefined) return;

        // try to find our touch event
        const touchList = event.changedTouches;
        let i: number;
        for (i = 0; i < touchList.length && touchList[i].identifier !== this.touchIdx; i++);
        // if touch event isnt found,
        if (i === touchList.length) return;

        // reset touchIdx - mark it as no-touch-in-progress
        this.touchIdx = undefined;

        event.preventDefault();

        return this.onUp();
    };

    private onTouchMove = (event: TouchEvent) => {
        // if there is no touch in progress, do nothing
        if (this.touchIdx === undefined) return;

        // try to find our touch event
        const touchList = event.changedTouches;

        let i: number;
        for (i = 0; i < touchList.length && touchList[i].identifier !== this.touchIdx; i++);
        // if touch event with the proper identifier isnt found, do nothing
        if (i === touchList.length) return;
        const touch = touchList[i];
        //console.log(touchList, i);
        event.preventDefault();
        return this.onMove(touch.clientX, touch.clientY);
    };

    private buildJoystickBase() {
        const canvas = document.createElement("canvas");
        canvas.width = 126;
        canvas.height = 126;

        const ctx = canvas.getContext("2d")!;
        ctx.beginPath();
        ctx.strokeStyle = this.strokeStyle;
        ctx.lineWidth = 6;
        ctx.arc(canvas.width / 2, canvas.width / 2, 40, 0, Math.PI * 2, true);
        ctx.stroke();

        ctx.beginPath();
        ctx.strokeStyle = this.strokeStyle;
        ctx.lineWidth = 2;
        ctx.arc(canvas.width / 2, canvas.width / 2, 60, 0, Math.PI * 2, true);
        ctx.stroke();

        return canvas;
    }

    private buildJoystickStick() {
        const canvas = document.createElement("canvas");
        canvas.width = 86;
        canvas.height = 86;
        const ctx = canvas.getContext("2d")!;
        ctx.beginPath();
        ctx.strokeStyle = this.strokeStyle;
        ctx.lineWidth = 6;
        ctx.arc(canvas.width / 2, canvas.width / 2, 40, 0, Math.PI * 2, true);
        ctx.stroke();
        return canvas;
    }

    private move(style: CSSStyleDeclaration, x: number, y: number) {
        style.left = `${x}px`;
        style.top = `${y}px`;
    }

    onMoved(fct: () => void): void {
        this.moved.push(fct);
    }
    dispatchMoved(): void {
        for (const fn of this.moved) {
            fn();
        }
    }
}

