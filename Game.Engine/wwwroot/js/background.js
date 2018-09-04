(function () {
    var Background = function (canvas, context, settings) {
        settings = settings || {};
        this.img = new Image();
        this.img.src = 'img/bg.png';
        this.context = context;

        this.parallaxFactor = 50;

        var self = this;
        this.img.onload = function () {
            self.pattern = self.context.createPattern(self.img, 'repeat');
        };
        this.canvas = canvas;
    };

    Background.prototype = {
        draw: function (x, y) {
            var ctx = this.context;
            x /= this.parallaxFactor;
            y /= this.parallaxFactor;

            ctx.save();
            ctx.scale(10, 10);
            ctx.fillStyle = this.pattern;
            ctx.translate(-x, -y)
            ctx.fillRect(-100000 + x, -100000 + y, 200000, 200000); 
            ctx.restore();
        }
    };

    this.Game.Background = Background;
}).call(this);
