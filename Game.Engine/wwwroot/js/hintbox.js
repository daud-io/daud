var hintbox = document.getElementById("hintbox");

var texts = [
    "Controls: Mouse to aim, click to fire, press \"s\" to boost!",
    "Tip: If you have low frames-per-second in FireFox, try Chrome",
    "Tip: The particles flying around are fish. Shoot them to grow bigger",
    "Tip: Trouble focusing your fire? Try adjusting mouse-sensitivity option in settings.",
    "Tip: Don't be afraid of dying. Soon others will!",
    "Tip: Fire before you boost, boost before your shots hit your opponent."
];

var index = 1;
window.setInterval(function() {
    hintbox.innerText = texts[index % texts.length];
    index++;
}, 4000);
