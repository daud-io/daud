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
    font: "Exo 2",
    leaderboardEnabled: true,
    displayMinimap: "always",
    hudEnabled: true,
    namesEnabled: true,
    bandwidth: 100,
    showCooldown: true,
    logLength: 4,
    displayMinimap: true,
    bigKillMessage: true,
    showKeyboardHints: true,
    showOwnName: true,
    showHints: true,
    nameSize: 48,
    background: "on",
    mipmapping: true,
    updatesVersion: 0
};

function save() {
    const cookieOptions = { expires: 300 };
    let reload = false;

    if (Settings.theme != document.getElementById("settingsThemeSelector").value) {
        Settings.theme = document.getElementById("settingsThemeSelector").value;
        if (Settings.theme == "") reload = true;
        else theme(Settings.theme);
    }
    if (Settings.themeCustom != document.getElementById("settingsThemeSelectorCustom").value) {
        Settings.themeCustom = document.getElementById("settingsThemeSelectorCustom").value;
        theme(Settings.themeCustom);
    }

    if (Settings.mipmapping != document.getElementById("settingsMipMapping").checked) {
        Settings.mipmapping = document.getElementById("settingsMipMapping").checked;
        reload = true;
    }

    Settings.font = "Exo 2";
    Settings.mouseScale = document.getElementById("settingsMouseScale").value;
    Settings.leaderboardEnabled = document.getElementById("settingsLeaderboardEnabled").checked;
    Settings.showHints = document.getElementById("settingsShowHints").checked;
    Settings.namesEnabled = document.getElementById("settingsNamesEnabled").checked;
    Settings.bandwidth = document.getElementById("settingsBandwidth").value;
    Settings.hudEnabled = document.getElementById("settingsHUDEnabled").checked;
    Settings.showCooldown = document.getElementById("settingsShowCooldown").checked;
    Settings.logLength = document.getElementById("settingsLog").value;
    Settings.displayMinimap = document.getElementById("settingsDisplayMinimap").checked;
    Settings.mipmapping = document.getElementById("settingsMipMapping").checked;
    Settings.bigKillMessage = document.getElementById("settingsBigKillMessage").checked;
    Settings.showKeyboardHints = document.getElementById("settingsShowKeyboardHints").checked;
    Settings.showOwnName = document.getElementById("settingsShowOwnName").checked;
    Settings.nameSize = Number(document.getElementById("settingsNameSize").value);
    Settings.background = document.getElementById("settingsBackground").value;

    Cookies.set("settings", Settings, cookieOptions);

    keyboardHints();

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

        // retro upgrades
        if (Settings.theme == "3ds2agh4z76feci") Settings.theme = "xn4t5ce2916uxbx";
        if (Settings.theme == "516mkwof6m4d4tg") Settings.theme = "xn4t5ce2916uxbx";
    }

    document.getElementById("settingsThemeSelector").value = Settings.theme;
    document.getElementById("settingsThemeSelectorCustom").value = Settings.themeCustom || "";

    document.getElementById("settingsMouseScale").value = Settings.mouseScale;
    document.getElementById("settingsLeaderboardEnabled").checked = Settings.leaderboardEnabled;
    document.getElementById("settingsShowHints").checked = Settings.showHints;
    document.getElementById("settingsMipMapping").checked = Settings.mipmapping;
    document.getElementById("settingsNamesEnabled").checked = Settings.namesEnabled;
    document.getElementById("settingsBandwidth").value = Settings.bandwidth;
    document.getElementById("settingsHUDEnabled").checked = Settings.hudEnabled;
    document.getElementById("settingsShowCooldown").checked = Settings.showCooldown;
    document.getElementById("settingsLog").value = Settings.logLength;
    document.getElementById("settingsDisplayMinimap").checked = Settings.displayMinimap;
    document.getElementById("settingsBigKillMessage").checked = Settings.bigKillMessage;
    document.getElementById("settingsShowKeyboardHints").checked = Settings.showKeyboardHints;
    document.getElementById("settingsShowOwnName").checked = Settings.showOwnName;
    document.getElementById("settingsNameSize").value = Settings.nameSize;
    document.getElementById("settingsBackground").value = Settings.background;
}

const debug = true;

