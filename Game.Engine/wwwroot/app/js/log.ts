import { Settings } from "./settings";
import { escapeHtml } from "./leaderboard";
const log = document.getElementById("log");
const bigLog = document.getElementById("bigLog");
const scoreCon = document.getElementById("plusScoreContainer");
const comboMsg = document.getElementById("comboMessage");

export class Log {
    data: Array<{ time: Date; entry: any }>;
    lastDisplay: any;
    constructor() {
        this.data = [];
        this.lastDisplay = false;
    }

    addEntry(entry) {
        this.data.push({ time: new Date(), entry });
        while (this.data.length > Settings.logLength) this.data.shift();

        this.lastDisplay = performance.now();

        let out = "<table>";

        for (const slot of this.data) {
            out += "<tr>" + `<td><b style="color:gray">${slot.time.toLocaleTimeString()}</b></td>` + `<td>${slot.entry.text}</td>`;

            if (slot.entry.extraData && slot.entry.extraData.ping) out += `<td><b style="color:gray">${slot.entry.extraData.ping.you}ms/${slot.entry.extraData.ping.them}ms</b></td>`;
            else out += "<td></td>";

            if (slot.entry.extraData && slot.entry.extraData.stats && slot.entry.extraData.stats.deaths > 0)
                out += `<td><b style="color:gray">k/d: ${slot.entry.extraData.stats.kills / slot.entry.extraData.stats.deaths}</b></td>`;
            else out += "<td></td>";

            out += "</tr>";
        }

        out += "</table>";

        log.innerHTML = out;

        const lastData = this.data[this.data.length - 1].entry;
        /*
        var lastDataArr = lastData.split(" - ");

        var score = lastDataArr[1];
        lastData = lastDataArr.join(" - ");
        if (score[0] === "+") {
            scoreCon.insertAdjacentHTML("beforeend", "<div class='plusScore'>" + score + "</div>");
        }*/

        let lastMsg;
        if (Settings.bigKillMessage) {
            if (lastData.type == "kill") {
                lastMsg = "<span style='color:#00ff00'>[&nbsp;</span>" + escapeHtml(lastData.text) + "<span style='color:#00ff00'>&nbsp;]</span>";
                scoreCon.insertAdjacentHTML("beforeend", "<div class='plusScore'>+" + lastData.pointsDelta + "</div>");
            } else if (lastData.type == "killed") {
                lastMsg = "<span style='color:#ff0000'>[&nbsp;</span>" + escapeHtml(lastData.text) + "<span style='color:#ff0000'>&nbsp;]</span>";
                deathStats(lastData);
            } else {
                if (lastData.type === "universeDeath") {
                    deathStats(lastData);
                }
                return;
            }
            bigLog.innerHTML = lastMsg;
        }

        if (lastData.extraData.combo !== undefined && lastData.extraData.combo.text !== "") {
            comboMsg.innerHTML = lastData.extraData.combo.text + " +" + lastData.extraData.combo.score;
        }

        /*
		scoreCon.insertAdjacentHTML("beforeend", "<div class='plusScore'>" + lastData.pointsDelta + "</div>");
		comboMsg.innerHTML = lastData.extraData.combo;*/
    }

    check() {
        const time = performance.now() - this.lastDisplay;
        if (time > 6000) {
            log.innerHTML = "";
        }

        if (time > 3000) {
            bigLog.innerHTML = "";
        }

        if (time > 2000) {
            comboMsg.innerHTML = "";
        }
    }
}

function deathStats(lastData) {
    document.getElementById("deathScreen").style.visibility = "visible";
    document.getElementById("deathScreenScore").innerHTML = lastData.extraData.score;
    document.getElementById("deathScreenKills").innerHTML = lastData.extraData.kills;
    const gameTimeInSeconds = Math.round(lastData.extraData.gameTime / 1000),
        gameTimeMinutes = Math.floor(gameTimeInSeconds / 60),
        gameTimeSeconds = gameTimeInSeconds - 60 * gameTimeMinutes;
    if (gameTimeMinutes === 0) {
        document.getElementById("deathScreenGameTime").innerHTML = gameTimeSeconds + "sec";
    } else {
        document.getElementById("deathScreenGameTime").innerHTML = gameTimeMinutes + "min " + gameTimeSeconds + "sec";
    }
    document.getElementById("deathScreenMaxKillStreak").innerHTML = lastData.extraData.maxCombo;
}
