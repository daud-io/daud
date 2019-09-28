import { Settings } from "./settings";
const progress = document.getElementById("cooldown");
const progressVal = document.getElementById("cooldownValue");
export class Cooldown {
    setCooldown(prog: number) {
        if (Settings.showCooldown) progress.style.visibility = "visible";
        else progress.style.visibility = "hidden";

        progressVal.style.width = prog/255*100 + "%";
    }
}
