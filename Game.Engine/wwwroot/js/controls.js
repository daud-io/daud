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
        canvas.addEventListener("mousemove", function(e) {
            var pos = getMousePos(canvas, e);
            Controls.mouseX = pos.x;
            Controls.mouseY = pos.y;
        });

        canvas.addEventListener("mousedown", function(e) {
            Controls.shoot = true;
            selector.focus();
            e.preventDefault();
            return false;
        });
        canvas.addEventListener("mouseup", function(e) {
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

function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + exdays * 24 * 60 * 60 * 1000);
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + encodeURIComponent(cvalue) + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(";");
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === " ") {
            c = c.substring(1);
        }
        if (c.indexOf(name) === 0) {
            return decodeURIComponent(c.substring(name.length, c.length));
        }
    }
    return false;
}

function save() {
    setCookie("nick", Controls.nick);
    setCookie("color", Controls.color);
}

var savedNick = getCookie("nick");
var savedColor = getCookie("color");

if (savedNick !== false) {
    Controls.nick = savedNick;
    nick.value = savedNick;
}

if (savedColor !== false) {
    Controls.color = savedColor;
    selector.value = savedColor;
}

selector.dispatchEvent(new Event("change"));
selector.dispatchEvent(new Event("change"));
