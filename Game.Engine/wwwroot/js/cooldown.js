import { Settings } from "./settings";
const progress = document.getElementById("cooldown");
export class Cooldown {
    setCooldown(data) {
        if (Settings.showCooldown) progress.style.visibility = "visible";
        else progress.style.visibility = "hidden";

        progress.value = data;
    }
}
