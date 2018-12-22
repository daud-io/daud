import Cookies from "js-cookie";

const dauth = document.getElementById("dauth");
dauth.addEventListener("click", () => {
    window.location = `https://discordapp.com/api/oauth2/authorize?response_type=token&client_id=514844767511642112&scope=identify&redirect_uri=${encodeURIComponent(window.location.origin)}`;
});

const sp = new URLSearchParams(window.location.hash.substr(1));
export let token = sp.get("access_token");
if (token || Cookies.get("daud_auth_token")) {
    history.pushState({}, "", "/");
    dauth.style.display = "none";
    dauth.previousElementSibling.value = "Launch";
    token = token || Cookies.get("daud_auth_token");

    let expirationSeconds = sp.get("expires_in") || 60 * 60 * 24 * 10;
    let cookieOptions = { expires: new Date(new Date().getTime() + expirationSeconds * 1000) };
    Cookies.set("daud_auth_token", token, cookieOptions);
} else if (window.frameElement) {
    dauth.previousElementSibling.value = "Launch";
    dauth.style.display = "none";
}
