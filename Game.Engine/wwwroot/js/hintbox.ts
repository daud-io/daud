import { Settings } from "./settings";

const hintbox = document.getElementById("instructions-text");

const texts = [
    "MOVE MOUSE to steer fleet",
    "CLICK or SPACE to shoot",
    "'S' to Split & Dash"
];

let index = 0;

const eventStart = new Date("2018-12-13T17:00:00.000Z");
if (new Date().getTime() < eventStart.getTime()) {
    window.setInterval(() => {
        const distance = eventStart.getTime() - new Date().getTime();

        // Time calculations for days, hours, minutes and seconds
        const days = Math.floor(distance / (1000 * 60 * 60 * 24));
        const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);

        const remaining = `${hours.toString().padStart(2, "0")}:${minutes.toString().padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`;
        hintbox.innerText = `Team match begins in ${remaining}`;
    }, 1000);
} else {
    window.setInterval(() => {
        if (Settings.showHints) hintbox.style.visibility = "visible";
        else hintbox.style.visibility = "hidden";

        hintbox.innerText = texts[index % texts.length];
        index++;
    }, 6000);
}
