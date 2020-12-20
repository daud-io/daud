export const message = {
    txt: "",
    time: Date.now(),
};
const chat = document.getElementById("chat")!;
const messages = ["YES", "NO", "OOPS", "HI", "GO", "LAG", "HMM?", "STOP RUNNING", "GG", "LOL"];
for (let i = 0; i < messages.length; i++) {
    chat.innerHTML += `<tr><td>${(i + 1) % 10}</td><td>${messages[i]}</td></tr>`;
}
window.addEventListener("keydown", (e) => {
    if (e.key == "t" && document.body.classList.contains("alive")) {
        chat.classList.toggle("open");
    }
    if ((e.code.startsWith("Digit") || e.code.startsWith("Numpad")) && document.body.classList.contains("alive")) {
        message.txt = messages[(Number(e.key) + 9) % 10] || "";
        message.time = Date.now();
        chat.classList.remove("open");
    }
});
