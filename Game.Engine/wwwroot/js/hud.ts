import { Settings } from "./settings";

const hudh = document.getElementById("hud");
export class HUD {
    _latency: number;
    framesPerSecond: number;
    playerCount: number;
    spectatorCount: number;
    set latency(l) {
        this._latency = l;
        this.update();
    }
    update() {
        if (Settings.hudEnabled) hudh.style.visibility = "visible";
        else hudh.style.visibility = "hidden";

        /* hudh.innerHTML = `fps: ${this.framesPerSecond || 0} - \
                          players: ${this.playerCount || 0} - \
                          spectators: ${this.spectatorCount || 0} - \
                          ping: ${Math.floor(this._latency || 0)}`;*/
        hudh.style.fontFamily = Settings.font;

        if (this.playerCount > 0)
            window.document.title = `SPACEONE.io (${this.playerCount})`;
        else
            window.document.title = `SPACEONE.io`;
    }
}
