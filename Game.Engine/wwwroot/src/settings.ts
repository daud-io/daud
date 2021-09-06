import Cookies from "js-cookie";
import bus from "./bus";

export const Settings = {
    theme: "",
    mouseScale: 1.0,
    bandwidth: 100,
    logLength: 4,
    showHints: true,
    nameSize: 48,
    background: true,
    latencyMode: "server",
    latencyOffset: 0
};

const themeSelector = document.getElementById("settingsTheme") as HTMLSelectElement;
const mouseScale = document.getElementById("settingsMouseScale") as HTMLInputElement;
const showHints = document.getElementById("settingsShowHints") as HTMLInputElement;
const bandwidth = document.getElementById("settingsBandwidth") as HTMLInputElement;
const logLength = document.getElementById("settingsLog") as HTMLInputElement;
const nameSize = document.getElementById("settingsNameSize") as HTMLInputElement;
const backgroundEl = document.getElementById("settingsShowBackground") as HTMLInputElement;
const latencyModeEl = document.getElementById("settingsLatencyMode") as HTMLInputElement;
const latencyOffsetEl = document.getElementById("settingsLatencyOffset") as HTMLInputElement;

loadSettings();

async function themeChange() {
    Settings.theme = themeSelector.value;
    save();
    bus.emit("themechange");
}
themeSelector.onchange = themeChange;

mouseScale.onchange = () => {
    Settings.mouseScale = Number(mouseScale.value);
    save();
};
showHints.onchange = () => {
    Settings.showHints = showHints.checked;
    save();
};
bandwidth.onchange = () => {
    Settings.bandwidth = Number(bandwidth.value);
    save();
};
logLength.onchange = () => {
    Settings.logLength = Number(logLength.value);
    save();
};
nameSize.onchange = () => {
    Settings.nameSize = Number(nameSize.value);
    save();
};
backgroundEl.onchange = () => {
    Settings.background = backgroundEl.checked;
    save();
};
latencyModeEl.onchange = () => {
    Settings.latencyMode = latencyModeEl.value;
    save();
}
latencyOffsetEl.onchange = () => {
    Settings.latencyOffset = Number(latencyOffsetEl.value);
    save();
}

function save() {
    bus.emit("settings");
    Cookies.set("settings", Settings, { expires: 300 });
}

export function loadSettings(): void {
    const savedSettings = JSON.parse(Cookies.get("settings") || "false");

    if (savedSettings) {
        // copying value by value because cookies can be old versions
        // any values NOT in the cookie will remain defined with the new defaults
        for (const key in savedSettings) {
            Settings[key] = savedSettings[key];
        }
    }

    switch (Settings.theme)
    {
        case "":
            Settings.theme = "bitty";
            break;
        case "/themes/daudmod.zip":
            Settings.theme = "original";
            break;
        case "/themes/retro.zip":
            Settings.theme = "retro";
            break;
    }

    themeSelector.value = Settings.theme;
    mouseScale.value = String(Settings.mouseScale);
    showHints.checked = Settings.showHints;
    bandwidth.value = String(Settings.bandwidth);
    logLength.value = String(Settings.logLength);
    nameSize.value = String(Settings.nameSize);
    backgroundEl.checked = Settings.background;
    latencyModeEl.value = Settings.latencyMode;
    latencyOffsetEl.value = Settings.latencyOffset.toString();
}

const gear = document.getElementById("gear")!;
document.getElementById("settings")!.addEventListener("click", () => {
    gear.classList.remove("closed");
});

document.getElementById("settingsClose")!.addEventListener("click", () => {
    gear.classList.add("closed");
});
