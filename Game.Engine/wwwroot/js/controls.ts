import Cookies from "js-cookie";
import * as nipplejs from "nipplejs";
import { Settings } from "./settings";
import { Ship } from "./models/ship";
import "emoji-mart/css/emoji-mart.css";
import { Picker } from "emoji-mart";
import React from "react";
import ReactDOM from "react-dom";

ReactDOM.render(
    React.createElement(
        Picker,
        {
            native: true,
            onClick: e => {
                console.log(e);
                Cookies.set("emoji", e.native);
                var x = e.native;
                emojiTrigger.innerText = e.native;

                Controls.emoji = x;
                console.log(Controls.emoji);
                document.getElementById("emoji-container").classList.remove("open");
            }
        },
        null
    ),
    document.getElementById("emoji-container")
);

const autofCon = document.getElementById("autofireContainer");
const autofTgg = document.getElementById("autofireToggle");
const emojiTrigger = document.getElementById("emoji-trigger");

emojiTrigger.addEventListener("click", () => {
    document.getElementById("emoji-container").classList.toggle("open");
});

export const nipple = nipplejs.create({
    zone: document.getElementById("nipple-zone"),
    restJoystick: false
});
const isMobile = "ontouchstart" in document.documentElement;
if (!isMobile) {
    nipple.destroy();
    document.getElementById("nipple-controls").style.display = "none";
}

const shipSelectorSwitch = document.getElementById("shipSelectorSwitch");

const refreshSelectedStyle = function() {
    const options = Array.from(document.getElementById("shipSelectorSwitch").children);

    for (const option of options) {
        if (option.getAttribute("data-color") == Controls.ship) option.classList.add("selected");
        else option.classList.remove("selected");
    }
};

shipSelectorSwitch.addEventListener("click", function(e) {
    Controls.ship = e.srcElement.getAttribute("data-color");
    save();
    refreshSelectedStyle();
});

const nick: HTMLInputElement = document.querySelector("#nick");
nick.addEventListener("change", e => {
    Controls.nick = nick.value;
    if (Controls && Controls.canvas) Controls.canvas.focus();

    save();
});

