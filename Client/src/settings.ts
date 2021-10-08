import { Cookies } from "./cookies";
import * as bus from "./bus";

bus.on("pageReady", () => Settings.initialize());

export class Settings {
    static theme: string = "";
    static bandwidth: number = 100;
    static logLength: number = 4;
    static showHints: boolean = true;
    static nameSize: number = 48;
    static graphics: string = "low";

    private static themeSelectorEL: HTMLSelectElement;
    private static showHintsEL: HTMLInputElement;
    private static bandwidthEL: HTMLInputElement;
    private static logLengthEL: HTMLInputElement;
    private static nameSizeEL: HTMLInputElement;
    private static gearEL: HTMLElement;
    private static graphicsSelectorEL: HTMLSelectElement;

    static initialize() {
        this.graphicsSelectorEL = document.getElementById('settingsGraphics') as HTMLSelectElement;
        this.themeSelectorEL = document.getElementById("settingsTheme") as HTMLSelectElement;
        this.showHintsEL = document.getElementById("settingsShowHints") as HTMLInputElement;
        this.bandwidthEL = document.getElementById("settingsBandwidth") as HTMLInputElement;
        this.logLengthEL = document.getElementById("settingsLog") as HTMLInputElement;
        this.nameSizeEL = document.getElementById("settingsNameSize") as HTMLInputElement;
        this.gearEL = document.getElementById("gear")!;

        this.graphicsSelectorEL.onchange = () => {
            Settings.graphics = this.graphicsSelectorEL.value;
            Settings.saveSettings();
        };

        async function themeChange() {
            Settings.theme = Settings.themeSelectorEL.value;
            Settings.saveSettings();
            bus.emit("themechange");
        }

        this.themeSelectorEL.onchange = themeChange;
        
        this.showHintsEL.onchange = () => {
            Settings.showHints = this.showHintsEL.checked;
            Settings.saveSettings();
        };
        this.bandwidthEL.onchange = () => {
            Settings.bandwidth = Number(this.bandwidthEL.value);
            Settings.saveSettings();
        };
        this.logLengthEL.onchange = () => {
            Settings.logLength = Number(this.logLengthEL.value);
            Settings.saveSettings();
        };
        this.nameSizeEL.onchange = () => {
            Settings.nameSize = Number(this.nameSizeEL.value);
            Settings.saveSettings();
        };

        document.getElementById("settings")!.addEventListener("click", () => {
            this.gearEL.classList.remove("closed");
        });
        document.getElementById("settingsClose")!.addEventListener("click", () => {
            this.gearEL.classList.add("closed");
        });

        Settings.loadSettings();
    }

    static saveSettings() {
        const json = JSON.stringify(
            {
                theme: Settings.theme,
                bandwidth: Settings.bandwidth,
                logLength: Settings.logLength,
                showHints: Settings.showHints,
                nameSize: Settings.nameSize
            });
            
        console.log('saving settings: ' + json);
        Cookies.set("settings", json, { expires: 300 });
        bus.emit("settings");
    }

    static loadSettings(): void {
        const savedSettings = JSON.parse(Cookies.get("settings") || "false");

        if (savedSettings) {
            // copying value by value because cookies can be old versions
            // any values NOT in the cookie will remain defined with the new defaults
            for (const key in savedSettings) {
                Settings[key] = savedSettings[key];
            }
        }

        switch (Settings.theme) {
            case "":
                Settings.theme = "original";
                break;
            case "/themes/daudmod.zip":
                Settings.theme = "original";
                break;
            case "/themes/retro.zip":
                Settings.theme = "retro";
                break;
        }

        this.themeSelectorEL.value = Settings.theme;
        this.showHintsEL.checked = Settings.showHints;
        this.bandwidthEL.value = String(Settings.bandwidth);
        this.logLengthEL.value = String(Settings.logLength);
        this.nameSizeEL.value = String(Settings.nameSize);

        console.log("settings loaded");
        bus.emit("settings");
    }
}

