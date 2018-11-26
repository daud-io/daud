import Cookies from "js-cookie";

var dauth = document.getElementById("dauth");
dauth.addEventListener("click", () => {
    window.location = `https://discordapp.com/api/oauth2/authorize?response_type=token&client_id=514844767511642112&scope=identify&redirect_uri=${encodeURIComponent(window.location.origin)}`;
});

var sp = new URLSearchParams(window.location.hash.substr(1));
export var token = sp.get("access_token");
if (token || Cookies.get("access_token")) {
    history.pushState({}, "", "/");
    dauth.style.display = "none";
    dauth.previousElementSibling.value = "Launch";
    token = token || Cookies.get("access_token");
    Cookies.set("access_token", token);
} else if (window.frameElement) {
    dauth.previousElementSibling.value = "Launch";
    dauth.style.display = "none";
}