function unicode(e) {
    return e.split("-").reduce((total, x) => total + getUnicodeCharacter(parseInt(x, 16)), "");
}
function getUnicodeCharacter(cp) {
    if ((cp >= 0 && cp <= 0xd7ff) || (cp >= 0xe000 && cp <= 0xffff)) {
        return String.fromCharCode(cp);
    } else if (cp >= 0x10000 && cp <= 0x10ffff) {
        // we substract 0x10000 from cp to get a 20-bits number
        // in the range 0..0xFFFF
        cp -= 0x10000;

        // we add 0xD800 to the number formed by the first 10 bits
        // to give the first byte
        var first = ((0xffc00 & cp) >> 10) + 0xd800;

        // we add 0xDC00 to the number formed by the low 10 bits
        // to give the second byte
        var second = (0x3ff & cp) + 0xdc00;

        return String.fromCharCode(first) + String.fromCharCode(second);
    }
}
export var Controls = {
    emoji: "👋",
    nick: "unknown",
    left: false,
    up: false,
    right: false,
    down: false,
    boost: false,
    shoot: false,
    autofire: false,
    downSince: null,
    customData: false,
    mouseX: 0,
    mouseY: 0,
    angle: 0,
    canvas: null,
    color: null,
    registerCanvas(canvas) {
        const getMousePos = (canvas, { clientX, clientY }) => {
            const rect = canvas.getBoundingClientRect();
            return {
                x: clientX - rect.left,
                y: clientY - rect.top
            };
        };
        if (isMobile) {
            nipple.on("move", (e, { angle, force }) => {
                Controls.angle = angle.radian;
                const cx = canvas.width / 2;
                const cy = canvas.height / 2;
                Controls.mouseX = Math.cos(angle.radian) * force * window.innerHeight + cx;
                Controls.mouseY = Math.sin(-angle.radian) * force * window.innerHeight + cy;
            });
            document.getElementById("shoot").addEventListener("touchstart", e => {
                Controls.shoot = true;
            });
            document.getElementById("shoot").addEventListener("touchend", e => {
                if (!Controls.autofire) {
                    Controls.shoot = false;
                }
            });
            document.getElementById("boost").addEventListener("touchstart", e => {
                Controls.boost = true;
            });
            document.getElementById("boost").addEventListener("touchend", e => {
                Controls.boost = false;
            });
        } else {
            window.addEventListener("mousemove", e => {
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
                if (button == 2)
                    //right click
                    Controls.boost = true;
                else {
                    if (Settings.mouseOneButton > 0) {
                        Controls.downSince = new Date().getTime();
                    } else {
                        Controls.shoot = true;
                    }
                }
            });

            window.addEventListener("mouseup", ({ button }) => {
                if (button == 2)
                    //right click
                    Controls.boost = false;
                else {
                    if (Settings.mouseOneButton > 0) {
                        const timeDelta = new Date().getTime() - Controls.downSince;
                        Controls.downSince = false;
                        if (timeDelta < Settings.mouseOneButton) {
                            Controls.shoot = true;
                            setTimeout(function() {
                                if (!Controls.autofire) {
                                    Controls.shoot = false;
                                }
                            }, 100);
                        } else {
                            Controls.boost = true;
                            setTimeout(function() {
                                Controls.boost = false;
                            }, 100);
                        }
                    } else if (!Controls.autofire) {
                        Controls.shoot = false;
                    }
                }
            });
            document.getElementById("gameArea").addEventListener("contextmenu", e => {
                e.preventDefault();
                return false;
            });
        }
        Controls.canvas = canvas;
    },
    initializeWorld: function(world) {
        const colors = world.allowedColors;
        const selector = document.getElementById("shipSelectorSwitch");
        while (selector.firstChild) selector.removeChild(selector.firstChild);

        for (let i = 0; i < colors.length; i++) {
            const selectorImage = Ship.getSelectorImage(colors[i]);

            if (selectorImage) {
                selector.appendChild(selectorImage);
                selectorImage.setAttribute("data-color", colors[i]);
                selectorImage.classList.add("circle");
            }
        }

        const shipIndex = Math.floor(Math.random() * colors.length);

        Controls.ship = colors[shipIndex];
        refreshSelectedStyle();
    },
    ship: "ship_green"
};

window.addEventListener(
    "keydown",
    ({ keyCode }) => {
        switch (keyCode) {
            case 37: // left arrow
                Controls.left = true;
                break;
            case 38: // up arrow
                Controls.up = true;
                break;
            case 39: // right arrow
                Controls.right = true;
                break;
            case 40: // down arrow
                Controls.down = true;
                break;
            case 83: // s
                Controls.boost = true;
                break;
            case 32: // space
                Controls.shoot = true;
                break;
            case 69: // e
                // Autofire
                if (!document.body.classList.contains("alive")) {
                    break;
                } else if (!Controls.autofire) {
                    Controls.autofire = true;
                    Controls.shoot = true;
                    autofTgg.innerHTML = "ON";
                    autofCon.style.color = "#fff";
                    console.log("Autofire enabled!");
                } else {
                    Controls.autofire = false;
                    Controls.shoot = false;
                    autofTgg.innerHTML = "OFF";
                    autofCon.style.color = "";
                    console.log("Autofire disabled!");
                }
                break;
        }
    },
    false
);

window.addEventListener(
    "keyup",
    ({ keyCode }) => {
        switch (keyCode) {
            case 37: // left arrow
                Controls.left = false;
                break;
            case 38: // up arrow
                Controls.up = false;
                break;
            case 39: // right arrow
                Controls.right = false;
                break;
            case 40: // down arrow
                Controls.down = false;
                break;
            case 83: // s
                Controls.boost = false;
                break;
            case 32: // space
                if (!Controls.autofire) {
                    Controls.shoot = false;
                }
                break;
        }
    },
    false
);

function save() {
    const cookieOptions = { expires: 300 };

    if (Controls.nick) Cookies.set("nick", Controls.nick, cookieOptions);
    Cookies.set("color", Controls.color, cookieOptions);
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
