import { Settings } from "./settings";

const hudh = document.getElementById("hud");
export class HUD {
    set latency(l) {
        this._latency = l;
        this.update();
    }
    update() {
        hudh.innerHTML = `fps: ${this.framesPerSecond || 0} - \
                          players: ${this.playerCount || 0} - \
                          spectators: ${this.spectatorCount || 0} - \
                          ping: ${Math.floor(this._latency || 0)}`;
        hudh.style.fontFamily = Settings.font;
    }
}
