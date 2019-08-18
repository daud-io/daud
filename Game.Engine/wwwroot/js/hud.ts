import { Settings } from "./settings";

const hudh = document.getElementById("hud");

// for lag measurement system
var pingValues = [];

// lag measurement system settings
const pingValuesLength = 3;
const lagSysSet = {
    mult: 0.3,
    expo: 0.6,
    base: 2.7
};

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
        
        // calculate lag level
        if (this._latency !== 0) {
            if (typeof(pingValues[0]) == "undefined") {
                for (var i = 0; i < pingValuesLength; i++) {
                    pingValues.push(this._latency);
                }
            } else {
                pingValues.shift();
                pingValues.push(this._latency);
            }
            var pingSum = 0;
            for (var i = 0; i < pingValues.length; i++) {
                pingSum += pingValues[i];
            }
            var pingMean = pingSum / pingValues.length,
                pingDevSum = 0;
            for (var i = 0; i < pingValues.length; i++) {
                pingDevSum += Math.abs(pingValues[i] - pingMean);
            }
            var pingDevMean = pingDevSum / pingValues.length,
                lagLevel = Math.round((pingDevMean + lagSysSet.base) * Math.pow(pingMean, lagSysSet.expo) * lagSysSet.mult),
                lagLevelDes = lagLevelDescription(lagLevel);
        }

        hudh.innerHTML = `fps: ${this.framesPerSecond || 0} - \
                          players: ${this.playerCount || 0} - \
                          spectators: ${this.spectatorCount || 0} - \
                          ping: ${Math.floor(this._latency || 0)} - \
                          lag level: ${lagLevel || 0} (${lagLevelDes})`;
        hudh.style.fontFamily = Settings.font;

        if (this.playerCount > 0)
            window.document.title = `Daud.io (${this.playerCount})`;
        else
            window.document.title = `Daud.io`;
    }
}

function lagLevelDescription(lagLevel) {
    if (typeof(lagLevel) !== "undefined") {
        if (lagLevel <= 15) {
            return "very low";
        } else if (lagLevel <= 40) {
            return "low";
        } else if (lagLevel <= 100) {
            return "medium";
        } else if (lagLevel <= 150) {
            return "high";
        } else if (lagLevel <= 250) {
            return "very high";
        } else {
            return "extreme";
        }
    }
}