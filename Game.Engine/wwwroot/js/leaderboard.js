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

            if (this.data && this.data.Entries) {
                ctx.font = "12pt sans-serif";
                ctx.fillStyle = "white";
                ctx.textAlign = "left";

                var width = 200;
                var rowHeight = 28;
                var rightMargin = 40;

                for (var i = 0; i < this.data.Entries.length; i++) {
                    var entry = this.data.Entries[i];

                    ctx.fillText(entry.Name || "Unknown Fleet", this.canvas.width - width, rowHeight + (i * rowHeight)); 
                    ctx.fillText(entry.Score, this.canvas.width - rightMargin, rowHeight + (i * rowHeight)); 
                }
            }
        }
    };

    this.Game.Leaderboard = Leaderboard;
}).call(this);
