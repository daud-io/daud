import { gui } from "./serverinfo";

var pressable = true;
document.addEventListener("keydown", function(e) {
    if ((e.keyCode == 70 || e.which == 70) && document.body.classList.contains("alive")) {
        gui.closed = !gui.closed;
        pressable = false;
    }
});

document.addEventListener("keyup", function(e) {
    if (e.keyCode == 70 || e.which == 70) pressable = true;
});
