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
        draw: function () {
            var ctx = this.context;
            ctx.save();
            if (this.data && this.data.Entries) {
                ctx.font = "12pt sans-serif";
                ctx.fillStyle = "white";
                ctx.textAlign = "left";

                var width = 200;
                var rowHeight = 28;
                var margin = 20;

                for (var i = 0; i < this.data.Entries.length; i++) {
                    var entry = this.data.Entries[i];

                    ctx.fillStyle = "white";


                    ctx.fillText(entry.Name || "Unknown Fleet", this.canvas.width - width, rowHeight + (i * rowHeight)); 
                    ctx.fillText(entry.Score, this.canvas.width - 60, rowHeight + (i * rowHeight)); 

                    ctx.fillStyle = entry.Color;
                    ctx.fillRect(this.canvas.width - width - rowHeight, (i * rowHeight) + 10, rowHeight, rowHeight);

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
