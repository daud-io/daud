import { html, render } from "uhtml";
import * as bus from "./bus";
import { Settings } from "./settings";
const logElement = document.getElementById("log")!;
const bigLog = document.getElementById("bigLog")!;
const scoreCon = document.getElementById("plusScoreContainer")!;
const comboMsg = document.getElementById("comboMessage")!;

export function escapeHtml(str: string): string {
    const div = document.createElement("div");
    div.appendChild(document.createTextNode(str));
    return div.innerHTML;
}

type LogType = { type: string; text: string; pointsDelta: number; extraData: any };

const data: { time: Date; entry: LogType }[] = [];
let lastDisplay: number;
let logElHide: NodeJS.Timeout, comboMsgHide: NodeJS.Timeout, bigLogHide: NodeJS.Timeout;
export function addEntry(entry: LogType): void {
    data.push({ time: new Date(), entry });
    while (data.length > Settings.logLength) data.shift();

    lastDisplay = Date.now();

    const out = data.map(
        (slot) =>
            html`<tr>
                <td><b style="color:gray">${slot.time.toLocaleTimeString()}</b></td>
                <td>${slot.entry.text}</td>
                ${slot.entry.extraData?.ping ? html`<td><b style="color:gray">${`${slot.entry.extraData.ping.you}ms/${slot.entry.extraData.ping.them}ms`}</b></td>` : html`<td></td>`}
                ${slot.entry.extraData?.stats?.deaths > 0
                    ? html`<td><b style="color:gray">${`k/d: ${(slot.entry.extraData.stats.kills / slot.entry.extraData.stats.deaths).toFixed(2)}`}</b></td>`
                    : html`<td></td>`}
            </tr>`
    );

    logElement.style.visibility = "";
    render(
        logElement,
        html`<table>
            ${out}
        </table>`
    );
    clearTimeout(logElHide);
    logElHide = setTimeout(() => (logElement.style.visibility = "hidden"), 6000);

    let lastMsg: string;
    if (entry.type == "kill") {
        lastMsg = "<span style='color:#00ff00'>[&nbsp;</span>" + escapeHtml(entry.text) + "<span style='color:#00ff00'>&nbsp;]</span>";
        scoreCon.insertAdjacentHTML("beforeend", "<div class='plusScore'>+" + entry.pointsDelta + "</div>");
    } else if (entry.type == "killed") {
        lastMsg = "<span style='color:#ff0000'>[&nbsp;</span>" + escapeHtml(entry.text) + "<span style='color:#ff0000'>&nbsp;]</span>";
        deathStats(entry);
    } else if (entry.type == "announce") {
        lastMsg = "<span style='color:#00ff00'>[&nbsp;</span>" + escapeHtml(entry.text) + "<span style='color:#00ff00'>&nbsp;]</span>";
    } else {
        if (entry.type === "universeDeath") {
            deathStats(entry);
        }
        return;
    }
    bigLog.innerHTML = lastMsg;
    clearTimeout(bigLogHide);
    bigLogHide = setTimeout(() => (bigLog.innerHTML = ""), 3000);

    if (entry.extraData != null && entry.extraData.combo !== undefined && entry.extraData.combo.text !== "") {
        comboMsg.innerHTML = entry.extraData.combo.text + " +" + entry.extraData.combo.score;
        clearTimeout(comboMsgHide);
        comboMsgHide = setTimeout(() => (comboMsg.innerHTML = ""), 2000);
    }
}

function deathStats(lastData: LogType) {
    document.getElementById("deathScreen")!.style.display = "block";
    document.getElementById("deathScreenScore")!.innerHTML = lastData.extraData.score;
    document.getElementById("deathScreenKills")!.innerHTML = lastData.extraData.kills;
    const gameTimeInSeconds = Math.round(lastData.extraData.gameTime / 1000),
        gameTimeMinutes = Math.floor(gameTimeInSeconds / 60),
        gameTimeSeconds = gameTimeInSeconds - 60 * gameTimeMinutes;
    document.getElementById("deathScreenGameTime")!.innerHTML = (gameTimeMinutes === 0 ? "" : gameTimeMinutes + "min ") + gameTimeSeconds + "sec";
    document.getElementById("deathScreenMaxKillStreak")!.innerHTML = lastData.extraData.maxCombo;
}

window.addEventListener("keydown", ({ key }) => {
    if (key.toLowerCase() == "l") logElement.style.visibility = "";
});
window.addEventListener("keyup", ({ key }) => {
    if (key.toLowerCase() == "l") logElement.style.visibility = logElement.style.visibility = Date.now() - lastDisplay > 6000 ? "hidden" : "";
});
