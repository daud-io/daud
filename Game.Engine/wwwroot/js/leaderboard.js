import { sprites } from "./renderer";
import { Settings } from "./settings";

export class Leaderboard {
    constructor(canvas, context, settings = {}) {
        this.context = context;
        this.canvas = canvas;
        this.data = false;
    }

    setData(data) {
        this.data = data;
    }

    drawTeamLeaderboardAt(entries, relativeTo, leftEdge) {
        const ctx = this.context;
        const arrow = sprites["arrow"];
        entries.forEach(({ Name, Score, Color, Position }, i) => {
            var rowHeight = 28;
            ctx.fillStyle = "white";
            ctx.fillText(Name || "Unknown Fleet", leftEdge, rowHeight + i * rowHeight);
            ctx.fillText(Score, leftEdge + 200, rowHeight + i * rowHeight);
            ctx.fillStyle = Color;

            const x = leftEdge - rowHeight;
            const y = i * rowHeight + 10;

            ctx.fillRect(x, y, rowHeight, rowHeight);

            if (relativeTo && (Position.X != 0 && Position.Y != 0)) {
                const angle = Math.atan2(Position.Y - relativeTo.Y, Position.X - relativeTo.X);

                ctx.save();
                ctx.translate(x + rowHeight / 2, y + rowHeight / 2);
                const w = arrow.image.width;
                const h = arrow.image.height;
                ctx.rotate(angle);
                ctx.scale(arrow.scale, arrow.scale);
                ctx.drawImage(arrow.image, -w / 2, -h / 2, w, h);
                ctx.restore();
            }
        });
    };


    modeCTF(relativeTo) {
        const ctx = this.context;
        ctx.save();

        ctx.font = "12pt " + Settings.font;
        ctx.fillStyle = "white";
        ctx.textAlign = "left";

        const arrow = sprites["arrow"];

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

        const hudX = this.canvas.width / 2 - 310 / 2;
        const hudY = 0;
        const hudWidth = 300;
        const hudHeight = 100;

        const drawSprite = sprite => {
            if (sprites.hasOwnProperty(sprite)) ctx.drawImage(sprites[sprite].image, hudX, hudY, hudWidth, hudHeight);
        };

        drawSprite("ctf_score_stripes");
        drawSprite(`ctf_score_left_${Math.min(cyanScore, 4)}`);
        drawSprite(`ctf_score_right_${Math.min(redScore, 4)}`);

        if (cyanScore >= 5) drawSprite("ctf_score_final_blue");
        else if (redScore >= 5) drawSprite("ctf_score_final_red");
        else drawSprite("ctf_score_final");

        var teams = ['cyan', 'red'];

        var cyanFlag = false;
        var cyanTeam = [];
        var redFlag = false;
        var redTeam = [];

        this.data.Entries.forEach((entry, i) => {
            if (i == 0)
                cyanFlag = entry;
            else if (i == 1)
                redFlag = entry;
            else if (entry.Color == 'cyan')
                cyanTeam.push(entry);
            else
                redTeam.push(entry);
        });


        const drawFlagArrow = (flag, sprite, teamIndex) => {
            if (relativeTo) {
                const angle = Math.atan2(flag.Position.Y - relativeTo.Y, flag.Position.X - relativeTo.X);
                var arrow = sprites[sprite];
                var overlay = sprites['ctf_arrow_trans_flag'];

                ctx.save();
                const w = arrow.image.width;
                const h = arrow.image.height;

                if (teamIndex == 0)
                    ctx.translate(hudX, hudY + 40);
                else
                    ctx.translate(hudX + hudWidth, hudY + 40);

                ctx.save();
                ctx.rotate(angle);
                ctx.scale(arrow.scale, arrow.scale);
                ctx.drawImage(arrow.image, -w / 2, -h / 2, w, h);
                ctx.restore();

                //ctx.scale(overlay.scale, overlay.scale);
                //ctx.drawImage(overlay.image, -overlay.image.width / 2, -overlay.image.height / 2, overlay.image.width, overlay.image.height);

                ctx.restore();
            }
        };

        drawFlagArrow(redFlag, "ctf_arrow_red", 1);
        drawFlagArrow(cyanFlag, "ctf_arrow_blue", 0);
        this.drawTeamLeaderboardAt(cyanTeam, relativeTo, 60);
        this.drawTeamLeaderboardAt(redTeam, relativeTo, this.canvas.width - 60 - 200);

        ctx.restore();
    }

