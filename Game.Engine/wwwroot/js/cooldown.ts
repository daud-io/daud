import { Settings } from "./settings";
const progress = document.getElementById("cooldown") as HTMLProgressElement;
export class Cooldown {
    setCooldown(prog:number) {
        if (Settings.showCooldown) progress.style.visibility = "visible";
        else progress.style.visibility = "hidden";

        progress.value = prog;
    }
}
