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

        var out = "";
        for (let i = 0; i < this.data.length; i++) {
            const slot = this.data[i];
            out += `<span><b style="color:gray">${slot.time.toLocaleTimeString()}</b> ${slot.entry}</span><br>`;
        }
        log.innerHTML = out;
		
		var lastData = this.data[this.data.length-1]["entry"];
		if (lastData.toLowerCase().indexOf("you killed") === 0) {
			lastData = "<span style='color:#00ff00'>" + lastData + "</span>";
		} else if (lastData.toLowerCase().indexOf("killed by") === 0) {
			lastData = "<span style='color:#ff0000'>" + lastData + "</span>";
		} else {
			return;
		}
		bigLog.innerHTML = lastData;
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
