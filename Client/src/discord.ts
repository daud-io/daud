import { Cookies } from "./cookies";
import * as bus from "./bus";

const secondsToDays = 60 * 60 * 24;
const sp = new URLSearchParams(window.location.hash.substr(1));
const token = sp.get("access_token") || Cookies.get("auth_token");

export function getToken(): string {
    return token || "";
}

bus.on("pageReady", () => {

    const spawn = document.getElementById("spawn") as HTMLButtonElement;
    const dauth = document.getElementById("dauth") as HTMLButtonElement;
    dauth.addEventListener("click", () => {
        window.location.assign(`https://discordapp.com/api/oauth2/authorize?response_type=token&client_id=524285465793396741&scope=identify&redirect_uri=${encodeURIComponent(window.location.origin)}`);
    });

    if (token) {
        history.pushState({}, "", "/");
        dauth.classList.add("authenticated");
        dauth.innerText = "Logged in";
        dauth.disabled = true;

        if (sp.get("access_token")) {
            const expirationSeconds = parseFloat(sp.get("expires_in")!);
            const cookieOptions = { expires: expirationSeconds / secondsToDays };
            Cookies.set("auth_token", token, cookieOptions);
        }
    } else if (window.frameElement) {
        spawn.innerText = "Play";
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
            })
            .catch(function (error) {
                console.log(error);
            });
    }
});
