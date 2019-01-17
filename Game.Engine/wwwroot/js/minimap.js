import { Settings } from "./settings";

const colors = {
    red: "#ff0000",
    pink: "#ffc0cb",
    orange: "#ffa500",
    yellow: "#ffff00",
    cyan: "#00ffff",
    green: "#00ff00"
};

const minimap = document.getElementById("minimap");
const minimapCtx = minimap.getContext("2d");

export class Minimap {
    update(data, worldSize, fleetID) {
        this.worldSize = worldSize;
        const startIndex = data.Type == "Team" || data.Type == "CTF" ? 2 : 0;
        minimapCtx.clearRect(0, 0, minimap.width, minimap.height);
        for (let i = startIndex; i < data.Entries.length; i++) {
            const entry = data.Entries[i];
            var entryIsSelf = entry.FleetID == fleetID;
            this.drawMinimap(entry.Position.X, entry.Position.Y, colors[entry.Color], entryIsSelf);
        }
    }
    drawMinimap(x, y, color, self) {
        var minimapX = ((x + this.worldSize) / 2 / this.worldSize) * minimap.width - 2;
        var minimapY = ((y + this.worldSize) / 2 / this.worldSize) * minimap.height - 2;

        if (!self) {
            minimapCtx.fillStyle = color;
            minimapCtx.fillRect(minimapX, minimapY, 4, 4);
        } else {
            minimapCtx.fillStyle = "white";
            minimapCtx.fillRect(minimapX - 1, minimapY - 1, 6, 6);
        }
    }
}
