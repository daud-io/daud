import { VirtualJoystick } from "./virtualjoystick";
import { ServerWorld } from "./lobby";
import Cookies from "js-cookie";
import { Picker } from "emoji-picker-element";
import { getDefinition } from "./loader";

const emojiContainer = document.getElementById("emoji-container")!;
const picker = new Picker();
emojiContainer.appendChild(picker as HTMLElement);
picker.addEventListener("emoji-click", (e) => {
    Cookies.set("emoji", (Controls.emoji = emojiTrigger.innerText = e.detail.unicode || ""));
    emojiContainer.classList.remove("open");
});

const autofTgg = document.getElementById("autofireToggle")!;
const emojiTrigger = document.getElementById("emoji-trigger")!;
const secretShips = ["ship_secret", "ship_zed"];

document.addEventListener("click", (event) => {
    if (event.target == emojiTrigger) emojiContainer.classList.toggle("open");
    else if (!emojiContainer.contains(event.target as Node)) {
        emojiContainer.classList.remove("open");
    }
});

const isMobile = "ontouchstart" in document.documentElement;
let joystick: VirtualJoystick;

if (isMobile) {
    document.getElementById("nipple-controls")!.style.display = "unset";
    joystick = new VirtualJoystick({
        container: document.getElementById("nipple-zone")!,
    });
}

const shipSelectorSwitch = document.getElementById("shipSelectorSwitch")!;

const refreshSelectedStyle = () => {
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
    if (Controls && Controls.canvas) Controls.canvas.focus();

    save();
});

export const Controls = {
    emoji: "👋",
    nick: "unknown",
    boost: false,
    shoot: false,
    autofire: false,
    downSince: undefined,
    customData: undefined as any,
    mouseX: 0,
    mouseY: 0,
    angle: 0,
    canvas: undefined as HTMLCanvasElement | undefined,
    color: undefined as string | undefined,
    ship: "ship_green",
};

export function registerCanvas(canvas: HTMLCanvasElement): void {
    const getMousePos = (canvas: HTMLCanvasElement, { clientX, clientY }: MouseEvent) => {
        const rect = canvas.getBoundingClientRect();
        return {
            x: clientX - rect.left,
            y: clientY - rect.top,
        };
    };
    if (isMobile) {
        joystick.onMoved(() => {
            const cx = canvas.width / 2;
            const cy = canvas.height / 2;
            Controls.mouseX = joystick.deltaX() * 100 + cx;
            Controls.mouseY = joystick.deltaY() * 100 + cy;
            Controls.angle = Math.atan2(joystick.deltaY(), joystick.deltaX());
        });
        const shootEl = document.getElementById("shoot")!;
        const boostEl = document.getElementById("boost")!;
        shootEl.addEventListener("touchstart", () => {
            Controls.shoot = true;
        });
        shootEl.addEventListener("touchend", () => {
            Controls.shoot = false;
        });
        boostEl.addEventListener("touchstart", () => {
            Controls.boost = true;
        });
        boostEl.addEventListener("touchend", () => {
            Controls.boost = false;
        });
    } else {
        window.addEventListener("mousemove", (e) => {
            const pos = getMousePos(canvas, e);
            Controls.mouseX = pos.x;
            Controls.mouseY = pos.y;
            const cx = canvas.width / 2;
            const cy = canvas.height / 2;
            const dy = pos.y - cy;
            const dx = pos.x - cx;

            Controls.angle = Math.atan2(dy, dx);
        });
        window.addEventListener("mousedown", ({ button }) => {
            //right click
            if (button == 2) Controls.boost = true;
            else Controls.shoot = true;
        });

        window.addEventListener("mouseup", ({ button }) => {
            //right click
            if (button == 2) Controls.boost = false;
            else Controls.shoot = false;
        });
        document.body.addEventListener("contextmenu", (e) => {
            if (document.body.classList.contains("alive")) {
                e.preventDefault();
                return false;
            }
        });
    }
    Controls.canvas = canvas;
}

let currentWorld: ServerWorld;
export function setCurrentWorld(world?: ServerWorld): ServerWorld {
    return (currentWorld = world || currentWorld);
}

export function initializeWorld(world: ServerWorld = currentWorld): void {
    const colors = world.allowedColors;
    const selector = document.getElementById("shipSelectorSwitch")!;
    while (selector.firstChild) selector.removeChild(selector.firstChild);

    for (let i = 0; i < colors.length; i++) {
        const selectorImage = new Image();
        selectorImage.src = getDefinition(colors[i]).url;

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
window.addEventListener("keydown", ({ key }) => {
    if (key.toLowerCase() == "s") Controls.boost = true;
    if (key == " ") Controls.shoot = true;
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
    if (key.toLowerCase() == "s") Controls.boost = false;
    if (key == " ") Controls.shoot = false;
});

function save() {
    const cookieOptions = { expires: 300 };

    if (Controls.nick) Cookies.set("nick", Controls.nick, cookieOptions);
    Cookies.set("color", Controls.color!, cookieOptions);
}

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
