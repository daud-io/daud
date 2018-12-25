// about card open and close script

document.getElementById("openArrow").addEventListener("click", function() {
  document.getElementById("fullPageCard").classList.remove("hidden");
});

document.getElementById("close").addEventListener("click", function() {
  document.getElementById("fullPageCard").classList.add("hidden");
});
