import { Settings } from "./settings";

const log = document.getElementById("log");
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
    }

    check() {
        const time = performance.now() - this.lastDisplay;
        if (time > 6000) {
            log.innerHTML = "";
        }
    }
}
