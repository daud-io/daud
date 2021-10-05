import { VirtualJoystick } from "./virtualjoystick";
import { ServerWorld } from "./registry";
import { Cookies } from "./cookies";
import { Picker } from "emoji-picker-element";
import { GameContainer } from "./gameContainer";
import { Matrix, PointerEventTypes } from "@babylonjs/core";
import * as bus from "./bus";

const secretShips = ["ship_secret", "ship_zed"];

var refreshSelectedStyle: () => void;
var shipSelectorSwitch: HTMLElement;

const isMobile = "ontouchstart" in document.documentElement;
let joystick: VirtualJoystick;
if (isMobile) {
    document.getElementById("nipple-controls")!.style.display = "unset";
    joystick = new VirtualJoystick({
        container: document.getElementById("nipple-zone")!,
    });
}

bus.on("pageReady", function () {

    const autofTgg = document.getElementById("autofireToggle")!;
    const emojiTrigger = document.getElementById("emoji-trigger")!;
    const emojiContainer = document.getElementById("emoji-container")!;

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
        }
        if (key == " ") Controls.shootKeyboard = true;

        if (key.toLowerCase() == "e" && document.body.classList.contains("alive")) {
            // Autofire
            if (!Controls.autofire) {
                Controls.autofire = true;
                autofTgg.innerHTML = "ON";
            } else {
                Controls.autofire = false;
                autofTgg.innerHTML = "OFF";
            }
        }
    });

    window.addEventListener("keyup", ({ key }) => {
        if (key.toLowerCase() == "s") Controls.boostKeyboard = false;
        if (key == " ") Controls.shootKeyboard = false;
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
    emoji: "🥚",
    nick: "unknown",

    boostPointer: false,
    boostKeyboard: false,
    get boost() { return Controls.boostPointer || Controls.boostKeyboard; },

    shootPointer: false,
    shootKeyboard: false,
    get shoot() { return Controls.shootPointer || Controls.shootKeyboard; },

    autofire: false,
    customData: undefined as any,
    mouseX: 0,
    mouseY: 0,
    screenMouseX: 0,
    screenMouseY: 0,
    container: undefined as GameContainer | undefined,
    color: undefined as string | undefined,
    ship: "ship_green",



};

function sendControl()
{
    let spectateControl = "";

    // if (isSpectating) {
    //     if (spectateNextDebounce && !Controls.shoot) {
    //         spectateNextDebounce = false;
    //     }
    //     if (!spectateNextDebounce && Controls.shoot) {
    //         spectateControl = "action:next";
    //         spectateNextDebounce = true;
    //     } else spectateControl = "spectating";
    // }

    if (Controls.container)
        Controls.container.connection.sendControl(Controls.boost, Controls.shoot || Controls.autofire, Controls.mouseX, Controls.mouseY, spectateControl, Controls.customData);
}

export function registerContainer(container: GameContainer): void {
    const getMousePos = (canvas: HTMLCanvasElement, { clientX, clientY }: MouseEvent) => {
        const rect = canvas.getBoundingClientRect();
        return {
            x: clientX - rect.left,
            y: clientY - rect.top,
        };
    };
    if (isMobile) {
        joystick.onMoved(() => {
            const cx = container.canvas.width / 2;
            const cy = container.canvas.height / 2;
            Controls.mouseX = joystick.deltaX() * 10 + cx;
            Controls.mouseY = -1 * joystick.deltaY() * 10 + cy;
            sendControl();
            //Controls.angle = Math.atan2(joystick.deltaY(), joystick.deltaX());
        });
        const shootEl = document.getElementById("shoot")!;
        const boostEl = document.getElementById("boost")!;
        shootEl.addEventListener("touchstart", () => {
            Controls.shootPointer = true;
            sendControl();
        });
        shootEl.addEventListener("touchend", () => {
            Controls.shootPointer = false;
            sendControl();
        });
        boostEl.addEventListener("touchstart", () => {
            Controls.boostPointer = true;
            sendControl();
        });
        boostEl.addEventListener("touchend", () => {
            Controls.boostPointer = false;
            sendControl();
        });
    } else {
        container.scene.onPointerObservable.add((pointerInfo) => {
            let dirty = false;

            if (pointerInfo.event.buttons != undefined) {
                Controls.boostPointer = (pointerInfo.event.buttons & 2) == 2;
                Controls.shootPointer = (pointerInfo.event.buttons & 1) == 1;
            }

            switch (pointerInfo.type) {
                case PointerEventTypes.POINTERDOWN:
                    pointerInfo.event as MouseEvent;

                    if (pointerInfo.event.buttons == undefined) {
                        if (pointerInfo.event.button == 2)
                            Controls.boostPointer = true;

                        if (pointerInfo.event.button == 1)
                            Controls.shootPointer = true;
                    }
                    dirty = true;
                    break;

                case PointerEventTypes.POINTERUP:
                    if (pointerInfo.event.buttons == undefined) {
                        if (pointerInfo.event.button == 2)
                            Controls.boostPointer = false;

                        if (pointerInfo.event.button == 1)
                            Controls.shootPointer = false;
                    }
                    dirty = true;
                    break;

                case PointerEventTypes.POINTERMOVE:
                    var mousePos = getMousePos(container.canvas, pointerInfo.event);

                    Controls.screenMouseX = mousePos.x;
                    Controls.screenMouseY = mousePos.y;
                    const ray = container.scene.createPickingRay(Controls.screenMouseX, Controls.screenMouseY, Matrix.Identity(), container.camera);
                    const pos = ray.intersectsAxis("y", 100);
                    if (pos) {
                        Controls.mouseX = pos.x - container.cameraPosition.x;
                        Controls.mouseY = pos.z - container.cameraPosition.y;
                        dirty = true;
                    }
                    break;

                case PointerEventTypes.POINTERWHEEL:
                    //console.log("POINTER WHEEL");
                    break;
                case PointerEventTypes.POINTERPICK:
                    //console.log("POINTER PICK");
                    break;
                case PointerEventTypes.POINTERTAP:
                    //console.log("POINTER TAP");
                    break;
                case PointerEventTypes.POINTERDOUBLETAP:
                    //console.log("POINTER DOUBLE-TAP");
                    break;
            }

            if (dirty)
                sendControl();
        });

        container.canvas.addEventListener("contextmenu", (e) => {
            e.preventDefault();
            return false;
        });
    }

    Controls.container = container;
}

export function updateControlAim() {
}

let currentWorld: ServerWorld;
export function setCurrentWorld(world?: ServerWorld): ServerWorld {
    return (currentWorld = world || currentWorld);
}

export function initializeWorld(world: ServerWorld = currentWorld): void {
    const colors = world.hook.allowedColors;
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

