import { Settings } from "./settings";
import { RenderedObject } from "./models/renderedObject";
import arrow from "../img/arrow.png";
const record = document.getElementById("record");
const leaderboard = document.getElementById("leaderboard");
const leaderboardLeft = document.getElementById("leaderboard-left");
const leaderboardCenter = document.getElementById("leaderboard-center");

export function clear() {
    leaderboard.innerHTML = "";
    leaderboardLeft.innerHTML = "";
    leaderboardCenter.innerHTML = "";
    leaderboardCenter.style.width = null;
    leaderboardCenter.style.height = null;
}

export function escapeHtml(str) {
    const div = document.createElement("div");
    div.appendChild(document.createTextNode(str));
    return div.innerHTML;
}
function getOut(entry, position, rank, entryIsSelf) {
    const angle = Math.atan2(entry.Position.Y - position.Y, entry.Position.X - position.X);
	
	if (rank === undefined) {
		rank = "";
	} else {
		rank += ".";
	}
	
	var begin;
	if (!entryIsSelf) {
		begin = `<tr>`;
	} else if (rank <= 10) {
		begin = `<tr style="background-color:rgba(255,255,255,0.1)">`;
	} else {
		begin = `<tr style="background-color:rgba(255,255,255,0.1);transform:translateY(7px)">`;
	}
	
    return (
        begin +
		`<td style="width:25px">${rank}</td>` +
        `<td style="width:28px;height:28px;background:${entry.Color}"><img class="arrow" src="${arrow}" style="transform:rotate(${angle}rad)"></img></td>` +
        `<td style="width:5px" class="blue">${entry.Token ? "✓" : ""}</td>` +
        `<td class="name">${escapeHtml(entry.Name) || "Unknown Fleet"}</td>` +
        `<td class="score">${entry.Score}</td>` +
        `</tr>`
    );
}
export class Leaderboard {
    update(data, position, fleetID) {
        if (Settings.leaderboardEnabled) {
            record.style.visibility = "visible";
            leaderboard.style.visibility = "visible";
            leaderboardLeft.style.visibility = "visible";
            leaderboardCenter.style.visibility = "visible";
        } else {
            record.style.visibility = "hidden";
            leaderboard.style.visibility = "hidden";
            leaderboardLeft.style.visibility = "hidden";
            leaderboardCenter.style.visibility = "hidden";
            return;
        }

        if (data.Record) {
            record.style.fontFamily = Settings.font;
            record.innerHTML = `record: ${escapeHtml(data.Record.Name) || "Unknown Fleet"} - ${data.Record.Score}`;
        }

        //Hide or show elements based on Arena.
        if (data.Type == "CTF") {
            document.getElementById("ctf_arena").classList.remove("hide");
        } else {
            document.getElementById("ctf_arena").classList.add("hide");
        }

        if (data.Type == "FFA") {
            let out = "";
            for (let i = 0; i < data.Entries.length; i++) {
				const entryIsSelf = data.Entries[i].FleetID == fleetID;
				if (i < 10 || entryIsSelf) {
					out += getOut(data.Entries[i], position, i + 1, entryIsSelf);
				}
            }
            leaderboard.innerHTML = `<tbody>${out}</tbody>`;
        } else if (data.Type == "Team") {
            let outL = "";
            let outR = "";
            let outC = "";

            data.Entries.forEach((entry, i) => {
                let str = getOut(entry, position);
                if (i == 0 || i == 1) {
                    outC += str;
                } else if (entry.Color == "cyan") {
                    outL += str;
                } else {
                    outR += str;
                }
            });

            leaderboard.innerHTML = `<tbody>${outR}</tbody>`;
            leaderboardLeft.innerHTML = `<tbody>${outL}</tbody>`;
            leaderboardCenter.innerHTML = `<tbody>${outC}</tbody>`;
        } else if (data.Type == "CTF") {
            let outL = "";
            let outR = "";
            let redFlag = null;
            let cyanFlag = null;

            data.Entries.forEach((entry, i) => {
                let str = getOut(entry, position);
                if (i == 0) {
                    cyanFlag = entry;
                } else if (i == 1) {
                    redFlag = entry;
                } else if (entry.Color == "cyan") {
                    outL += str;
                } else {
                    outR += str;
                }
            });

            const flagStatus = {
                cyan: data.Entries[0].ModeData.flagStatus,
                red: data.Entries[1].ModeData.flagStatus
            };

            const cyanFlagStatus = document.getElementById("ctf_cyan").getElementsByClassName("flag_status")[0];
            const redFlagStatus = document.getElementById("ctf_red").getElementsByClassName("flag_status")[0];

            if (flagStatus.cyan == "Home") {
                cyanFlagStatus.getElementsByClassName("home")[0].classList.remove("hide");
                cyanFlagStatus.getElementsByClassName("taken")[0].classList.add("hide");
            } else if (flagStatus.cyan == "Taken") {
                cyanFlagStatus.getElementsByClassName("home")[0].classList.add("hide");
                cyanFlagStatus.getElementsByClassName("taken")[0].classList.remove("hide");
            }

            if (flagStatus.red == "Home") {
                redFlagStatus.getElementsByClassName("home")[0].classList.remove("hide");
                redFlagStatus.getElementsByClassName("taken")[0].classList.add("hide");
            } else if (flagStatus.red == "Taken") {
                redFlagStatus.getElementsByClassName("home")[0].classList.add("hide");
                redFlagStatus.getElementsByClassName("taken")[0].classList.remove("hide");
            }

            const findTeam = teamName => {
                for (let i = 0; i < data.Entries.length; i++) {
                    if (data.Entries[i].Name == teamName) return data.Entries[i];
                }
                return false;
            };

            const cyan = findTeam("cyan") || { Score: 0 };
            const red = findTeam("red") || { Score: 0 };

            const cyanScore = Math.min(cyan.Score, 5);
            const redScore = Math.min(red.Score, 5);

            const image = textureName => {
                return `<img class="overlap" src="${RenderedObject.getTextureImage(textureName).src}"></img>`;
            };
            const cyanAngle = Math.atan2(cyanFlag.Position.Y - position.Y, cyanFlag.Position.X - position.X);
            const redAngle = Math.atan2(redFlag.Position.Y - position.Y, redFlag.Position.X - position.X);
            leaderboard.innerHTML = `<tbody>${outR}</tbody>`;
            leaderboardLeft.innerHTML = `<tbody>${outL}</tbody>`;
            leaderboardCenter.style.width = "372px";
            leaderboardCenter.style.height = "83px";
            leaderboardCenter.innerHTML =
                `<tbody><tr>` +
                `<td class="flag"><img class="flag-arrow" src="${RenderedObject.getTextureImage("ctf_arrow_blue").src}" style="transform:rotate(${cyanAngle}rad);right:-50px"></img></td>` +
                `<td style="width:300px;position:relative">` +
                image("ctf_score_stripes") +
                image(`ctf_score_left_${Math.min(cyanScore, 4)}`) +
                image(`ctf_score_right_${Math.min(redScore, 4)}`) +
                image(`ctf_score_final${cyanScore >= 5 ? "_blue" : redScore >= 5 ? "_red" : ""}`) +
                `</td>` +
                `<td class="flag"><img class="flag-arrow" src="${RenderedObject.getTextureImage("ctf_arrow_red").src}" style="transform:rotate(${redAngle}rad);left:-50px"></img></td>` +
                `</tr></tbody>`;
        }
    }
}
