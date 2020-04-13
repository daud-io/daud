export const message = {
    txt: "",
    time: Date.now(),
};
const chat = document.getElementById("chat");
const messages = ["✅", "❌", "⁉️", "👋", "☠️", "👑", "👈", "👉", "👆", "👇"];
for (let i = 0; i < messages.length; i++) {
    chat.innerHTML += `<tr><td>${i < 9 ? 1 + ~~i : 0}</td><td>${messages[i]}</td></tr>`;
}
window.addEventListener("keydown", (e) => {
    if (e.keyCode == 84 && document.body.classList.contains("alive")) {
        chat.classList.toggle("open");
    }
    if (e.keyCode < 58 && e.keyCode > 47 && document.body.classList.contains("alive")) {
        message.txt = messages[e.keyCode - 49] || messages[messages.length - 1];
        message.time = Date.now();
        chat.classList.remove("open");
    }
});
