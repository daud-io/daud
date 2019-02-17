export var message = {
    txt: "",
    time: Date.now()
};
var chat = document.getElementById("chat");
var messages = ["hi", "bye", "gg", "lol", "team?", "duel?", "yes!", "no!", "die!", "easy!"];
for (var i in messages) {
    chat.innerHTML += `<tr><td>${i < 9 ? 1 + ~~i : 0}</td><td>${messages[i]}</td></tr>`;
}
window.addEventListener("keydown", e => {
    if (e.keyCode == 84) {
        chat.classList.toggle("open");
    }
    if (e.keyCode < 58 && e.keyCode > 47 && document.body.classList.contains("alive")) {
        message.txt = messages[e.keyCode - 49] || messages[messages.length - 1];
        message.time = Date.now();
        chat.classList.remove("open");
    }
});
