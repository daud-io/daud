import { fetch } from "whatwg-fetch";
import { Controls } from "./controls";
import { __esModule } from "pixi.js/lib/core";

const worlds = document.getElementById("worlds");
const worldList = document.getElementById("worldList");

let allWorlds = false;

function selectRow(selectedWorld) {
    for (const world in allWorlds) {
        const row = document.getElementById(`${world}_row`);
        if (world == selectedWorld) row.classList.add("selected");
        else row.classList.remove("selected");
    }
}
const imgs = require(`../img/worlds/*.png`);
function buildList(response) {
    if (allWorlds != false) return updateList(response);

    allWorlds = {};

    let options = "";
    for (const world of response) {
        allWorlds[world.world] = world;

        options += `<tbody id="${world.world}_row" world="${world.world}" class="worldrow">`;
        options +=
            `<tr>` +
            `<td><button class="button1" id="join">Join</button> (<span id="${world.world}_playercount">${world.players}</span>)</td>` +
            `<td><b>${world.name}</b>: ${world.description}</td>` +
            `</tr>`;

        const img = world.image ? `<img src="${imgs[world.image]}" />` : "";
        if (world.instructions || img) options += `<tr class="details"><td colspan="2">${img}${world.instructions || ""}</td></tr>`;
        options += `</tbody>`;
    }

    worldList.innerHTML = `${options}`;

    document.querySelectorAll(".worldrow").forEach(worldRow =>
        worldRow.addEventListener("click", function(e) {
            const worldKey = this.getAttribute("world");

            if (e.srcElement.tagName == "BUTTON") joinWorld(worldKey);
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

export const LobbyCallbacks = {
    onLobbyClose: false,
    onWorldJoin: false
};

LobbyCallbacks.joinWorld = function(worldKey) {
    refreshList(worldKey);
};

function refreshList(autoJoinWorld) {
    if (!showing && !firstLoad && !autoJoinWorld) return;

    const autoJoin = firstLoad || autoJoinWorld;
    firstLoad = false;

    fetch("/api/v1/server/worlds", {
        method: "GET",
        headers: {
            "Content-Type": "application/json; charset=utf-8"
        }
    })
        .then(r => r.json())
        .then(({ success, response }) => {
            if (success) {
                if (window.location.hash) {
                    const selected = window.location.hash.substring(1);
                    window.Game.primaryConnection.connect(selected);
                }

                buildList(response);

                if (autoJoin) joinWorld(autoJoinWorld || "default");
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

document.getElementById("wcancel").addEventListener("click", e => {
    if (showing) hide();
});

document.getElementById("arenas").addEventListener("click", e => {
    show();
    refreshList();
    worlds.classList.remove("closed");
    e.preventDefault();
    return false;
});

//refreshList();
setInterval(refreshList, 1000);
