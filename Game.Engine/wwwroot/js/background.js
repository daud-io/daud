(function () {
    var Background = function (canvas, context, settings) {
        settings = settings || {};
        this.img = new Image();
        this.img.src = 'img/bg.png';
        this.context = context;
        this.canvas = canvas;
    };

    Background.prototype = {
        draw: function () {
            var ctx = this.context;

            ctx.save();
            ctx.scale(4, 4);
            ctx.fillStyle = this.context.createPattern(this.img, 'repeat');
            ctx.fillRect(-100000, -100000, 200000, 200000); 
            ctx.restore();
            
        }
    };

    this.Game.Background = Background;
}).call(this);
