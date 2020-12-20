import Cookies from "js-cookie";

const spawn = document.getElementById("spawn") as HTMLButtonElement;
const dauth = document.getElementById("dauth") as HTMLButtonElement;
dauth.addEventListener("click", () => {
    window.location.assign(`https://discordapp.com/api/oauth2/authorize?response_type=token&client_id=514844767511642112&scope=identify&redirect_uri=${encodeURIComponent(window.location.origin)}`);
});

const secondsToDays = 60 * 60 * 24;
const sp = new URLSearchParams(window.location.hash.substr(1));
const token = sp.get("access_token") || Cookies.get("auth_token");
export function getToken(): string {
    return token || "";
}
if (token) {
    history.pushState({}, "", "/");
    dauth.style.display = "none";
    spawn.style.width = "370px";
    spawn.innerText = "Launch!";

    if (sp.get("access_token")) {
        const expirationSeconds = parseFloat(sp.get("expires_in")!);
        const cookieOptions = { expires: expirationSeconds / secondsToDays };
        Cookies.set("auth_token", token, cookieOptions);
    }
} else if (window.frameElement) {
    spawn.innerText = "Launch!";
    spawn.style.width = "370px";
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
