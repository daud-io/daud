import Cookies from "js-cookie";

const dauth = document.getElementById("dauth");
dauth.addEventListener("click", () => {
    window.location.assign(`https://discordapp.com/api/oauth2/authorize?response_type=token&client_id=514844767511642112&scope=identify&redirect_uri=${encodeURIComponent(window.location.origin)}`);
});

const secondsToDays = 60 * 60 * 24;
const sp = new URLSearchParams(window.location.hash.substr(1));
export function getToken() {
    return token;
}
const token = sp.get("access_token") || Cookies.get("auth_token");
if (token) {
    history.pushState({}, "", "/");
    dauth.style.display = "none";
    (<HTMLButtonElement>dauth.previousElementSibling).value = "Launch";

    if (sp.get("access_token")) {
        const expirationSeconds = parseFloat(sp.get("expires_in"));
        const cookieOptions = { expires: expirationSeconds / secondsToDays };
        Cookies.set("auth_token", token, cookieOptions);
    }
} else if (window.frameElement) {
    (<HTMLButtonElement>dauth.previousElementSibling).value = "Launch";
    dauth.style.display = "none";
}

if (token) {
    window
        .fetch("https://discordapp.com/api/users/@me", {
            method: "GET",
            headers: {
                Authorization: `Bearer ${token}`,
            },
        })
        .then((r) => {
            if (!r.ok) {
                Cookies.remove("auth_token");
            }
        });
}
