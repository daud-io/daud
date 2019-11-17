import { fetch } from "whatwg-fetch";
import JSZip from "jszip";
import { textureMapRules } from "./models/textureMap";
import { spriteModeMapRules } from "./models/spriteModeMap";
import { textureCache } from "./models/textureCache";
import { Controls } from "./controls";
import { Connection } from "./connection";
import Cookies = require("js-cookie");
import sass = require("sass");
const Buffer = require("buffer").Buffer;
const textureMapRulesLen = textureMapRules[0].length;
const spriteModeMapRulesLen = spriteModeMapRules[0].length;

// in case your code is isomorphic
if (typeof window !== "undefined") (window as any).Buffer = Buffer;

import { queryProperties, parseScssIntoRules } from "./parser/parseTheme.js";

export const Settings = {
    theme: "",
    themeCustom: "",
    mouseScale: 1.0,
    font: "Exo 2",
    leaderboardEnabled: true,
    hudEnabled: true,
    namesEnabled: true,
    bandwidth: 100,
    showCooldown: true,
    logLength: 4,
    displayMinimap: true,
    bigKillMessage: true,
    showKeyboardHints: true,
    showOwnName: true,
    allowDarkblueShips: true,
    showHints: true,
    nameSize: 48,
    background: "on",
    mipmapping: true,
    updatesVersion: 0,
    mouseOneButton: 0
};
const themeSelector = document.getElementById("settingsThemeSelector") as HTMLInputElement;
const themeSelectorCustom = document.getElementById("settingsThemeSelectorCustom") as HTMLInputElement;
const mouseScale = document.getElementById("settingsMouseScale") as HTMLInputElement;
const leaderboardEnabled = document.getElementById("settingsLeaderboardEnabled") as HTMLInputElement;
const showHints = document.getElementById("settingsShowHints") as HTMLInputElement;
const namesEnabled = document.getElementById("settingsNamesEnabled") as HTMLInputElement;
const bandwidth = document.getElementById("settingsBandwidth") as HTMLInputElement;
const hudEnabled = document.getElementById("settingsHUDEnabled") as HTMLInputElement;
const showCooldown = document.getElementById("settingsShowCooldown") as HTMLInputElement;
const logLength = document.getElementById("settingsLog") as HTMLInputElement;
const displayMinimap = document.getElementById("settingsDisplayMinimap") as HTMLInputElement;
const mipmapping = document.getElementById("settingsMipMapping") as HTMLInputElement;
const bigKillMessage = document.getElementById("settingsBigKillMessage") as HTMLInputElement;
const showKeyboardHints = document.getElementById("settingsShowKeyboardHints") as HTMLInputElement;
const showOwnName = document.getElementById("settingsShowOwnName") as HTMLInputElement;
const allowDarkblueShips = document.getElementById("settingsAllowDarkblueShips") as HTMLInputElement;
const nameSize = document.getElementById("settingsNameSize") as HTMLInputElement;
const background = document.getElementById("settingsBackground") as HTMLInputElement;

function save() {
    const cookieOptions = { expires: 300 };
    let reload = false;

    if (Settings.theme != themeSelector.value) {
        Settings.theme = themeSelector.value;
        if (Settings.theme == "") reload = true;
        else theme(Settings.theme);
    }
    if (Settings.themeCustom != themeSelectorCustom.value) {
        Settings.themeCustom = themeSelectorCustom.value;
        theme(Settings.themeCustom);
    }

    if (Settings.mipmapping != mipmapping.checked) {
        Settings.mipmapping = mipmapping.checked;
        reload = true;
    }

    Settings.font = "Exo 2";
    Settings.mouseScale = Number(mouseScale.value);
    Settings.leaderboardEnabled = leaderboardEnabled.checked;
    Settings.showHints = showHints.checked;
    Settings.namesEnabled = namesEnabled.checked;
    Settings.bandwidth = Number(bandwidth.value);
    Settings.hudEnabled = hudEnabled.checked;
    Settings.showCooldown = showCooldown.checked;
    Settings.logLength = Number(logLength.value);
    Settings.displayMinimap = displayMinimap.checked;
    Settings.mipmapping = mipmapping.checked;
    Settings.bigKillMessage = bigKillMessage.checked;
    Settings.showKeyboardHints = showKeyboardHints.checked;
    Settings.showOwnName = showOwnName.checked;
    Settings.allowDarkblueShips = allowDarkblueShips.checked;
    Settings.nameSize = Number(nameSize.value);
    Settings.background = background.value;

    Cookies.set("settings", Settings, cookieOptions);

    keyboardHints();
    shipBlue();

    if (reload) window.location.reload();
}

