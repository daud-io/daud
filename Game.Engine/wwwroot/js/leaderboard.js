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
                ctx.font = "20px sans-serif";
                ctx.fillStyle = "white";
                ctx.textAlign = "center";

                for (var i = 0; i < this.data.Entries.length; i++) {
                    var entry = this.data.Entries[i];

                    ctx.fillText(entry.Name || "Unknown Fleet", this.canvas.width - 200, 40 + (i * 40)); 
                    ctx.fillText(entry.Score, this.canvas.width - 40, 40 + (i * 40)); 
                }
            }
        }
    };

    this.Game.Leaderboard = Leaderboard;
}).call(this);
