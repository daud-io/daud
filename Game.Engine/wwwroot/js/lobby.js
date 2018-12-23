import { fetch } from "whatwg-fetch";
import { Controls } from "./controls";

var worlds = document.getElementById("worlds");
var worldList = document.getElementById("worldList");

var allWorlds = false;

function buildList(response) {
    if (allWorlds != false) return updateList(response);

    allWorlds = {};

    var options = "";
    for (var world of response) {
        allWorlds[world.world] = world;
        options += `<tr id="${world.world}_row"><td><button id="${world.world}" class="j">Join</button></div></td><td>(<span id="${world.world}_playercount">${world.players}</span>)</td><td><b>${
            world.name
        }</b>: ${world.description}</td></tr>`;
    }

    worldList.innerHTML = options;
}

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

                // with jquery, I'd have done this with a handler on the parent
                // element and then looked at a $(sourceElement).data('world')
                // not sure what a xplat/non-jq equiv would be
                document.querySelectorAll(".j").forEach(j =>
                    j.addEventListener("click", function() {
                        const world = this.id;
                        window.Game.primaryConnection.disconnect();
                        window.Game.primaryConnection.connect(world);
                        changeRoom(world);
                    })
                );
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
