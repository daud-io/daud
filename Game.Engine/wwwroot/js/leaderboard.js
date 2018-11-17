(function () {
    var Leaderboard = function (canvas, context, settings) {
        settings = settings || {};
        this.context = context;
        this.canvas = canvas;
        this.data = false;
    };

    Leaderboard.prototype = {
        setData: function (data) {
            this.data = data;
        },
        draw: function (relativeTo) {
            var ctx = this.context;
            ctx.save();
            if (this.data && this.data.Entries) {
                ctx.font = "12pt sans-serif";
                ctx.fillStyle = "white";
                ctx.textAlign = "left";

                var width = 200;
                var rowHeight = 28;
                var margin = 20;
                var arrow = Game.Renderer.sprites['arrow'];

                for (var i = 0; i < this.data.Entries.length; i++) {
                    var entry = this.data.Entries[i];

                    ctx.fillStyle = "white";


                    ctx.fillText(entry.Name || "Unknown Fleet", this.canvas.width - width, rowHeight + (i * rowHeight));
                    ctx.fillText(entry.Score, this.canvas.width - 60, rowHeight + (i * rowHeight));

                    ctx.fillStyle = entry.Color;

                    var x = this.canvas.width - width - rowHeight;
                    var y = (i * rowHeight) + 10, rowHeight;

                    ctx.fillRect(x, y, rowHeight, rowHeight);


                    if (relativeTo) {
                        var angle = Math.atan2(entry.Position.Y - relativeTo.Y, entry.Position.X - relativeTo.X);

                        ctx.save();
                        ctx.translate(x + rowHeight / 2, y + rowHeight / 2);
                        var w = arrow.image.width;
                        var h = arrow.image.height;
                        ctx.rotate(angle);
                        ctx.scale(arrow.scale, arrow.scale);
                        ctx.drawImage(arrow.image, -w / 2, -h / 2, w, h);
                        ctx.restore();

                    }
                }

                if (this.data.Record) {
                    ctx.font = "8pt sans-serif";
                    ctx.fillStyle = "white";
                    ctx.fillText("record: " + (this.data.Record.Name || "Unknown Fleet") + " - " + this.data.Record.Score, margin, this.canvas.height - margin);

                }

            }

            ctx.restore();
        }
    };

    this.Game.Leaderboard = Leaderboard;
}).call(this);
