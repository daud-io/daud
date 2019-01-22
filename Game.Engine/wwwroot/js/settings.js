import { fetch } from "whatwg-fetch";
import Cookies from "js-cookie";
import JSZip from "jszip";
import { textureMap } from "./models/textureMap";
import { spriteModeMap } from "./models/spriteModeMap";
import { textureCache } from "./models/textureCache";

export const Settings = {
    theme: false,
    themeCustom: false,
    mouseScale: 1.0,
    font: "sans-serif",
    leaderboardEnabled: true,
    displayMinimap: "always",
    hudEnabled: true,
    namesEnabled: true,
    bandwidth: 100,
    showCooldown: true,
    logLength: 4,
    displayMinimap: true,
    bigKillMessage: true,
    showPickupSprites: false,
    showThrusterSprites: true,
    showOwnName: true,
    nameSize: 48,
    background: "on"
};

function save() {
    const cookieOptions = { expires: 300 };
    let reload = false;

    if (Settings.theme != document.getElementById("settingsThemeSelector").value) {
        Settings.theme = document.getElementById("settingsThemeSelector").value;
        theme(Settings.theme);
    }
    if (Settings.themeCustom != document.getElementById("settingsThemeSelectorCustom").value) {
        Settings.themeCustom = document.getElementById("settingsThemeSelectorCustom").value;
        theme(Settings.themeCustom);
    }

    Settings.mouseScale = document.getElementById("settingsMouseScale").value;
    Settings.font = document.getElementById("settingsFont").value;
    Settings.leaderboardEnabled = document.getElementById("settingsLeaderboardEnabled").checked;
    Settings.displayMinimap = document.getElementById("settingsDisplayMinimap").value;
    Settings.namesEnabled = document.getElementById("settingsNamesEnabled").checked;
    Settings.bandwidth = document.getElementById("settingsBandwidth").value;
    Settings.hudEnabled = document.getElementById("settingsHUDEnabled").checked;
    Settings.showCooldown = document.getElementById("settingsShowCooldown").checked;
    Settings.logLength = document.getElementById("settingsLog").value;
    Settings.displayMinimap = document.getElementById("settingsDisplayMinimap").checked;
    Settings.bigKillMessage = document.getElementById("settingsBigKillMessage").checked;
    Settings.showPickupSprites = document.getElementById("settingsShowPickupSprites").checked;
    Settings.showThrusterSprites = document.getElementById("settingsShowThrusterSprites").checked;
    Settings.showOwnName = document.getElementById("settingsShowOwnName").checked;
    Settings.nameSize = Number(document.getElementById("settingsNameSize").value);
    Settings.background = document.getElementById("settingsBackground").value;

    Cookies.set("settings", Settings, cookieOptions);

    if (reload) window.location.reload();
}

function reset() {
    Cookies.remove("settings");
}

function load() {
    const savedSettings = Cookies.getJSON("settings");

    if (savedSettings) {
        // copying value by value because cookies can be old versions
        // any values NOT in the cookie will remain defined with the new defaults
        for (const key in savedSettings) Settings[key] = savedSettings[key];

        if (Settings.theme == "3ds2agh4z76feci") Settings.theme = "516mkwof6m4d4tg";
    }

    document.getElementById("settingsThemeSelector").value = Settings.theme;
    document.getElementById("settingsThemeSelectorCustom").value = Settings.themeCustom || "";

    document.getElementById("settingsMouseScale").value = Settings.mouseScale;
    document.getElementById("settingsFont").value = Settings.font;
    document.getElementById("settingsLeaderboardEnabled").checked = Settings.leaderboardEnabled;
    document.getElementById("settingsDisplayMinimap").checked = Settings.displayMinimap;
    document.getElementById("settingsNamesEnabled").checked = Settings.namesEnabled;
    document.getElementById("settingsBandwidth").value = Settings.bandwidth;
    document.getElementById("settingsHUDEnabled").checked = Settings.hudEnabled;
    document.getElementById("settingsShowCooldown").checked = Settings.showCooldown;
    document.getElementById("settingsLog").value = Settings.logLength;
    document.getElementById("settingsDisplayMinimap").checked = Settings.displayMinimap;
    document.getElementById("settingsBigKillMessage").checked = Settings.bigKillMessage;
    document.getElementById("settingsShowPickupSprites").checked = Settings.showPickupSprites;
    document.getElementById("settingsShowThrusterSprites").checked = Settings.showThrusterSprites;
    document.getElementById("settingsShowOwnName").checked = Settings.showOwnName;
    document.getElementById("settingsNameSize").value = Settings.nameSize;
    document.getElementById("settingsBackground").value = Settings.background;
}

