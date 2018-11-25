import Cookies from "js-cookie";
import nipplejs from "nipplejs";

export var nipple = nipplejs.create({
    zone: document.getElementById("nipple-zone"),
    resetJoystick: false
});

if (!("ontouchstart" in document.documentElement)) {
    nipple.destroy();
    document.getElementById("niple-buttons").style.display = "none";
}
var selector = document.querySelector("#shipSelector");
selector.addEventListener("change", function(e) {
    Controls.ship = "ship_" + selector.value || "ship_green";
    Controls.color = selector.value || "green";

    save();
});

var nick = document.querySelector("#nick");
nick.addEventListener("change", function(e) {
    Controls.nick = nick.value;
    if (Controls && Controls.canvas) Controls.canvas.focus();

    save();
});
var domElement = document.querySelector(".noselect");

export var Controls = {
    left: false,
    up: false,
    right: false,
    down: false,
    boost: false,
    shoot: false,
    registerCanvas: function(canvas) {
        var getMousePos = function(canvas, evt) {
            var rect = canvas.getBoundingClientRect();
            return {
                x: evt.clientX - rect.left,
                y: evt.clientY - rect.top
            };
        };
        domElement.addEventListener("mousemove", function(e) {
            var pos = getMousePos(canvas, e);
            Controls.mouseX = pos.x;
            Controls.mouseY = pos.y;
            var cx = canvas.width / 2;
            var cy = canvas.height / 2;
            var dy = pos.y - cy;
            var dx = pos.x - cx;
            Controls.angle = Math.atan2(dy, dx);
        });
        nipple.on("move", function(e, data) {
            Controls.angle = data.angle.radian;
            var cx = canvas.width / 2;
            var cy = canvas.height / 2;
            Controls.mouseX = Math.cos(data.angle.radian) * data.force * window.innerHeight + cx;
            Controls.mouseY = Math.sin(-data.angle.radian) * data.force * window.innerHeight + cy;
        });
        domElement.addEventListener("mousedown", function(e) {
            Controls.shoot = true;
            selector.focus();
            e.preventDefault();
            return false;
        });
        document.getElementById("shoot").addEventListener("touchstart", function(e) {
            Controls.shoot = true;
        });
        document.getElementById("shoot").addEventListener("touchend", function(e) {
            Controls.shoot = false;
        });
        document.getElementById("boost").addEventListener("touchstart", function(e) {
            Controls.boost = true;
        });
        document.getElementById("boost").addEventListener("touchend", function(e) {
            Controls.boost = false;
        });
        domElement.addEventListener("mouseup", function(e) {
            Controls.shoot = false;
        });

        Controls.canvas = canvas;
    },
    ship: "ship_green"
};

window.addEventListener(
    "keydown",
    function(e) {
        switch (e.keyCode) {
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
        }
    },
    false
);

window.addEventListener(
    "keyup",
    function(e) {
        switch (e.keyCode) {
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
                Controls.shoot = false;
                break;
        }
    },
    false
);

function save() {
    if (Controls.nick) Cookies.set("nick", Controls.nick);
    Cookies.set("color", Controls.color);
}

var savedNick = Cookies.get("nick");
var savedColor = Cookies.get("color");

if (savedNick !== undefined) {
    Controls.nick = savedNick;
    nick.value = savedNick;
}

if (savedColor !== undefined) {
    Controls.color = savedColor;
    selector.value = savedColor;
}

var event = document.createEvent("Event");
event.initEvent("change", true, true);
selector.dispatchEvent(event);