    modeTeam(relativeTo) {
        const ctx = this.context;
        ctx.save();

        ctx.font = "12pt " + Settings.font;
        ctx.fillStyle = "white";
        ctx.textAlign = "left";

        var cyanTeam = [];
        var redTeam = [];
        var teams = [];

        this.data.Entries.forEach((entry, i) => {
            if (i == 0)
                teams.push(entry);
            else if (i == 1)
                teams.push(entry);
            else if (entry.Color == 'cyan')
                cyanTeam.push(entry);
            else
                redTeam.push(entry);
        });

        this.drawTeamLeaderboardAt(teams, false, this.canvas.width / 2 - 100);

        this.drawTeamLeaderboardAt(cyanTeam, relativeTo, 60);
        this.drawTeamLeaderboardAt(redTeam, relativeTo, this.canvas.width - 60 - 200);

        ctx.restore();
    }

    modeStandard(relativeTo) {
        const ctx = this.context;
        ctx.save();
        ctx.font = "12pt " + Settings.font;
        ctx.fillStyle = "white";
        ctx.textAlign = "left";

        const width = 200;
        var rowHeight = 28;
        const margin = 15;

        const arrow = sprites["arrow"];

        for (let i = 0; i < this.data.Entries.length; i++) {
            const entry = this.data.Entries[i];

            if (entry.Token) {
                ctx.fillStyle = "aqua";
                ctx.fillText("✓", this.canvas.width - width, rowHeight + i * rowHeight);
            }
            var tokenWidth = entry.Token ? 15 : 0;
            ctx.fillStyle = "white";
            ctx.fillText(entry.Name || "Unknown Fleet", this.canvas.width - width + tokenWidth, rowHeight + i * rowHeight);
            ctx.fillText(entry.Score, this.canvas.width - 60, rowHeight + i * rowHeight);

            ctx.fillStyle = entry.Color;

            const x = this.canvas.width - width - rowHeight;
            const y = i * rowHeight + 10;

            ctx.fillRect(x, y, rowHeight, rowHeight);

            if (relativeTo) {
                const angle = Math.atan2(entry.Position.Y - relativeTo.Y, entry.Position.X - relativeTo.X);

                ctx.save();
                ctx.translate(x + rowHeight / 2, y + rowHeight / 2);
                const w = arrow.image.width;
                const h = arrow.image.height;
                ctx.rotate(angle);
                ctx.scale(arrow.scale, arrow.scale);
                ctx.drawImage(arrow.image, -w / 2, -h / 2, w, h);
                ctx.restore();
            }
        }

        if (this.data.Record) {
            ctx.font = "8pt " + Settings.font;
            ctx.fillStyle = "white";
            ctx.fillText(`record: ${this.data.Record.Name || "Unknown Fleet"} - ${this.data.Record.Score}`, margin, this.canvas.height - margin);
        }
        ctx.restore();
    }

    draw(relativeTo) {

        if (!Settings.leaderboardEnabled)
            return;

        switch (this.data.Type) {
            case "Team":
                this.modeTeam(relativeTo);
                break;
            case "CTF":
                this.modeCTF(relativeTo);
                break;
            case "FFA":
                this.modeStandard(relativeTo);
                break;
            default:
                //console.log(`Unknown leaderboard type: ${this.data.Type}`);
                break;
        }
    }
}