async function theme(v) {
    if (v)
        v = v.toLowerCase();
    const link = `https://dl.dropboxusercontent.com/s/${v}/daudmod.zip`;
    const zip = await fetch(link)
        .then(response => response.blob())
        .then(JSZip.loadAsync);
    zip.file("daudmod/info.json")
        .async("string")
        .then(text => {
            const info = JSON.parse(text);

            const version = info.version || 1;

            if (info.spriteModeMap) {
                for (let key in info.spriteModeMap) {
                    const modeMap = info.spriteModeMap[key];

                    for (const mapKey in modeMap) spriteModeMap[key][mapKey] = modeMap[mapKey];
                }
            }

            if (info.textureMap) {
                const promises = [];
                for (let key in info.textureMap) {
                    const map = info.textureMap[key];

                    for (const textureKey in map) {
                        if (!textureMap[key]) textureMap[key] = {};

                        textureMap[key][textureKey] = map[textureKey];
                    }

                    promises.push(
                        zip
                            .file(`daudmod/${map.file}.png`)
                            .async("arraybuffer")
                            .then(ab => {
                                const arrayBufferView = new Uint8Array(ab);
                                const blob = new Blob([arrayBufferView], { type: "image/png" });
                                const urlCreator = window.URL || window.webkitURL;
                                const url = urlCreator.createObjectURL(blob);

                                textureMap[key].url = url;
                            })
                    );
                }
                Promise.all(promises).then(() => {
                    if (window.Game && window.Game.cache) {
                        textureCache.clear();
                        window.Game.cache.refreshSprites();
                        window.Game.reinitializeWorld();
                    }
                });
            }
        });
}

load();

// override settins from querystring values
const qs = new URLSearchParams(window.location.search);
if (qs.has("themeCustom")) Settings.themeCustom = qs.get("themeCustom");
if (qs.has("leaderboardEnabled")) Settings.leaderboardEnabled = qs.get("leaderboardEnabled") == "true";
if (qs.has("hudEnabled")) Settings.hudEnabled = qs.get("hudEnabled") == "true";
if (qs.has("namesEnabled")) Settings.namesEnabled = qs.get("namesEnabled") == "true";
if (qs.has("bandwidth")) Settings.bandwidth = Number(qs.get("bandwidth"));

if (Settings.themeCustom) {
    theme(Settings.themeCustom);
} else if (Settings.theme) {
    theme(Settings.theme);
} // no good way to reset to default :(

const gear = document.getElementById("gear");
document.getElementById("settings").addEventListener("click", () => {
    gear.classList.remove("closed");
});

document.getElementById("settingsCancel").addEventListener("click", () => {
    gear.classList.add("closed");
});

document.getElementById("settingsSave").addEventListener("click", () => {
    save();
    load();
    gear.classList.add("closed");
});

document.getElementById("settingsReset").addEventListener("click", () => {
    reset();
    window.location.reload();
});

let minimapChanged = false;
window.addEventListener("keydown", function(e) {
    if (e.keyCode == 77 && !minimapChanged) {
        Settings.displayMinimap = !Settings.displayMinimap;
        minimapChanged = true;
    }
});

window.addEventListener("keyup", function(e) {
    if (e.keyCode == 77) minimapChanged = false;
});
