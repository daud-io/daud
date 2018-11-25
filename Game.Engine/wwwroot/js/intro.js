import changes from '/changes.html';
var parser = new DOMParser();
var doc = parser.parseFromString(changes, "text/html");
document.getElementById("twitterpated").innerHTML = doc.body.innerHTML;
