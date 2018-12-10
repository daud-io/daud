import { sprites } from "./renderer";
import { img as background, setPattern } from "./background";
import Cookies from "js-cookie";
import JSZip from "jszip";

export const Settings = {
    theme: false,
    themeCustom: false,
    background: "slow",
    mouseScale: 1.0,
    font: "sans-serif",
    leaderboardEnabled: true,
    hudEnabled: true,
    namesEnabled: true,
    bandwidth: 100,
    showHitboxes: false
};

function parseQuery(queryString) {
    const query = {};
    const pairs = (queryString[0] === "?" ? queryString.substr(1) : queryString).split("&");
    for (let i = 0; i < pairs.length; i++) {
        const pair = pairs[i].split("=");
        query[decodeURIComponent(pair[0])] = decodeURIComponent(pair[1] || "");
    }
    return query;
}

function save() {
    const cookieOptions = { expires: 300 };
    let reload = false;

    if (Settings.theme != document.getElementById("settingsThemeSelector").value) {
        Settings.theme = document.getElementById("settingsThemeSelector").value;
        reload = true;
    }
    if (Settings.themeCustom != document.getElementById("settingsThemeSelectorCustom").value) {
        Settings.themeCustom = document.getElementById("settingsThemeSelectorCustom").value;
        reload = true;
    }

    Settings.background = document.getElementById("settingsBackground").value;
    Settings.mouseScale = document.getElementById("settingsMouseScale").value;
    Settings.font = document.getElementById("settingsFont").value;
    Settings.leaderboardEnabled = document.getElementById("settingsLeaderboardEnabled").checked;
    Settings.namesEnabled = document.getElementById("settingsNamesEnabled").checked;
    Settings.bandwidth = document.getElementById("settingsBandwidth").value;
    Settings.hudEnabled = document.getElementById("settingsHUDEnabled").checked;
    Settings.showHitboxes = document.getElementById("settingsShowHitboxes").checked;

    Cookies.set("settings", Settings, cookieOptions);

    if (reload) window.location.reload();
}

function reset() {
    Cookies.remove("settings");
}

function load() {
    try {
        const savedSettings = Cookies.getJSON("settings");

        if (savedSettings) {
            // copying value by value because cookies can be old versions
            // any values NOT in the cookie will remain defined with the new defaults
            for (const key in savedSettings) Settings[key] = savedSettings[key];
        }

        document.getElementById("settingsThemeSelector").value = Settings.theme;
        document.getElementById("settingsThemeSelectorCustom").value = Settings.themeCustom || "";

        document.getElementById("settingsBackground").value = Settings.background;
        document.getElementById("settingsMouseScale").value = Settings.mouseScale;
        document.getElementById("settingsFont").value = Settings.font;
        document.getElementById("settingsLeaderboardEnabled").checked = Settings.leaderboardEnabled;
        document.getElementById("settingsNamesEnabled").checked = Settings.namesEnabled;
        document.getElementById("settingsBandwidth").value = Settings.bandwidth;
        document.getElementById("settingsHUDEnabled").checked = Settings.hudEnabled;
        document.getElementById("settingsShowHitboxes").checked = Settings.showHitboxes;
    } catch (e) {
        // maybe reset()? will make debugging difficult
    }
}

async function theme(v) {
    const link = `https://dl.dropboxusercontent.com/s/${v}/daudmod.zip`;
    const zip = await fetch(link)
        .then(response => response.blob())
        .then(JSZip.loadAsync);
    zip.file("daudmod/info.json")
        .async("string")
        .then(text => {
            const info = JSON.parse(text);
            info.files.forEach(element => {
                zip.file(`daudmod/${element[0]}.png`)
                    .async("arraybuffer")
                    .then(ab => {
                        const arrayBufferView = new Uint8Array(ab);
                        const blob = new Blob([arrayBufferView], { type: "image/jpeg" });
                        const urlCreator = window.URL || window.webkitURL;
                        const url = urlCreator.createObjectURL(blob);
                        if (element[0] == "bg") {
                            background.src = url;
                            background.onload = () => {
                                setPattern();
                            };
                        } else {
                            sprites[element[0]].image.src = url;
                            if (element[1]) {
                                sprites[element[0]].scale = element[1];
                                sprites[element[0]].scaleToSize = !element[0].startsWith("ship");
                            }
                        }
                    });
            });
        });
}

load();

const qs = parseQuery(window.location.search);
if (qs.themeCustom) Settings.themeCustom = qs.themeCustom;
if (qs.background) Settings.background = qs.background;
if (qs.leaderboardEnabled) Settings.leaderboardEnabled = qs.leaderboardEnabled == "true";
if (qs.hudEnabled) Settings.hudEnabled = qs.hudEnabled == "true";
if (qs.namesEnabled) Settings.namesEnabled = qs.namesEnabled == "true";
if (qs.bandwidth) Settings.bandwidth = Number(qs.bandwidth);

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
