import { bestServer, findBestServer, save } from "./router";
import { html, render, Hole } from "uhtml";
import * as bus from "./bus";

export type ServerWorld = {
    world: string;
    server: {
        value: string;
        hasValue: boolean;
        host: string;
        port: number;
    };
    players: number;
    name: string;
    description: string;
    allowedColors: string[];
    instructions: string;
    isDefault: boolean;
};

var worldsWrapper:HTMLElement;
var worldList:HTMLElement;
let allWorlds: Record<string, ServerWorld> = {};
var refreshIntervalHandle:NodeJS.Timer;

function buildList(response: ServerWorld[]) {
    allWorlds = {};

    const options: Hole[] = [];
    for (const world of response) {
        allWorlds[world.world] = world;

        options.push(
            html`<tbody
                id=${world.world + "_row"}
                onclick=${(e: MouseEvent) => {
                    if ((e.target as HTMLElement).tagName == "BUTTON") joinWorld(world.world);
                    else selectRow(world.world);
                }}
                class=${"worldrow" + (world.players ? "" : " empty")}
            >
                <tr>
                    <td><button>Join</button> (<span id=${world.world + "_playercount"}>${world.players}</span>)</td>
                    <td id="second-world-td"><b>${world.name}</b>: ${world.description}</td>
                </tr>
                ${world.instructions
                    ? html`<tr class="details">
                        <td colspan="3" .innerHTML=${world.instructions}></td>
                    </tr>`
                    : ""}
            </tbody>`
        );
    }

    render(worldList, html`${options}`);
}

function selectRow(selectedWorld: string) {
    for (const world in allWorlds) {
        const row = document.getElementById(`${world}_row`)!;
        if (world == selectedWorld) row.classList.add("selected");
        else row.classList.remove("selected");
    }
}


let showing = false;
function hide() {
    worldsWrapper.classList.add("closed");
    document.body.classList.remove("lobby");
    showing = false;
    clearInterval(refreshIntervalHandle);
}

function show() {
    document.body.classList.add("lobby");
    showing = true;
    refreshIntervalHandle = setInterval(refreshList, 1000);
}

bus.on("pageReady", () => {

    worldsWrapper = document.getElementById("worldsWrapper")!;
    worldList = document.getElementById("worldList")!;

    document.getElementById("wcancel")!.addEventListener("click", () => {
        if (showing) hide();
    });

    document.getElementById("arenas")!.addEventListener("click", (e) => {
        show();
        refreshList();
        worldsWrapper.classList.remove("closed");
        e.preventDefault();
        return false;
    });
});

export async function firstLoad(): Promise<void> {
    console.log("firstload calling refreshlist");
    await refreshList();
    console.log();
    const url = new URLSearchParams(window.location.search);

    const hostName = window.location.hash.substr(1) || url.get("host");
    const worldConnect = url.get("world") || "default";

    //If user manually sets a particular host via params
    //if (hostName) {
    //    joinWorld(`${hostName}/${worldConnect}`);
    //    return;
    //}

    for (const worldKey in allWorlds) {
        const world = allWorlds[worldKey];
        if (world.isDefault) {
            console.log("joining default world from lobby list");
            joinWorld(worldKey);
            return;
        }
    }

    if (bestServer) {
        // If there is a cookie saved with the best server.
        joinWorld(bestServer + worldConnect);
    } else {
        // if there is no best server cached cookie or the cookie expired then find the best server for user.
        findBestServer(["us.daud.io/default", "de.daud.io/default"], (best) => {
            if (allWorlds[best]) {
                best = best = best.split("/")[0] + "/";
                save(best);
            } else {
                joinWorld(`us.daud.io/${worldConnect}`);
            }
        });
    }
}

export async function refreshList(): Promise<void> {
    const url = new URLSearchParams(window.location.search);
    const host = url.get("host") || "daud.io";
    const fetched = await window.fetch(`/api/v1/world/all`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json; charset=utf-8",
        },
    });
    const { response }: { response: ServerWorld[] } = await fetched.json();

    buildList(response);
}

export function joinWorld(worldKey: string): void {
    bus.emit("worldjoin", worldKey, allWorlds[worldKey]);
    hide();
}