function reset() {
    Cookies.remove("settings");
}

function load() {
    const savedSettings = JSON.parse(Cookies.get("settings") || "false");

    if (savedSettings) {
        // copying value by value because cookies can be old versions
        // any values NOT in the cookie will remain defined with the new defaults
        for (const key in savedSettings) Settings[key] = savedSettings[key];

        // retro upgrades
        if (Settings.theme == "3ds2agh4z76feci") Settings.theme = "xn4t5ce2916uxbx";
        if (Settings.theme == "516mkwof6m4d4tg") Settings.theme = "xn4t5ce2916uxbx";
    }

    themeSelector.value = Settings.theme;
    themeSelectorCustom.value = Settings.themeCustom || "";

    mouseScale.value = String(Settings.mouseScale);
    leaderboardEnabled.checked = Settings.leaderboardEnabled;
    showHints.checked = Settings.showHints;
    mipmapping.checked = Settings.mipmapping;
    namesEnabled.checked = Settings.namesEnabled;
    bandwidth.value = String(Settings.bandwidth);
    hudEnabled.checked = Settings.hudEnabled;
    showCooldown.checked = Settings.showCooldown;
    logLength.value = String(Settings.logLength);
    displayMinimap.checked = Settings.displayMinimap;
    bigKillMessage.checked = Settings.bigKillMessage;
    showKeyboardHints.checked = Settings.showKeyboardHints;
    showOwnName.checked = Settings.showOwnName;
    allowDarkblueShips.checked = Settings.allowDarkblueShips;
    nameSize.value = String(Settings.nameSize);
    background.value = Settings.background;
}
function oldTextureKeyEntToNew(key, entry) {
    let newEntList = [];
    if (key == "animationSpeed") {
        newEntList = [["animation-speed", entry]];
    } else if (key == "scale") {
        newEntList = [["size", parseFloat(entry) * 100 + "%"]];
    } else if (key == "tileSize") {
        newEntList = [["tile-size", entry]];
    } else if (key == "tileWidth") {
        newEntList = [["tile-width", entry]];
    } else if (key == "tileHeight") {
        newEntList = [["tile-height", entry]];
    } else if (key == "tileCount") {
        newEntList = [["tile-count", entry]];
    } else if (key == "imageWidth") {
        newEntList = [["image-width", entry]];
    } else if (key == "imageHeight") {
        newEntList = [["image-height", entry]];
    } else if (key == "tileSpaceHeight") {
        newEntList = [["tile-space-height", entry]];
    } else if (key == "tileSpaceWidth") {
        newEntList = [["tile-space-width", entry]];
    } else if (key == "offset") {
        newEntList = [
            ["offset-x", entry.x],
            ["offset-y", entry.y]
        ];
    } else if (key == "animationSpeed") {
        newEntList = [["animation-speed", entry]];
    } else {
        newEntList = [[key, entry]];
    }
    return newEntList;
}
function oldTextureEntryToNew(entry) {
    const newEnt = {};
    for (const mapKey in entry) {
        const toMake = oldTextureKeyEntToNew(mapKey, entry[mapKey]);
        for (let i = 0; i < toMake.length; i++) {
            newEnt[toMake[i][0]] = [JSON.stringify(toMake[i][1])];
            if (toMake[i][0] == "size") {
                newEnt[toMake[i][0]] = [toMake[i][1]];
            }
        }
    }
    return newEnt;
}

const debug = true;

