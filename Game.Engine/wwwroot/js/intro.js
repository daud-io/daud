import changes from "/changes.html";
const parser = new DOMParser();
const doc = parser.parseFromString(changes, "text/html");
document.getElementById("twitterpated").innerHTML = doc.body.innerHTML;
