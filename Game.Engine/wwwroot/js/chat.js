export var message = {
    txt: ""
};
var chat = document.getElementById("chat");
var messages = ["👋", "✅", "❌", "⁉️"];
window.addEventListener("keydown", e => {
    if (e.keyCode == 84) {
        chat.classList.add("open");
    }
    if (e.keyCode < 58 && e.keyCode > 47 && chat.classList.contains("open")) {
        message.txt = messages[e.keyCode - 48];
        chat.classList.remove("open");
    }
});
