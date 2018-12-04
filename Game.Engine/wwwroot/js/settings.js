import { sprites } from "./renderer";
import { img as background, setPattern } from "./background";
import Cookies from "js-cookie";
import JSZip from "jszip";

export var Settings = {
    theme: false,
    themeCustom: false,
    background: "slow",
    mouseScale: 1.0,
    font: "sans-serif",
    leaderboardEnabled: true,
    namesEnabled: true
};

function save() {
    var cookieOptions = { expires: 300 };

    Settings.theme = document.getElementById("settingsThemeSelector").value;
    Settings.themeCustom = document.getElementById("settingsThemeSelectorCustom").value

    Settings.background = document.getElementById("settingsBackground").value
    Settings.mouseScale = document.getElementById("settingsMouseScale").value;
    Settings.font = document.getElementById("settingsFont").value;
    Settings.leaderboardEnabled = document.getElementById("settingsLeaderboardEnabled").checked;
    Settings.namesEnabled = document.getElementById("settingsNamesEnabled").checked;

    Cookies.set("settings", Settings, cookieOptions)
}

function reset() {
    Cookies.remove("settings");
}

function load() {
    try {
        var savedSettings = Cookies.getJSON("settings");

        if (savedSettings)
            Settings = savedSettings;


        document.getElementById("settingsThemeSelector").value = Settings.theme;
        document.getElementById("settingsThemeSelectorCustom").value = Settings.themeCustom || "";

        document.getElementById("settingsBackground").value = Settings.background;
        document.getElementById("settingsMouseScale").value = Settings.mouseScale;
        document.getElementById("settingsFont").value = Settings.font;
        document.getElementById("settingsLeaderboardEnabled").checked = Settings.leaderboardEnabled;
        document.getElementById("settingsNamesEnabled").checked = Settings.namesEnabled;


        if (Settings.themeCustom) {
            theme(Settings.themeCustom);
        } else if (Settings.theme) {
            theme(Settings.theme);
        } // no good way to reset to default :(
    }
    catch
    {
        // maybe reset()? will make debugging difficult
    }
}

async function theme(v) {
    var link = "https://dl.dropboxusercontent.com/s/" + v + "/daudmod.zip";
    var zip = await fetch(link)
        .then(function(response) {
            return response.blob();
        })
        .then(JSZip.loadAsync);
    zip.file("daudmod/info.json")
        .async("string")
        .then(function(text) {
            var info = JSON.parse(text);
            info.files.forEach(element => {
                zip.file("daudmod/" + element[0] + ".png")
                    .async("arraybuffer")
                    .then(function(ab) {
                        var arrayBufferView = new Uint8Array(ab);
                        var blob = new Blob([arrayBufferView], { type: "image/jpeg" });
                        var urlCreator = window.URL || window.webkitURL;
                        var url = urlCreator.createObjectURL(blob);
                        if (element[0] == "bg") {
                            background.src = url;
                            background.onload = function() {
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

var gear = document.getElementById("gear");
document.getElementById("settings").addEventListener("click", function() {
    gear.classList.remove("closed");
});

document.getElementById("settingsCancel").addEventListener("click", function () {
    gear.classList.add("closed");
});

document.getElementById("settingsSave").addEventListener("click", function () {

    save();
    load();

    gear.classList.add("closed");
});

document.getElementById("settingsReset").addEventListener("click", function () {
    reset();
    window.location.reload();
});
