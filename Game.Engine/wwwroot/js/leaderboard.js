import { sprites } from "./renderer";
import { Settings } from "./settings";

var record = document.getElementById("record");
var leaderboard = document.getElementById("leaderboard");
var leaderboardLeft = document.getElementById("leaderboard-left");
var leaderboardCenter = document.getElementById("leaderboard-center");
export function clear() {
    leaderboard.innerHTML = "";
    leaderboardLeft.innerHTML = "";
    leaderboardCenter.innerHTML = "";
    leaderboardCenter.style.width = null;
    leaderboardCenter.style.height = null;
}
export class Leaderboard {
    constructor(canvas, context, settings = {}) {
        this.context = context;
        this.canvas = canvas;
        this.data = false;
    }

    setData(data, position) {
        this.data = data;
        if (this.data.Record) {
            record.style.fontFamily = Settings.font;
            record.innerHTML = `record: ${this.data.Record.Name || "Unknown Fleet"} - ${this.data.Record.Score}`;
        }
        if (this.data.Type == "FFA") {
            let out = "";
            for (let i = 0; i < this.data.Entries.length; i++) {
                const entry = this.data.Entries[i];
                const angle = Math.atan2(entry.Position.Y - position.Y, entry.Position.X - position.X);

                out +=
                    `<tr>` +
                    `<td style="width:28px;height:28px;background:${entry.Color}"><img class="arrow" src="${require("../img/arrow.png")}" style="transform:rotate(${angle}rad)"></img></td>` +
                    `<td style="width:5px" class="blue">${entry.Token ? "✓" : ""}</td>` +
                    `<td class="name">${entry.Name || "Unknown Fleet"}</td>` +
                    `<td class="score">${entry.Score}</td>` +
                    `</tr>`;
            }
            leaderboard.innerHTML = `<tbody>${out}</tbody>`;
        } else if (this.data.Type == "Team") {
            let outL = "";
            let outR = "";
            let outC = "";
            for (let i = 0; i < this.data.Entries.length; i++) {
                const entry = this.data.Entries[i];
                const angle = Math.atan2(entry.Position.Y - position.Y, entry.Position.X - position.X);
                let str =
                    `<tr>` +
                    `<td style="width:28px;height:28px;background:${entry.Color}"><img class="arrow" src="${require("../img/arrow.png")}" style="transform:rotate(${angle}rad)"></img></td>` +
                    `<td style="width:5px" class="blue">${entry.Token ? "✓" : ""}</td>` +
                    `<td class="name">${entry.Name || "Unknown Fleet"}</td>` +
                    `<td class="score">${entry.Score}</td>` +
                    `</tr>`;
                if (i == 0 || i == 1) {
                    outC += str;
                } else if (entry.Color == "cyan") {
                    outL += str;
                } else {
                    outR += str;
                }
            }
            leaderboard.innerHTML = `<tbody>${outR}</tbody>`;
            leaderboardLeft.innerHTML = `<tbody>${outL}</tbody>`;
            leaderboardCenter.innerHTML = `<tbody>${outC}</tbody>`;
        } else if (this.data.Type == "CTF") {
            let outL = "";
            let outR = "";
            let redFlag = false;
            let cyanFlag = false;
            for (let i = 0; i < this.data.Entries.length; i++) {
                const entry = this.data.Entries[i];
                const angle = Math.atan2(entry.Position.Y - position.Y, entry.Position.X - position.X);
                let str =
                    `<tr>` +
                    `<td style="width:28px;height:28px;background:${entry.Color}"><img class="arrow" src="${require("../img/arrow.png")}" style="transform:rotate(${angle}rad)"></img></td>` +
                    `<td style="width:5px" class="blue">${entry.Token ? "✓" : ""}</td>` +
                    `<td class="name">${entry.Name || "Unknown Fleet"}</td>` +
                    `<td class="score">${entry.Score}</td>` +
                    `</tr>`;
                if (i == 0) {
                    cyanFlag = entry;
                } else if (i == 1) {
                    redFlag = entry;
                } else if (entry.Color == "cyan") {
                    outL += str;
                } else {
                    outR += str;
                }
            }

            const self = this;
            const findTeam = teamName => {
                for (let i = 0; i < self.data.Entries.length; i++) {
                    if (self.data.Entries[i].Name == teamName) return self.data.Entries[i];
                }
                return false;
            };

            const cyan = findTeam("cyan") || { Score: 0 };
            const red = findTeam("red") || { Score: 0 };

            const cyanScore = Math.min(cyan.Score, 5);
            const redScore = Math.min(red.Score, 5);

            const image = i => {
                return `<img class="overlap" src="${sprites[i].image.src}"></img>`;
            };
            const cyanAngle = Math.atan2(cyanFlag.Position.Y - position.Y, cyanFlag.Position.X - position.X);
            const redAngle = Math.atan2(redFlag.Position.Y - position.Y, redFlag.Position.X - position.X);
            leaderboard.innerHTML = `<tbody>${outR}</tbody>`;
            leaderboardLeft.innerHTML = `<tbody>${outL}</tbody>`;
            leaderboardCenter.style.width = "372px";
            leaderboardCenter.style.height = "83px";
            leaderboardCenter.innerHTML =
                `<tbody><tr>` +
                `<td class="flag"><img class="flag-arrow" src="${sprites["ctf_arrow_blue"].image.src}" style="transform:rotate(${cyanAngle}rad);right:-50px"></img></td>` +
                `<td style="width:300px;position:relative">` +
                image("ctf_score_stripes") +
                image(`ctf_score_left_${Math.min(cyanScore, 4)}`) +
                image(`ctf_score_right_${Math.min(redScore, 4)}`) +
                image(`ctf_score_final${cyanScore >= 5 ? "_blue" : redScore >= 5 ? "_red" : ""}`) +
                `</td>` +
                `<td class="flag"><img class="flag-arrow" src="${sprites["ctf_arrow_red"].image.src}" style="transform:rotate(${redAngle}rad);left:-50px"></img></td>` +
                `</tr></tbody>`;
            console.log("leaderboarded");
        }
    }
}
