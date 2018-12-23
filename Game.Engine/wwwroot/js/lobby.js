import { fetch } from "whatwg-fetch";
import { Controls } from "./controls";

var worlds = document.getElementById("worlds");
var worldList = document.getElementById("worldList");

var allWorlds = false;
var lastSelected = false;

function selectRow(selectedWorld) {
    if (lastSelected == selectedWorld) selectedWorld = false;

    for (var world in allWorlds) {
        var row = document.getElementById(`${world}_row`);
        if (world == selectedWorld) row.classList.add("selected");
        else row.classList.remove("selected");
    }

    lastSelected = selectedWorld;
}
const imgs = require(`../img/worlds/*.png`);
function buildList(response) {
    if (allWorlds != false) return updateList(response);

    allWorlds = {};

    var options = "<tbody>";
    for (var world of response) {
        allWorlds[world.world] = world;

        var img = world.image ? `<img src="${imgs[world.image]}" />` : "";

        options += `<tr id="${world.world}_row" world="${world.world}" class="worldrow">`;

        options =
            options +
            // `<td><button id="${world.world}" class="j">Join</button></td>` +
            `<td>(<span id="${world.world}_playercount">${world.players}</span>)</td>` +
            `<td><b>${world.name}</b>: ${world.description}</td>`;

        if (world.instructions || img) options = options + `<td colspan="3" class="details">${img}${world.instructions || ""}</td>`;

        options += `</tr>`;
    }
    options += `</tbody>`;

    worldList.innerHTML = options;

    document.querySelectorAll(".worldrow").forEach(worldRow =>
        worldRow.addEventListener("click", function() {
            selectRow(this.getAttribute("world"));
        })
    );
}

document.getElementById("join").addEventListener("click", function() {
    const world = document.querySelector(".selected").getAttribute("world");
    window.Game.primaryConnection.disconnect();
    window.Game.primaryConnection.connect(world);
    changeRoom(world);
    selectRow(world);
});

function updateList(response) {
    for (var world of response) {
        document.getElementById(`${world.world}_playercount`).innerHTML = world.players;
        var row = document.getElementById(`${world.world}_row`);

        if (world.players > 0) row.classList.remove("empty");
        else row.classList.add("empty");
    }
}

function refreshList() {
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
                    changeRoom(selected);
                }

                buildList(response);
            }
        });
}

function changeRoom(worldKey) {
    document.getElementById("wcancel").click();
    var world = allWorlds[worldKey];
    if (world) {
        var colors = world.allowedColors;
        var options = "";

        for (var i = 0; i < colors.length; i++) options += `<option value="${colors[i]}">${colors[i]}</option>`;

        document.getElementById("shipSelector").innerHTML = options;
        document.getElementById("shipSelector").value = colors[0];
        Controls.color = colors[0];
    } else console.log(`Warning: could not find selected world ${worldKey}`);
}

var controls = document.querySelector(".controls");
var social = document.querySelector(".social");
var blurred = false;
export function blur() {
    if (!blurred) {
        controls.classList.add("blur");
        social.classList.add("blur");
    } else {
        controls.classList.remove("blur");
        social.classList.remove("blur");
    }
    blurred = !blurred;
}

document.getElementById("arenas").addEventListener("click", () => {
    refreshList();
    worlds.classList.remove("closed");
    blur();
});
document.getElementById("wrefresh").addEventListener("click", () => {
    refreshList();
});

setInterval(refreshList, 1000);
