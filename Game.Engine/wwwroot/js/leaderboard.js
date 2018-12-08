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

    draw(relativeTo) {
        const ctx = this.context;

        if (!Settings.leaderboardEnabled)
            return;

        ctx.save();
        if (this.data && this.data.Entries) {
            ctx.font = "12pt " + Settings.font;
            ctx.fillStyle = "white";
            ctx.textAlign = "left";

            const width = 200;
            var rowHeight = 28;
            const margin = 20;
            const arrow = sprites["arrow"];

            if (this.data.Type == "Team") {
                var self = this;
                var findTeam = function (teamName) {
                    for (var i = 0; i < self.data.Entries.length; i++) {
                        if (self.data.Entries[i].Name == teamName)
                            return self.data.Entries[i];
                    }
                    return false;
                }

                var cyan = findTeam("cyan") || { Score: 0};
                var red = findTeam("red") || { Score: 0 };

                var cyanScore = Math.min(cyan.Score, 5);
                var redScore = Math.min(red.Score, 5);

                var x = this.canvas.width - 300;
                var y = 15;
                var w = 300;
                var h = 100;

                var draw = function (sprite) {
                    if (sprites.hasOwnProperty(sprite))
                        ctx.drawImage(sprites[sprite].image, x, y, w, h);
                }

                draw("ctf_score_stripes");
                draw(`ctf_score_left_${Math.min(cyanScore, 4)}`);
                draw(`ctf_score_right_${Math.min(redScore, 4)}`);

                if (cyan.Score == 5)
                    draw("ctf_score_final_blue");
                else if (red.Score == 5)
                    draw("ctf_score_final_red");
                else
                    draw("ctf_score_final");
            }
            else {


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
            }
        }

        ctx.restore();
    }
}
