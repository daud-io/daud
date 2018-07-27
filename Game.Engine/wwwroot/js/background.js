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
            this.context.fillStyle = this.context.createPattern(this.img, 'repeat');
            this.context.fillRect(-100000, -100000, 200000, 200000); 
        }
    };

    this.Game.Background = Background;
}).call(this);
