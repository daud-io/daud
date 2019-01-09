import { fetch } from "whatwg-fetch";
import { Controls } from "./controls";
import { __esModule } from "pixi.js/lib/core";

var worlds = document.getElementById("worlds");
var worldList = document.getElementById("worldList");

var allWorlds = false;

function selectRow(selectedWorld) {
    for (var world in allWorlds) {
        var row = document.getElementById(`${world}_row`);
        if (world == selectedWorld) row.classList.add("selected");
        else row.classList.remove("selected");
    }
}
const imgs = require(`../img/worlds/*.png`);
function buildList(response) {
    if (allWorlds != false) return updateList(response);

    allWorlds = {};

    var options = "";
    for (var world of response) {
        allWorlds[world.world] = world;

        options += `<tbody id="${world.world}_row" world="${world.world}" class="worldrow">`;
        options += `<tr>` + `<td><button class="button1" id="join">Join</button> (<span id="${world.world}_playercount">${world.players}</span>)</td>` + `<td><b>${world.name}</b>: ${world.description}</td>` + `</tr>`;

        var img = world.image ? `<img src="${imgs[world.image]}" />` : "";
        if (world.instructions || img) options += `<tr class="details"><td colspan="2">${img}${world.instructions || ""}</td></tr>`;
        options += `</tbody>`;
    }

    worldList.innerHTML = `${options}`;

    document.querySelectorAll(".worldrow").forEach(worldRow =>
        worldRow.addEventListener("click", function (e) {
            var worldKey = this.getAttribute("world");

            if (e.srcElement.tagName == "BUTTON")
                joinWorld(worldKey);
            else
                selectRow(worldKey);
        })
    );
}

function updateList(response) {
    for (var world of response) {
        document.getElementById(`${world.world}_playercount`).innerHTML = world.players;
        var row = document.getElementById(`${world.world}_row`);

        if (world.players > 0) row.classList.remove("empty");
        else row.classList.add("empty");
    }
}

var controls = document.querySelector(".controls");
var social = document.querySelector(".social");
var showing = false;
var firstLoad = true;

export var LobbyCallbacks = {
    onLobbyClose: false,
    onWorldJoin: false
}

function refreshList() {
    if (!showing && !firstLoad)
        return;
    
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
                    var selected = window.location.hash.substring(1);
                    window.Game.primaryConnection.connect(selected);
                }

                buildList(response);

                if (firstLoad)
                    joinWorld('default')

                firstLoad = false;
            }
        });
}

function hide()
{
    worlds.classList.add("closed");
    controls.classList.remove("blur");
    social.classList.remove("blur");
    document.body.classList.remove("lobby");
    showing = false;

    if (LobbyCallbacks.onLobbyClose)
        LobbyCallbacks.onLobbyClose();
}

function show()
{
    controls.classList.add("blur");
    social.classList.add("blur");
    document.body.classList.add("lobby");
    showing = true;
}

function joinWorld(worldKey) {
    LobbyCallbacks.onWorldJoin(worldKey, allWorlds[worldKey]);
    hide();
}

export function toggleLobby() {
    if (!showing)
        show();
    else
        hide();
}

document.getElementById("wcancel").addEventListener("click", (e) => {
    if (showing)
        hide();
});

document.getElementById("arenas").addEventListener("click", (e) => {
    show();
    refreshList();
    worlds.classList.remove("closed");
    e.preventDefault();
    return false;
});

refreshList();
setInterval(refreshList, 1000);
