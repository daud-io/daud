import { fetch } from "whatwg-fetch";
import { Cache } from "./cache";
import * as dat from "dat.gui";

export const gui = new dat.GUI({ autoPlace: false });
document.querySelector(".ansible").appendChild(gui.domElement);
gui.domElement.style.position = "absolute";
gui.domElement.style.bottom = "0";
gui.domElement.style.right = "15px";
gui.domElement.firstChild.style.maxHeight = "50vh";
gui.domElement.firstChild.style.overflowY = "scroll";
gui.__closeButton.style.display = "none";
gui.closed = true;

const hooks = {
    Ping: new Function(),
    Bandwidth: new Function(),
    FPS: new Function()
};

const latencyDisplay = gui.add(hooks, "Ping");
const bandwidthDisplay = gui.add(hooks, "Bandwidth");
const statsDisplay = gui.add(hooks, "FPS");

const token = fetch("/api/v1/user/authenticate", {
    method: "POST",
    headers: {
        "Content-Type": "application/json; charset=utf-8"
    },
    body: JSON.stringify({
        Identifier: {
            UserKey: "Administrator"
        },
        password: ""
    })
})
    .then(r => r.json())
    .then(({ response }) => response.token)
    .then(r => {
        fetch("/api/v1/server/hook", {
            method: "POST",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
                Authorization: `Bearer ${r}`
            },
            body: "{}"
        })
            .then(r => r.json())
            .then(({ response }) => {
                const obj = JSON.parse(response);
                for (const key in obj) {
                    hooks[key] = obj[key] === 0 ? obj[key] + 0.01 : obj[key];
                }
                for (const key in hooks) {
                    if (typeof hooks[key] == "boolean") {
                        gui.add(hooks, key).onChange(bind_param(key));
                    } else if (typeof hooks[key] != "function") {
                        let min;
                        let max;
                        let step;
                        if (hooks[key] < 0) {
                            min = -1;
                            max = 0;
                            step = 0.000001;
                        } else if (hooks[key] <= 1) {
                            min = 0;
                            max = 1;
                            step = 0.000001;
                        } else {
                            min = 0;
                            max = 10 ** Math.ceil(Math.log10(hooks[key] + 1));
                            step = 1;
                        }

                        if (step) {
                            gui.add(hooks, key, min, max, step).onChange(bind_param(key));
                        } else {
                            gui.add(hooks, key, min, max).onChange(bind_param(key));
                        }
                    }
                }
            });

        return r;
    });

async function send_hook(attr) {
    const changer = {};
    changer[attr] = hooks[attr];
    fetch("/api/v1/server/hook", {
        method: "POST",
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            Authorization: `Bearer ${await token}`
        },
        body: JSON.stringify(changer)
    });
}

function bind_param(a) {
    return () => send_hook(a);
}

const connection = window.Game.primaryConnection;
setInterval(() => {
    statsDisplay.name(`vps:${window.Game.Stats.viewsPerSecond} ups:${window.Game.Stats.updatesPerSecond} fps:${window.Game.Stats.framesPerSecond} cs:${Cache.count}`);

    if (connection !== null) {
        bandwidthDisplay.name(`bandwidth: ${(Math.floor(connection.statBytesUpPerSecond / 102.4) / 10) * 8}Kb/s up ${(Math.floor(connection.statBytesDownPerSecond / 102.4) / 10) * 8}Kb/s down`);
        latencyDisplay.name(`ping: ${connection.latency ? Math.floor(connection.latency * 100) / 100 + " ms" : "n/a"}`);
    } else latencyDisplay.name("ping: n/a");
}, 1000);
