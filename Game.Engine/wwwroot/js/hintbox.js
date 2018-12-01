var hintbox = document.getElementById("hintbox");

var texts = ['Controls: Mouse to aim, click to fire, press "s" to boost!', "The particles flying around are fish. Shoot them to grow bigger"];

var index = 1;
window.setInterval(function() {
    hintbox.innerText = texts[index % texts.length];
    index++;
}, 4000);
