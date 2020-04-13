import { Router } from "./router";

const worlds = document.getElementById("worlds");
const worldList = document.getElementById("worldList");

let allWorlds = null;
let lastKeys = null;

function selectRow(selectedWorld) {
    for (const world in allWorlds) {
        const row = document.getElementById(`${world}_row`);
        if (world == selectedWorld) row.classList.add("selected");
        else row.classList.remove("selected");
    }
}

function buildList(response) {
    if (allWorlds != null) {
        let keys = "";
        response.forEach((w) => (keys += ":" + w.world));

        if (lastKeys == keys) return updateList(response);
        else lastKeys = keys;
    }

    allWorlds = {};

    let options = "";
    for (const world of response) {
        allWorlds[world.world] = world;

        options += `<tbody id="${world.world}_row" world="${world.world}" class="worldrow">`;
        options +=
            `<tr>` +
            `<td><button class="button1 button3" id="join">Join</button> (<span id="${world.world}_playercount">${world.players}</span>)</td>` +
            `<td id="second-world-td"><b>${world.name}</b>: ${world.description}</td>` +
            `</tr>`;

        const img = world.image ? `<img src="/img/worlds/${world.image}.png" />` : "";
        if (world.instructions || img) options += `<tr class="details"><td colspan="3">${img}${world.instructions || ""}</td></tr>`;
        options += `</tbody>`;
    }

    worldList.innerHTML = `${options}`;

    document.querySelectorAll(".worldrow").forEach((worldRow) =>
        worldRow.addEventListener("click", function (e) {
            const worldKey = this.getAttribute("world");

            if ((e.srcElement as HTMLElement).tagName == "BUTTON") joinWorld(worldKey);
            else selectRow(worldKey);
        })
    );
}

function updateList(response) {
    for (const world of response) {
        document.getElementById(`${world.world}_playercount`).innerHTML = world.players;
        const row = document.getElementById(`${world.world}_row`);

        if (world.players > 0) row.classList.remove("empty");
        else row.classList.add("empty");
    }
}

const controls = document.querySelector(".controls");
const social = document.querySelector(".social");
let showing = false;
let firstLoad = true;
let hostName = window.location.hash;
let worldConnect = "default";
let manualHostSet = false;
let manualWorldSet = false;

if (firstLoad) {
    const url = new URL(window.location.href);
    const hostParam = url.searchParams.get("host");

    if (hostParam != null) {
        manualHostSet = true;
        hostName = hostParam;
    } else {
        hostName = "us.daud.io";
    }

    const worldParam = url.searchParams.get("world");

    if (worldParam != null) {
        manualWorldSet = true;
        worldConnect = worldParam;
    }
}

export const LobbyCallbacks = {
    onLobbyClose: null,
    onWorldJoin: null,
    joinWorld: null,
};

LobbyCallbacks.joinWorld = function (worldKey) {
    refreshList(worldKey);
};

function refreshList(autoJoinWorld) {
    if (!showing && !firstLoad && !autoJoinWorld) return;

    const autoJoin = firstLoad || autoJoinWorld;

    firstLoad = false;

    window
        .fetch("/api/v1/world/all", {
            method: "GET",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
            },
        })
        .then((r) => r.json())
        .then(({ success, response }) => {
            const world = worldConnect;
            if (success) {
                if (window.location.hash) {
                    const selected = window.location.hash.substring(1);
                    window.Game.primaryConnection.connect(selected);
                }

                buildList(response);

                if (autoJoin) {
                    const worldKey = hostName + "/" + world;

                    if (!autoJoinWorld) {
                        if (manualHostSet) {
                            //If user manually sets a particular host via params
                            manualHostSet = false;
                            joinWorld(worldKey);
                            return;
                        }

                        const router = new Router();

                        if (router.savedBestServer) {
                            // If there is no cookie saved with the best server.
                            joinWorld(router.savedBestServer + world);
                        } else {
                            // if there is no best server cached cookie or the cookie expired then find the best server for user.
                            router.findBestServer(["us.daud.io/default", "de.daud.io/default"], (best) => {
                                if (allWorlds[best]) {
                                    best = best = best.split("/")[0] + "/";
                                    router.save(best);

                                    joinWorld(best + world);
                                } else {
                                    joinWorld(worldKey);
                                }
                            });
                        }
                    } else joinWorld(autoJoinWorld || worldKey);
                }
            }
        });
}

function hide() {
    worlds.classList.add("closed");
    controls.classList.remove("blur");
    social.classList.remove("blur");
    document.body.classList.remove("lobby");
    showing = false;

    if (LobbyCallbacks.onLobbyClose) LobbyCallbacks.onLobbyClose();
}

function show() {
    controls.classList.add("blur");
    social.classList.add("blur");
    document.body.classList.add("lobby");
    showing = true;
}

function joinWorld(worldKey) {
    if (LobbyCallbacks.onWorldJoin) LobbyCallbacks.onWorldJoin(worldKey, allWorlds[worldKey]);
    hide();
}

export function toggleLobby() {
    if (!showing) show();
    else hide();
}

document.getElementById("wcancel").addEventListener("click", (e) => {
    if (showing) hide();
});

document.getElementById("arenas").addEventListener("click", (e) => {
    show();
    refreshList(false);
    worlds.classList.remove("closed");
    e.preventDefault();
    return false;
});

refreshList(false);
setInterval(refreshList, 1000);
