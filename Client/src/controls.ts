import { VirtualJoystick } from "./virtualjoystick";
import { Cookies } from "./cookies";
import { Picker } from "emoji-picker-element";
import { GameContainer } from "./gameContainer";
import * as bus from "./bus";
import { Matrix } from "@babylonjs/core";

const secretShips = ["ship_secret", "ship_zed"];

var refreshSelectedStyle: () => void;
var shipSelectorSwitch: HTMLElement;

const isMobile = "ontouchstart" in document.documentElement;
let joystick: VirtualJoystick;

bus.on("pageReady", function () {
    const emojiTrigger = document.getElementById("emoji-trigger")!;
    const emojiContainer = document.getElementById("emoji-container")!;

    document.getElementById("autofireContainer")!.addEventListener('touchstart', () => Controls.toggleAutofire());

    // old ie browser, avoid the problem with no emoji picker
    if ((<any>window).crypto) {
        const picker = new Picker();
        emojiContainer.appendChild(picker as HTMLElement);
        picker.addEventListener("emoji-click", (e) => {
            Cookies.set("emoji", (Controls.emoji = emojiTrigger.innerText = e.detail.unicode || ""));
            emojiContainer.classList.remove("open");
        });

        document.addEventListener("click", (event) => {
            if (event.target == emojiTrigger) emojiContainer.classList.toggle("open");
            else if (!emojiContainer.contains(event.target as Node)) {
                emojiContainer.classList.remove("open");
            }
        });
    }
    shipSelectorSwitch = document.getElementById("shipSelectorSwitch")!;

    refreshSelectedStyle = () => {
        const options = Array.from(shipSelectorSwitch.children);

        for (const option of options) {
            if (option.getAttribute("data-color") == Controls.ship) option.classList.add("selected");
            else option.classList.remove("selected");
        }
    };

    shipSelectorSwitch.addEventListener("click", (e) => {
        Controls.ship = (e.target as HTMLElement).getAttribute("data-color")!;
        save();
        refreshSelectedStyle();
    });

    const nick = document.querySelector("#nick") as HTMLInputElement;
    nick.addEventListener("input", () => {
        Controls.nick = nick.value;
        save();
    });

    window.addEventListener("keydown", ({ key }) => {
        if (key.toLowerCase() == "s") {
            Controls.boostKeyboard = true;
            sendControlPacket();
        }
        if (key == " ") {
            Controls.shootKeyboard = true;
            sendControlPacket();
        }

        if (key.toLowerCase() == "m") {
            bus.emit("mapShow");
        }


        if (key.toLowerCase() == "e" && document.body.classList.contains("alive")) {
            // Autofire
            Controls.toggleAutofire();
        }
    });

    window.addEventListener("keyup", ({ key }) => {
        if (key.toLowerCase() == "s") {
            Controls.boostKeyboard = false;
            sendControlPacket();
        }
        if (key == " ") {
            Controls.shootKeyboard = false;
            sendControlPacket();
        }

        if (key.toLowerCase() == "m") {
            bus.emit("mapHide");
        }

    });

    document.body.addEventListener("contextmenu", (e) => {
        if (document.body.classList.contains("alive")) {
            e.preventDefault();
            return false;
        }
    });

    const savedNick = Cookies.get("nick");
    const savedColor = Cookies.get("color");
    const savedEmoji = Cookies.get("emoji");

    if (savedNick != undefined) {
        Controls.nick = savedNick;
        nick.value = savedNick;
    }

    if (savedColor != undefined) {
        Controls.color = savedColor;
        refreshSelectedStyle();
    }

    if (savedEmoji != undefined) {
        Controls.emoji = savedEmoji;
        emojiTrigger.innerText = savedEmoji;
    }
});


