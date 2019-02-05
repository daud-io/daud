import { Settings } from "./settings";

const log = document.getElementById("log");
const bigLog = document.getElementById("bigLog");

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

			slot.entry.text = slot.entry.text.replace(/</g, "&lt;").replace(/&/g, "&amp;"); // fix XSS vulnerability
			
            out += `<span><b style="color:gray">${slot.time.toLocaleTimeString()}</b> ${slot.entry.text}</span><br>`;
        }

        log.innerHTML = out;

        if (Settings.bigKillMessage) {
            var lastData = this.data[this.data.length - 1].entry;
			lastData.text = lastData.text.replace(/</g, "&lt;"); // fix XSS vulnerability
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
