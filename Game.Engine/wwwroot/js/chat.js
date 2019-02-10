export var message = {
    txt: "",
    time: Date.now()
};
var chat = document.getElementById("chat");
var messages = ["✅", "❌", "⁉️", "👋", "☠️", "👑", "👈", "👉", "👆", "👇"];
for (var i in messages) {
    chat.innerHTML += `<tr><td>${i < 9 ? 1 + ~~i : 0}</td><td>${messages[i]}</td></tr>`;
}
window.addEventListener("keydown", e => {
    if (e.keyCode == 84) {
        chat.classList.add("open");
    }
    if (e.keyCode < 58 && e.keyCode > 47 && chat.classList.contains("open")) {
        message.txt = messages[e.keyCode - 49] || messages[messages.length - 1];
        message.time = Date.now();
        chat.classList.remove("open");
    }
});
