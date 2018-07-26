(function () {
    Game.Controls = {
        left: false,
        up: false,
        right: false,
        down: false,
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
        }
    }, false);
}).call(this);