async function theme(v): Promise<any> {
    document.getElementById("theme-styles").innerHTML = "";
    if (v) v = v.toLowerCase();
    const link = `https://dl.dropboxusercontent.com/s/${v}/daudmod.zip`;
    const zip = await fetch(link)
        .then(response => response.blob())
        .then(JSZip.loadAsync);
    const baseFiles = [];
    zip.folder("daudmod").forEach(function(relativePath, file) {
        if (!file.dir) {
            baseFiles.push(file.name);
        }
    });
    const hasInfo = baseFiles.includes("daudmod/info.json");
    const hasSpriteModeMap = baseFiles.includes("daudmod/spriteModeMap.scss");
    const hasTextureMap = baseFiles.includes("daudmod/textureMap.scss");
    const hasStylesScss = baseFiles.includes("daudmod/styles.scss");
    if (hasStylesScss && hasSpriteModeMap && hasStylesScss) {
        //new theme
        zip.file("daudmod/spriteModeMap.scss")
            .async("string")
            .then(text => {
                zip.file("daudmod/textureMap.scss")
                    .async("string")
                    .then(text2 => {
                        zip.file("daudmod/styles.scss")
                            .async("string")
                            .then(text3 => {
                                textureMapRules[0] = textureMapRules[0].slice(0, textureMapRulesLen);
                                spriteModeMapRules[0] = spriteModeMapRules[0].slice(0, spriteModeMapRulesLen);

                                if (text) {
                                    const spriteModeMapR = parseScssIntoRules(text);
                                    spriteModeMapRules[0] = spriteModeMapRules[0].concat(spriteModeMapR);
                                }

                                if (text2) {
                                    const textureMapR = parseScssIntoRules(text2);

                                    const promises = [];
                                    for (const entry of textureMapR) {
                                        (function(ent) {
                                            if (ent.obj.file) {
                                                const file = JSON.parse(ent.obj.file[0]) + "";

                                                promises.push(
                                                    zip
                                                        .file(`daudmod/${file}.png`)
                                                        .async("arraybuffer")
                                                        .then(ab => {
                                                            const key = ent.selector + "";
                                                            const arrayBufferView = new Uint8Array(ab);
                                                            const blob = new Blob([arrayBufferView], { type: "image/png" });
                                                            const urlCreator = window.URL;
                                                            const url = urlCreator.createObjectURL(blob);

                                                            if (key == "shield") {
                                                                console.log("breakpoint");
                                                                (ent.obj as any).flag = ["true"];
                                                            }

                                                            if (debug) console.log(`textureMap.${key}.url: set to blob for file ${file}`);
                                                            ent.obj.url = [`"${url}"`];
                                                        })
                                                );
                                            }

                                            if (ent.obj.emitter) {
                                                const emitter = JSON.parse(ent.obj.emitter[0]) + "";

                                                promises.push(
                                                    zip
                                                        .file(`daudmod/${emitter}.json`)
                                                        .async("string")
                                                        .then(json => {
                                                            ent.obj.emitter = [JSON.parse(json)];
                                                        })
                                                );
                                            }
                                        })(entry);
                                    }
                                    textureMapRules[0] = textureMapRules[0].concat(textureMapR);

                                    if (text3) {
                                        try {
                                            const ab = sass.renderSync({ data: text3 }).css.toString("utf8");
                                            const imagePromises = [];
                                            let cleansed = ab;
                                            let images = ab.match(/url\("\.\/?(.*?\.png)"\)/g);
                                            images = images ? images : [];
                                            const fixed = [];
                                            const fixedMap = [];
                                            const replacePairs = [];
                                            for (let imagen = 0; imagen < images.length; imagen++) {
                                                const imocc = images[imagen];
                                                const m = /url\("\.\/?(.*?\.png)"\)/g.exec(imocc);
                                                const imgurl = m[1] + "";
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
                                                                    const urlCreator = window.URL;
                                                                    const url = urlCreator.createObjectURL(blob);
                                                                    fixedMap[fixed.indexOf(loo)] = url;

                                                                    if (debug) console.log(`theme css image ${loo}: set to blob url ${url}`);
                                                                });
                                                        })(imgurl)
                                                    );
                                                }
                                            }
                                            Promise.all(imagePromises).then(() => {
                                                for (let k = 0; k < replacePairs.length; k++) {
                                                    cleansed = cleansed.replace(replacePairs[k][0], "url(" + fixedMap[replacePairs[k][1]] + ")");
                                                }
                                                const blob = new Blob([cleansed], { type: "text/css" });
                                                const urlCreator = window.URL;
                                                const url = urlCreator.createObjectURL(blob);

                                                const link = document.createElement("link");
                                                link.setAttribute("rel", "stylesheet");
                                                link.setAttribute("type", "text/css");
                                                link.setAttribute("href", url);
                                                const m = document.getElementById("theme-styles").children.length;
                                                for (let l = 0; l < m; l++) {
                                                    document.getElementById("theme-styles").removeChild(document.getElementById("theme-styles").children[l]);
                                                }
                                                document.getElementById("theme-styles").appendChild(link);
                                            });
                                        } catch (e) {
                                            console.log("ERROR IN CUSTOM THEME STYLES (STEP 3):", e);
                                        }
                                    }

                                    Promise.all(promises).then(() => {
                                        if (window.Game && window.Game.cache) {
                                            if (debug) console.log(`theme loading complete`);
                                            (window as any).textureMapRules = textureMapRules;
                                            (window as any).spriteModeMapRules = spriteModeMapRules;
                                            textureCache.clear();
                                            window.Game.cache.refreshSprites();
                                            window.Game.reinitializeWorld();
                                            //Controls.addSecretShips(window.discordData);
                                        }
                                    });
                                }
                            });
                    });
            });
    } else if (hasInfo) {
        //old theme
        zip.file("daudmod/info.json")
            .async("string")
            .then(text => {
                const promises = [];
                const info = JSON.parse(text);

                const version = info.version || 1;
                textureMapRules[0] = textureMapRules[0].slice(0, textureMapRulesLen);
                spriteModeMapRules[0] = spriteModeMapRules[0].slice(0, spriteModeMapRulesLen);

                let changesBk = false;
                if (info.spriteModeMap) {
                    changesBk = changesBk || info.spriteModeMap.hasOwnProperty("bg");
                }
                if (info.textureMap) {
                    changesBk = changesBk || info.textureMap.hasOwnProperty("bg");
                }
                if (changesBk) {
                    //spriteModeMap["bg"].additionalLayers = [];
                }

                if (info.spriteModeMap) {
                    for (const key in info.spriteModeMap) {
                        const modeMap = info.spriteModeMap[key];

                        const hasModes = modeMap["modes"] !== undefined && modeMap["modes"] !== null;
                        const baseSelector = key.split("_").join(".") + "";
                        const baseRuleObj = {};
                        for (const mapKey in modeMap) {
                            if (mapKey != "modes") {
                                baseRuleObj[mapKey] = [JSON.stringify(modeMap[mapKey])];
                            }
                        }
                        const moreRules = [];
                        if (modeMap.modes) {
                            if (modeMap.modes["default"]) {
                                baseRuleObj["textures"] = [`"${modeMap.modes["default"][0]}"`];
                            }
                            for (const mapKey in modeMap.modes) {
                                if (mapKey != "default") {
                                    moreRules.push({ selector: baseSelector + "." + mapKey, obj: { textures: ["inherit", `"${modeMap.modes[mapKey][0]}"`] } });
                                }
                            }
                            const spriteModeMapR = [{ selector: baseSelector, obj: baseRuleObj }].concat(moreRules);
                            spriteModeMapRules[0] = spriteModeMapRules[0].concat(spriteModeMapR);
                        }
                    }
                }
                if (info.textureMap) {
                    const textureMapR = [];
                    for (const key in info.textureMap) {
                        const map = info.textureMap[key];
                        textureMapR.push({ selector: key + "", obj: oldTextureEntryToNew(map) });
                    }

                    for (const entry of textureMapR) {
                        (function(ent) {
                            if (ent.obj.file) {
                                let file = "";
                                try {
                                    file = JSON.parse(ent.obj.file[0]) + "";
                                } catch (e) {
                                    console.log("fdhsaush", e, ent.obj.file);
                                }

                                promises.push(
                                    zip
                                        .file(`daudmod/${file}.png`)
                                        .async("arraybuffer")
                                        .then(ab => {
                                            const key = ent.selector + "";
                                            const arrayBufferView = new Uint8Array(ab);
                                            const blob = new Blob([arrayBufferView], { type: "image/png" });
                                            const urlCreator = window.URL;
                                            const url = urlCreator.createObjectURL(blob);

                                            if (key == "shield") {
                                                console.log("breakpoint");
                                                (ent.obj as any).flag = ["true"];
                                            }

                                            if (debug) console.log(`OLD textureMap.${key}.url: set to blob for file ${file},BLOB:${url}`);
                                            ent.obj.url = [url];
                                        })
                                );
                            }
                        })(entry);
                    }
                    textureMapRules[0] = textureMapRules[0].concat(textureMapR);
                }
                if (info.styles) {
                    const m = document.getElementById("theme-styles").children.length;
                    for (let l = 0; l < m; l++) {
                        document.getElementById("theme-styles").removeChild(document.getElementById("theme-styles").children[l]);
                    }
                    for (let i = 0; i < info.styles.length; i++) {
                        const css = info.styles[i];

                        promises.push(
                            zip
                                .file(`daudmod/${css}`)
                                .async("string")
                                .then(ab => {
                                    const imagePromises = [];
                                    let cleansed = ab;
                                    let images = ab.match(/url\("\.\/?(.*?\.png)"\)/g);
                                    images = images ? images : [];
                                    const fixed = [];
                                    const fixedMap = [];
                                    const replacePairs = [];
                                    for (let imagen = 0; imagen < images.length; imagen++) {
                                        const imocc = images[imagen];
                                        const m = /url\("\.\/?(.*?\.png)"\)/g.exec(imocc);
                                        const imgurl = m[1] + "";
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
                                                            const urlCreator = window.URL;
                                                            const url = urlCreator.createObjectURL(blob);
                                                            fixedMap[fixed.indexOf(loo)] = url;

                                                            if (debug) console.log(`theme css image ${loo}: set to blob url ${url}`);
                                                        });
                                                })(imgurl)
                                            );
                                        }
                                    }
                                    Promise.all(imagePromises).then(() => {
                                        for (let k = 0; k < replacePairs.length; k++) {
                                            cleansed = cleansed.replace(replacePairs[k][0], "url(" + fixedMap[replacePairs[k][1]] + ")");
                                        }
                                        const blob = new Blob([cleansed], { type: "text/css" });
                                        const urlCreator = window.URL;
                                        const url = urlCreator.createObjectURL(blob);

                                        const link = document.createElement("link");
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
                        (window as any).textureMapRules = textureMapRules;
                        (window as any).spriteModeMapRules = spriteModeMapRules;
                        if (debug) console.log(`old theme loading complete`);
                        textureCache.clear();
                        window.Game.cache.refreshSprites();
                        window.Game.reinitializeWorld();
                        //Controls.addSecretShips(window.discordData);
                    }
                });
            });
    }
    return Promise.resolve(undefined);
}
(window as any).getTextureMapRules = function() {
    return textureMapRules;
};
(window as any).getModeMapRules = function() {
    return spriteModeMapRules;
};
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
    if (e.keyCode == 77 && !minimapChanged && (document.body.classList.contains("alive") || document.body.classList.contains("spectating"))) {
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

shipBlue();

function shipBlue() {
    if (!Settings.allowDarkblueShips) {
        //spriteModeMap.ship_blue.modes.default = ["ship_cyan"];
        //spriteModeMap.ship_blue.modes.boost = ["thruster_cyan"];
        //spriteModeMap.bullet_blue.modes.default = ["bullet_cyan"];
    } else {
        //spriteModeMap.ship_blue.modes.default = ["ship_blue"];
        //spriteModeMap.ship_blue.modes.boost = ["thruster_blue"];
        //spriteModeMap.bullet_blue.modes.default = ["bullet_blue"];
    }
}