export const Controls = {
    container: undefined as GameContainer | undefined,

    emoji: "🥚",
    nick: "unknown",

    boostPointer: false,
    boostKeyboard: false,
    get boost() { return Controls.boostPointer || Controls.boostKeyboard; },

    shootPointer: false,
    shootKeyboard: false,
    get shoot() { return Controls.shootPointer || Controls.shootKeyboard; },

    autofire: false,

    previousMouseX: 0,
    previousMouseY: 0,

    mouseX: 0,
    mouseY: 0,
    screenMouseX: 0,
    screenMouseY: 0,
    pointerSpeed: 0,

    color: undefined as string | undefined,
    ship: "ship_green",

    spectateControl: "" as string | undefined,
    spectateDebounce: false,

    requestPointerLock() {
        console.log('requesting lock');
        if (Controls.container) {
            const canvas = Controls.container.canvas;
            // Ask the browser to lock the pointer)
            const promise = (<any>canvas).requestPointerLock({
                unadjustedMovement: true,
            });

            if (!promise) {
                console.log("disabling mouse acceleration is not supported");
                return;
            }

            return promise
                .then(() => console.log("pointer is locked"))
                .catch((error) => {
                    if (error.name === "NotSupportedError") {
                        // Some platforms may not support unadjusted movement.
                        // You can request again a regular pointer lock.
                        return canvas.requestPointerLock();
                    }
                });
        }
    },

    toggleAutofire() {
        const autofTgg = document.getElementById("autofireToggle")!;

        if (!Controls.autofire) {
            Controls.autofire = true;
            autofTgg.innerHTML = "ON";
        } else {
            Controls.autofire = false;
            autofTgg.innerHTML = "OFF";
        }
        sendControlPacket();
    }
};


bus.on('postrender', (gametime) => {
    Controls.pointerSpeed += Math.abs(Controls.mouseX - Controls.previousMouseX) + Math.abs(Controls.mouseY - Controls.previousMouseY);
    Controls.pointerSpeed *= 0.9;

    Controls.previousMouseX = Controls.mouseX;
    Controls.previousMouseY = Controls.mouseY;
});

function sendControlPacket() {
    Controls.container?.connection.sendControl(Controls.boost, Controls.shoot || Controls.autofire, Controls.mouseX, Controls.mouseY, Controls.spectateControl);
}

function mouseUp(this: HTMLCanvasElement, ev: MouseEvent): any {
    
    switch (ev.button) {
        case 2:
            Controls.boostPointer = false;
            break;
        case 0:
            Controls.shootPointer = false;
            break;
    }
    sendControlPacket();
}

function spectateActionNext()
{
    console.log('spectate next');
    Controls.spectateControl = "action:next";
    sendControlPacket();
    Controls.spectateControl = undefined;
}

function mouseDown(this: HTMLCanvasElement, ev: MouseEvent): any {

    if (!Controls.container?.alive)
        spectateActionNext();

    switch (ev.button) {
        case 2:
            Controls.boostPointer = true;
            break;
        case 0:
            Controls.shootPointer = true;
            break;
    }
    sendControlPacket();
}

function mouseMove(this: any, ev: MouseEvent): any {
    const rect: DOMRect = Controls.container!.boundingRect;
    let scale = 1.5;

    if (Controls.container!.pointerLocked) {
    
        Controls.mouseX += ev.movementX * scale;
        Controls.mouseY -= ev.movementY * scale;
        Controls.mouseX = Math.max(Math.min(Controls.mouseX, rect.width * scale), -rect.width * scale);
        Controls.mouseY = Math.max(Math.min(Controls.mouseY, rect.height * scale), -rect.height * scale);
    }
    else
    {
        const container = Controls.container!;
        const ray = container.scene.createPickingRay(ev.clientX, ev.clientY, Matrix.Identity(), container.camera);
        const pos = ray.intersectsAxis("y", 100);
        if (pos) {
            Controls.mouseX = pos.x - container.cameraPosition.x;
            Controls.mouseY = pos.z - container.cameraPosition.y;
        }
    }

    sendControlPacket();
}

