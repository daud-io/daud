const hintbox = document.getElementById("hintbox");

const texts = [
    'Controls: Mouse to aim, click to fire, press "s" to boost!',
    "Tip: If you have low frames-per-second in FireFox, try Chrome.",
    "Tip: The particles flying around are fish. Shoot them to grow bigger.",
    "Tip: Larger fleets move, shoot, and reload slower than small fleets.",
    "Tip: Chat with other players, arrange duels, and more using the Discord link below.",
    "Tip: Trouble focusing your fire? Try adjusting mouse-sensitivity option in settings.",
    "Tip: Don't be afraid of dying. Soon others will!",
    "Tip: Fire before you boost, boost before your shots hit your opponent.",
    "Tip: Arrows on the leaderboard point to the players.",
    "Tip: Daud.io is open source and changing quickly. Join Discord to help."
];

let index = 1; // zero is duplicated in the HTML
window.setInterval(() => {
    hintbox.innerText = texts[index % texts.length];
    index++;
}, 6000);
