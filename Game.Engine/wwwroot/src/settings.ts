import JSZip from "jszip";
import Cookies from "js-cookie";
import { loadTexture, merge, setDefinitions, load } from "./loader";
import { initializeWorld } from "./controls";
import * as cache from "./cache";
import { allProgress } from "./loader";
import bus from "./bus";

export const Settings = {
    theme: "",
    mouseScale: 1.0,
    bandwidth: 100,
    logLength: 4,
    showHints: true,
    nameSize: 48,
    background: true,
};

const themeSelector = document.getElementById("settingsTheme") as HTMLSelectElement;
const mouseScale = document.getElementById("settingsMouseScale") as HTMLInputElement;
const showHints = document.getElementById("settingsShowHints") as HTMLInputElement;
const bandwidth = document.getElementById("settingsBandwidth") as HTMLInputElement;
const logLength = document.getElementById("settingsLog") as HTMLInputElement;
const nameSize = document.getElementById("settingsNameSize") as HTMLInputElement;
const backgroundEl = document.getElementById("settingsShowBackground") as HTMLInputElement;
loadSettings();

themeSelector.onchange = async () => {
    Settings.theme = themeSelector.value;
    if (Settings.theme) await theme();
    else await load();
    bus.emit("loaded");
    cache.refreshSprites();
    initializeWorld();
    save();
};
mouseScale.onchange = () => (Settings.mouseScale = Number(mouseScale.value)) && save();
showHints.onchange = () => (Settings.showHints = showHints.checked) && save();
bandwidth.onchange = () => (Settings.bandwidth = Number(bandwidth.value)) && save();
logLength.onchange = () => (Settings.logLength = Number(logLength.value)) && save();
nameSize.onchange = () => (Settings.nameSize = Number(nameSize.value)) && save();
backgroundEl.onchange = () => (Settings.background = backgroundEl.checked) && save();

function save() {
    bus.emit("settings");
    Cookies.set("settings", Settings, { expires: 300 });
}

export function loadSettings(): void {
    const savedSettings = JSON.parse(Cookies.get("settings") || "false");

    if (savedSettings) {
        // copying value by value because cookies can be old versions
        // any values NOT in the cookie will remain defined with the new defaults
        for (const key in savedSettings) Settings[key] = savedSettings[key];
    }

    themeSelector.value = Settings.theme;
    mouseScale.value = String(Settings.mouseScale);
    showHints.checked = Settings.showHints;
    bandwidth.value = String(Settings.bandwidth);
    logLength.value = String(Settings.logLength);
    nameSize.value = String(Settings.nameSize);
    backgroundEl.checked = Settings.background;
}

export async function theme(): Promise<void> {
    //`https://dl.dropboxusercontent.com/s/${v.toLowerCase()}/daudmod.zip`;
    const zip = await window
        .fetch(Settings.theme)
        .then((response) => response.blob())
        .then(JSZip.loadAsync);

    const text = await zip.file("daudmod/info.json")!.async("string");
    const info = JSON.parse(text);

    const parsedJSON: any = {};

    const all = Object.keys(info).map(async (key) => {
        const defaultKey = info[key].extends;
        if (defaultKey) {
            parsedJSON[key] = merge(info[defaultKey], info[key]);
            parsedJSON[key].abstract = info[key].abstract;
        } else {
            parsedJSON[key] = info[key];
        }

        if (!parsedJSON[key].abstract) {
            const file = zip.file(`daudmod/${parsedJSON[key].url}`);
            if (!file) throw new Error("Missing file: " + parsedJSON[key].url);
            const ab = await file.async("arraybuffer");
            const arrayBufferView = new Uint8Array(ab);
            const blob = new Blob([arrayBufferView], { type: "image/png" });
            const url = URL.createObjectURL(blob);
            parsedJSON[key].url = url;
            return await loadTexture(parsedJSON[key]);
        }
    });
    await allProgress(all);
    setDefinitions(parsedJSON);
}

const gear = document.getElementById("gear")!;
document.getElementById("settings")!.addEventListener("click", () => {
    gear.classList.remove("closed");
});

document.getElementById("settingsClose")!.addEventListener("click", () => {
    gear.classList.add("closed");
});