async function theme(v) {
    document.getElementById("theme-styles").innerHTML = "";
    if (v) v = v.toLowerCase();
    const link = `https://dl.dropboxusercontent.com/s/${v}/daudmod.zip`;
    const zip = await fetch(link)
        .then(response => response.blob())
        .then(JSZip.loadAsync);
    zip.file("daudmod/info.json")
        .async("string")
        .then(text => {
            const info = JSON.parse(text);

            const version = info.version || 1;
            var changesBk = false;
            if (info.spriteModeMap) {
                changesBk = changesBk || info.spriteModeMap.hasOwnProperty("bg");
            }
            if (info.textureMap) {
                changesBk = changesBk || info.textureMap.hasOwnProperty("bg");
            }
            if (changesBk) {
                spriteModeMap["bg"].additionalLayers = [];
            }

            if (info.spriteModeMap) {
                for (let key in info.spriteModeMap) {
                    const modeMap = info.spriteModeMap[key];

                    if (!spriteModeMap.hasOwnProperty(key)) {
                        console.log(`[warning] theme attempted to define a non-existant sprite: ${key}`);
                        continue;
                    }
                    for (const mapKey in modeMap) {
                        if (mapKey != "modes") spriteModeMap[key][mapKey] = modeMap[mapKey];
                    }

                    if (modeMap.modes) for (const mapKey in modeMap.modes) spriteModeMap[key].modes[mapKey] = modeMap.modes[mapKey];
                }
            }

            if (info.textureMap) {
                const promises = [];
                for (let key in info.textureMap) {
                    const map = info.textureMap[key];

                    for (const textureKey in map) {
                        if (!textureMap[key]) {
                            if (debug) console.log(`creating texture: ${key}`);
                            textureMap[key] = {};
                        }

                        if (debug) console.log(`textureMap.${key}.${textureKey}: ${map[textureKey]}`);
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

                                if (key == "shield") {
                                    console.log("breakpoint");
                                    textureMap[key].flag = true;
                                }

                                if (debug) console.log(`textureMap.${key}.url: set to blob`);
                                textureMap[key].url = url;
                            })
                    );
                }
                if (info.styles) {
                    for (var i = 0; i < info.styles.length; i++) {
                        const css = info.styles[i];

                        promises.push(
                            zip
                                .file(`daudmod/${css}`)
                                .async("string")
                                .then(ab => {
                                    var imagePromises = [];
                                    var cleansed = ab;
                                    var images = ab.match(/url\("\.\/?(.*?\.png)"\)/g);
                                    var fixed = [];
                                    var fixedMap = [];
                                    var replacePairs = [];
                                    for (var imagen = 0; imagen < images.length; imagen++) {
                                        var imocc = images[imagen];
                                        var m = /url\("\.\/?(.*?\.png)"\)/g.exec(imocc);
                                        var imgurl = m[1] + "";
                                        if (debug) console.log(`theme css imagesss ${imgurl}`);
                                        if (fixed.indexOf(imgurl) > 0) {
                                            replacePairs.push([imocc, fixed.indexOf(imgurl)]);
                                        } else {
                                            fixed.push(imgurl);
                                            fixedMap.push("");
                                            replacePairs.push([imocc, fixed.indexOf(imgurl)]);
                                            imagePromises.push(
                                                (function(loo) {
                                                    return zip
                                                        .file(`daudmod/${imgurl}`)
                                                        .async("arraybuffer")
                                                        .then(ab => {
                                                            const arrayBufferView = new Uint8Array(ab);
                                                            const blob = new Blob([arrayBufferView], { type: "image/png" });
                                                            const urlCreator = window.URL || window.webkitURL;
                                                            const url = urlCreator.createObjectURL(blob);
                                                            fixedMap[fixed.indexOf(loo)] = url;

                                                            if (debug) console.log(`theme css image ${loo}: set to blob url ${url}`);
                                                        });
                                                })(imgurl)
                                            );
                                        }
                                    }
                                    Promise.all(imagePromises).then(() => {
                                        for (var k = 0; k < replacePairs.length; k++) {
                                            cleansed = cleansed.replace(replacePairs[k][0], "url(" + fixedMap[replacePairs[k][1]] + ")");
                                        }
                                        const blob = new Blob([cleansed], { type: "text/css" });
                                        const urlCreator = window.URL || window.webkitURL;
                                        const url = urlCreator.createObjectURL(blob);

                                        var link = document.createElement("link");
                                        link.setAttribute("rel", "stylesheet");
                                        link.setAttribute("type", "text/css");
                                        link.setAttribute("href", url);
                                        document.getElementById("theme-styles").appendChild(link);
                                    });
                                })
                        );
                    }
                }
                Promise.all(promises).then(() => {
                    if (window.Game && window.Game.cache) {
                        if (debug) console.log(`theme loading complete`);
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
    if ((e.keyCode == 77 && !minimapChanged && document.body.classList.contains("alive")) || document.body.classList.contains("spectating")) {
        Settings.displayMinimap = !Settings.displayMinimap;
        minimapChanged = true;
    }
});

window.addEventListener("keyup", function(e) {
    if (e.keyCode == 77) minimapChanged = false;
});

keyboardHints();

function keyboardHints() {
    if (Settings.showKeyboardHints) {
        document.getElementById("minimapTip").style.display = "block";
        document.getElementById("autofireContainer").style.display = "block";
    } else {
        document.getElementById("minimapTip").style.display = "none";
        document.getElementById("autofireContainer").style.display = "none";
    }
}
