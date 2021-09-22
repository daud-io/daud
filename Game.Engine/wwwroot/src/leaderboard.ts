import { LeaderboardType, LeaderboardEntry } from "./connection";
import { html, render, Hole } from "uhtml";
import * as bus from "./bus";
import { Vector2 } from "@babylonjs/core";
import { GameContainer } from "./gameContainer";

export class Leaderboard {
    recordName: string = "";
    recordScore: number = 0;

    record = document.getElementById("record")!;
    leaderboard = document.getElementById("leaderboard")!;
    leaderboardLeft = document.getElementById("leaderboard-left")!;
    leaderboardCenter = document.getElementById("leaderboard-center")!;
    container: GameContainer;

    constructor(container: GameContainer) {
        this.container = container;

        bus.on("worldjoin", () => {
            render(this.leaderboard, html``);
            render(this.leaderboardLeft, html``);
            render(this.leaderboardCenter, html``);
            this.leaderboardCenter.style.width = "";
            this.leaderboardCenter.style.height = "";
        });

        bus.on("leaderboard", (lb) => this.update(lb, container.cameraPosition, container.fleetID));
    }

    getOut(entry: LeaderboardEntry, position: Vector2 | null, rank?: number, entryIsSelf?: boolean) {
        const angle = position != null && entryIsSelf !== true ? -Math.atan2(entry.Position.y - position.y, entry.Position.x - position.x) : null;

        let color;
        if (entry.Color === "blue") {
            color = "#2255ff";
        } else {
            color = entry.Color;
        }

        const styles = entryIsSelf ? "background-color: rgba(255,255,255,0.1)" : "";
        return html` <tr style=${styles}>
            <td style="width:25px">${rank ? rank + "." : ""}</td>
            <td style=${`width:28px;height:28px;background:${color}`}>${angle ? html`<img class="arrow" src="/img/arrow.png" style=${`transform:rotate(${angle}rad)`}></img>` : ""}</td>
            <td style="width:5px" class="blue">${entry.Token ? "✓" : ""}</td>
            <td class="name">${entry.Name}</td>
            <td class="score">${entry.Score}</td>
        </tr>`;
    }

    update(data: LeaderboardType, position: Vector2, fleetID: number): void {
        if (data.Record && (this.recordName != data.Record.Name || this.recordScore != data.Record.Score)) {
            this.recordName = data.Record.Name;
            this.recordScore = data.Record.Score;
            this.record.innerText = `record: ${this.recordName} - ${this.recordScore}`;
        }

        //Hide or show elements based on Arena.
        if (data.Type == "CTF") {
            document.getElementById("ctf_arena")!.classList.remove("hide");
        } else {
            document.getElementById("ctf_arena")!.classList.add("hide");
        }

        if (data.Type == "FFA") {
            const out: Hole[] = [];
            for (let i = 0; i < data.Entries.length; i++) {
                const entryIsSelf = data.Entries[i].FleetID == fleetID;

                const extra = (data.Entries[i].ModeData.advance * 10).toFixed(1);

                if (i < 10 || entryIsSelf) {
                    out.push(this.getOut(data.Entries[i], position, i + 1, entryIsSelf));
                }
            }
            render(
                this.leaderboard,
                html`<tbody>
                    ${out}
                </tbody>`
            );
        } else if (data.Type == "Team") {
            const outL: Hole[] = [];
            const outR: Hole[] = [];
            const outC: Hole[] = [];

            data.Entries.forEach((entry, i) => {
                const entryIsSelf = data.Entries[i].FleetID == fleetID;
                if (i == 0 || i == 1) {
                    const template = this.getOut(entry, null, undefined, entryIsSelf);
                    outC.push(template);
                } else if (entry.Color == "cyan") {
                    const template = this.getOut(entry, position, undefined, entryIsSelf);
                    outL.push(template);
                } else {
                    const template = this.getOut(entry, position, undefined, entryIsSelf);
                    outR.push(template);
                }
            });

            render(
                this.leaderboard,
                html`<tbody>
                    ${outR}
                </tbody>`
            );
            render(
                this.leaderboardLeft,
                html`<tbody>
                    ${outL}
                </tbody>`
            );
            render(
                this.leaderboardCenter,
                html`<tbody>
                    ${outC}
                </tbody>`
            );
        } else if (data.Type == "CTF") {
            const outL: Hole[] = [];
            const outR: Hole[] = [];
            let redFlag: LeaderboardEntry;
            let cyanFlag: LeaderboardEntry;

            data.Entries.forEach((entry, i) => {
                const template = this.getOut(entry, position);
                if (i == 0) {
                    cyanFlag = entry;
                } else if (i == 1) {
                    redFlag = entry;
                } else if (entry.Color == "cyan") {
                    outL.push(template);
                } else {
                    outR.push(template);
                }
            });

            const flagStatus = {
                cyan: data.Entries[0].ModeData.flagStatus,
                red: data.Entries[1].ModeData.flagStatus,
            };

            const cyanFlagStatus = document.getElementById("ctf_cyan")!.getElementsByClassName("flag_status")[0];
            const redFlagStatus = document.getElementById("ctf_red")!.getElementsByClassName("flag_status")[0];

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

            const findTeam = (teamName: string) => {
                for (let i = 0; i < data.Entries.length; i++) {
                    if (data.Entries[i].Name == teamName) return data.Entries[i];
                }
                return false;
            };

            const cyan = findTeam("cyan") || { Score: 0 };
            const red = findTeam("red") || { Score: 0 };

            const cyanScore = Math.min(cyan.Score, 5);
            const redScore = Math.min(red.Score, 5);

            const cyanAngle = Math.atan2(cyanFlag!.Position.y - position.y, cyanFlag!.Position.x - position.x);
            const redAngle = Math.atan2(redFlag!.Position.y - position.y, redFlag!.Position.x - position.x);
            render(
                this.leaderboard,
                html`<tbody>
                    ${outR}
                </tbody>`
            );
            render(
                this.leaderboardLeft,
                html`<tbody>
                    ${outL}
                </tbody>`
            );
            this.leaderboardCenter.style.width = "372px";
            this.leaderboardCenter.style.height = "83px";

            const image = (textureName: string) => {
                return html`<img class="overlap" src="${this.container.loader.getTextureDefinition(textureName).url}"></img>`;
            };
            render(
                this.leaderboardCenter,
                html`<tbody>
                    <tr>
                        <td class="flag">
                            <img class="flag-arrow" src=${this.container.loader.getTextureDefinition("ctf_arrow_blue").url} style=${`transform:rotate(${cyanAngle}rad);right:-50px`}></img>
                        </td>
                        <td style="width:300px;position:rejative">
                            ${image("ctf_score_stripes")}
                            ${image(`ctf_score_left_${Math.min(cyanScore, 4)}`)}
                            ${image(`ctf_score_right_${Math.min(redScore, 4)}`)}
                            ${image(`ctf_score_final${cyanScore >= 5 ? "_blue" : redScore >= 5 ? "_red" : ""}`)}
                        </td>
                        <td class="flag">
                            <img class="flag-arrow" src=${this.container.loader.getTextureDefinition("ctf_arrow_red").url} style=${`transform:rotate(${redAngle}rad);left:-50px`}></img>
                        </td>
                    </tr>
                    </tbody>`
            );
        }
    }
}

