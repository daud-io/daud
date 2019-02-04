import { Settings } from "./settings";

const log = document.getElementById("log");
const bigLog = document.getElementById("bigLog");
const scoreCon = document.getElementById("plusScoreContainer");

export class Log {
    constructor() {
        this.data = [];
        this.lastDisplay = false;
    }

    addEntry(entry) {
        this.data.push({ time: new Date(), entry });
        while (this.data.length > Settings.logLength) this.data.shift();

        this.lastDisplay = performance.now();

        let out = "";

        for (const slot of this.data) {
            if (slot.entry.extraData)
                console.log(slot.entry.extraData);
            if (slot.entry.pointsDelta)
                console.log(slot.entry.pointsDelta);

            out += `<span><b style="color:gray">${slot.time.toLocaleTimeString()}</b> ${slot.entry.text}</span><br>`;
        }

        log.innerHTML = out;

        let lastData = this.data[this.data.length - 1].entry;
		/*
        var lastDataArr = lastData.split(" - ");

        var score = lastDataArr[1];
        lastData = lastDataArr.join(" - ");
        if (score[0] === "+") {
            scoreCon.insertAdjacentHTML("beforeend", "<div class='plusScore'>" + score + "</div>");
        }*/

        if (Settings.bigKillMessage) {
            if (lastData.type == "kill") {
                lastData = "<span style='color:#00ff00'>[&nbsp;</span>" + lastData.text + "<span style='color:#00ff00'>&nbsp;]</span>";
            } else if (lastData.type == "killed") {
                lastData = "<span style='color:#ff0000'>[&nbsp;</span>" + lastData.text + "<span style='color:#ff0000'>&nbsp;]</span>";
            } else {
                return;
            }
            bigLog.innerHTML = lastData;
        }
    }

    check() {
        const time = performance.now() - this.lastDisplay;
        if (time > 6000) {
            log.innerHTML = "";
        }

        const time2 = performance.now() - this.lastDisplay;
        if (time > 3000) {
            bigLog.innerHTML = "";
        }
    }
}
