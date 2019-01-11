// about card open and close script

var aboutCard = document.getElementById("fullPageCard")

document.getElementById("openArrow").addEventListener("click", function() {
    aboutCard.classList.remove("hidden");
});

document.getElementById("close").addEventListener("click", function() {
    aboutCard.classList.add("hidden");
});

setTimeout(function(){
	aboutCard.removeAttribute("hidden");
},1000);