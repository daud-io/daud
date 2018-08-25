(function () {

    var selector = document.getElementById('shipSelector');
    selector.addEventListener("change", function (e) {
        Game.Controls.ship = selector.value;

        save();
    });

    var nick = document.getElementById('nick');
    nick.addEventListener("change", function (e) {
        Game.Controls.nick = nick.value;
        Game.Controls.canvas.focus();
        save();
    });


    Game.Controls = {
        left: false,
        up: false,
        right: false,
        down: false,
        boost: false,
        shoot: false,
        registerCanvas: function (canvas) {
            canvas.addEventListener("mousemove", function (e) {
                Game.Controls.mouseX = e.clientX;
                Game.Controls.mouseY = e.clientY;
            });

            canvas.addEventListener("mousedown", function (e) {
                Game.Controls.shoot = true;
                selector.focus();
                e.preventDefault();
                return false;
            });
            canvas.addEventListener("mouseup", function (e) {
                Game.Controls.shoot = false;
            });

            Game.Controls.canvas = canvas;
        },
        ship: "ship_gray"
    };

    window.addEventListener("keydown", function (e) {
        switch (e.keyCode) {
            case 37: // left arrow
                Game.Controls.left = true;
                break;
            case 38: // up arrow
                Game.Controls.up = true;
                break;
            case 39: // right arrow
                Game.Controls.right = true;
                break;
            case 40: // down arrow
                Game.Controls.down = true;
                break;
            case 83: // s
                Game.Controls.boost = true;
                break;
            case 32: // space
                Game.Controls.shoot = true;
                break;
        }
    }, false);

    window.addEventListener("keyup", function (e) {
        switch (e.keyCode) {
            case 37: // left arrow
                Game.Controls.left = false;
                break;
            case 38: // up arrow
                Game.Controls.up = false;
                break;
            case 39: // right arrow
                Game.Controls.right = false;
                break;
            case 40: // down arrow
                Game.Controls.down = false;
                break;
            case 83: // s
                Game.Controls.boost = false;
                break;
            case 32: // space
                Game.Controls.shoot = false;
                break;
        }
    }, false);

    function setCookie(cname, cvalue, exdays) {
        var d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        var expires = "expires=" + d.toUTCString();
        document.cookie = cname + "=" + encodeURIComponent(cvalue) + ";" + expires + ";path=/";
    }

    function getCookie(cname) {
        var name = cname + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) === 0) {
                return decodeURIComponent(c.substring(name.length, c.length));
            }
        }
        return false;
    }

    function save() {
        setCookie("nick", Game.Controls.nick);
        setCookie("ship", Game.Controls.ship);
    }

    var savedNick = getCookie("nick");
    var savedShip = getCookie("ship");

    if (savedNick !== false)
        Game.Controls.nick = nick.value = savedNick;

    if (savedShip !== false)
        Game.Controls.ship = selector.value = savedShip;



}).call(this);
