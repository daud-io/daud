import bus from "./bus";
import { Settings } from "./settings";

const hintbox = document.getElementById("hintbox")!;

const texts = [
    'Controls: Mouse to aim, click to fire, press "s" to boost!',
    "Tip: Larger fleets move, shoot, and reload slower than small fleets.",
    "Tip: The particles flying around are fish. Shoot them to grow bigger.",
    "Tip: Chat with other players, arrange duels, and more using the Chat link.",
    "Tip: Trouble focusing your fire? Try adjusting mouse-sensitivity option in settings.",
    "Tip: Fire before you boost, boost before your shots hit your opponent.",
    "Tip: Arrows on the leaderboard point to the players.",
    "Tip: Daud.io is open source and changing quickly. Join Discord to help.",
];

let index = 0;
window.setInterval(() => {
    hintbox.innerText = texts[index % texts.length];
    index++;
}, 6000);

function setVisible()
{
    if (Settings.showHints)
        hintbox.style.display = "";
    else
        hintbox.style.display = "none";
}

bus.on("settings", () => setVisible());
setVisible();