export function registerContainer(container: GameContainer): void {
    Controls.container = container;

    if (isMobile) {

        if (isMobile) {
            document.getElementById("nipple-controls")!.style.display = "unset";
            joystick = new VirtualJoystick({
                container: document.getElementById("nipple-zone")!,
            });
        
            document.body.classList.add('touchscreen');
    
            Controls.container!.touchscreen = true;
            Controls.toggleAutofire();
        }
    
        joystick.onExtraTap((x: number, y: number) => {
            const ray = container.scene.createPickingRay(x, y, Matrix.Identity(), container.camera);
            const pos = ray.intersectsAxis("y", 100);
            if (pos) {
                let previousMouseX = Controls.mouseX;
                let previousMouseY = Controls.mouseY;
                let previousShootPointer = Controls.shootPointer;

                Controls.mouseX = pos.x - container.cameraPosition.x;
                Controls.mouseY = pos.z - container.cameraPosition.y;
                Controls.shootPointer = true;
                sendControlPacket();
                Controls.shootPointer = previousShootPointer;
                Controls.mouseX = previousMouseX;
                Controls.mouseY = previousMouseY;
            }
        });
        joystick.onMoved(() => {
            const cx = container.boundingRect.width / 2;
            const cy = container.boundingRect.height / 2;

            Controls.mouseX = joystick.deltaX() * 10;
            Controls.mouseY = -1 * joystick.deltaY() * 10;
            sendControlPacket();
        });
        const shootEl = document.getElementById("shoot")!;
        const boostEl = document.getElementById("boost")!;
        shootEl.addEventListener("touchstart", () => {
            Controls.shootPointer = true;
            sendControlPacket();
        }, { passive: true });
        shootEl.addEventListener("touchend", () => {
            Controls.shootPointer = false;
            sendControlPacket();
        }, { passive: true });
        boostEl.addEventListener("touchstart", () => {
            Controls.boostPointer = true;
            sendControlPacket();
        }, { passive: true });
        boostEl.addEventListener("touchend", () => {
            Controls.boostPointer = false;
            sendControlPacket();
        }, { passive: true });
    } else {

        container.scene.onPrePointerObservable.add((pointerInfo) => {
            pointerInfo.skipOnPointerObservable = true;
        });

        Controls.container?.canvas.addEventListener("mousemove", mouseMove, { passive: true });
        Controls.container?.canvas.addEventListener("mouseup", mouseUp, { passive: true });
        Controls.container?.canvas.addEventListener("mousedown", mouseDown, { passive: true });
        Controls.container?.canvas.addEventListener("contextmenu", (e) => { e.preventDefault(); return false; });
    }

}

bus.on("dead", () => {
    if (Controls.container?.pointerLocked)
        document.exitPointerLock();
});

document.addEventListener('pointerlockchange', (event) => {
    if (Controls.container) {
        if ((<any>document).pointerLockElement !== Controls.container?.canvas &&
            (<any>document).mozPointerLockElement !== Controls.container?.canvas &&
            (<any>document).webkitPointerLockElement !== Controls.container?.canvas) {
            console.log('unlocked');
            Controls.container.pointerLocked = false;
            if (Controls.container.alive)
                Controls.container.connection.sendExit();
        }
        else {
            console.log('locked');
            Controls.container.pointerLocked = true;
            Controls.mouseX = 0;
            Controls.mouseY = 0;
        }
    }
});



function setupShipSelector() {
    const colors = Controls.container?.connection?.hook?.AllowedColors;

    if (!colors)
        return;

    const selector = document.getElementById("shipSelectorSwitch")!;
    while (selector.firstChild) selector.removeChild(selector.firstChild);

    for (let i = 0; i < colors.length; i++) {
        const selectorImage = new Image();
        if (Controls.container) {
            selectorImage.src = Controls.container.loader.getTextureDefinition(colors[i])?.url!!;
        }

        if (selectorImage) {
            selector.appendChild(selectorImage);
            selectorImage.setAttribute("data-color", colors[i]);
            selectorImage.classList.add("circle");
            if (secretShips.includes(colors[i])) {
                selectorImage.style.display = "none";
            }
        }
    }

    if (!colors.includes(Controls.ship)) {
        const shipIndex = Math.floor(Math.random() * colors.length);
        Controls.ship = colors[shipIndex];
    }
    refreshSelectedStyle();
}

bus.on("themechange", () => {
    setupShipSelector();
});

bus.on('hook', (hook) => {
    setupShipSelector();
});

export function addSecretShips(roles: string[]): void {
    if (roles.includes("Player")) {
        const ship = shipSelectorSwitch.querySelector("[data-color=ship_secret]") as HTMLElement;
        if (ship) ship.style.display = "inline-block";
    }
    if (roles.includes("Old Guard")) {
        const ship = shipSelectorSwitch.querySelector("[data-color=ship_zed]") as HTMLElement;
        if (ship) ship.style.display = "inline-block";
    }
}

function save() {
    const cookieOptions = { expires: 300 };

    Cookies.set("nick", Controls.nick, cookieOptions);
    Cookies.set("color", Controls.color!, cookieOptions);
    Cookies.set("emoji", Controls.emoji, cookieOptions);
}

