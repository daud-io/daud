import { Settings } from "./settings";

var hudh = document.getElementById("hud");
export class HUD {
    update() {
        hudh.innerHTML =
            `fps: ${window.Game.Stats.framesPerSecond || 0}` +
            ` - players: ${window.Game.Stats.playerCount || 0}` +
            ` - spectators: ${window.Game.Stats.spectatorCount || 0}` +
            ` - ping: ${Math.floor(window.Game.primaryConnection.latency || 0)}`;
        hudh.style.fontFamily = Settings.font;
    }
}
