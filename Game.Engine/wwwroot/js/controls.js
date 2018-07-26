// <-- configure Game controls:

Game.controls = {
    left: false,
    up: false,
    right: false,
    down: false,
};

window.addEventListener("keydown", function (e) {
    switch (e.keyCode) {
        case 37: // left arrow
            Game.controls.left = true;
            break;
        case 38: // up arrow
            Game.controls.up = true;
            break;
        case 39: // right arrow
            Game.controls.right = true;
            break;
        case 40: // down arrow
            Game.controls.down = true;
            break;
    }
}, false);

window.addEventListener("keyup", function (e) {
    switch (e.keyCode) {
        case 37: // left arrow
            Game.controls.left = false;
            break;
        case 38: // up arrow
            Game.controls.up = false;
            break;
        case 39: // right arrow
            Game.controls.right = false;
            break;
        case 40: // down arrow
            Game.controls.down = false;
            break;
    }
}, false);
