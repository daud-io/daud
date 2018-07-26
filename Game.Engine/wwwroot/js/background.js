(function () {
    var Background = function (canvas, context, settings) {
        settings = settings || {};
        this.img = new Image();
        this.img.src = 'img/bg.jpg';
        this.context = context;
        this.canvas = canvas;
    };

    Background.prototype = {
        draw: function () {
            this.context.fillStyle = this.context.createPattern(this.img, 'repeat');
            this.context.fillRect(-10000, -10000, 20000, 20000); 
            
        }
    };

    this.Game.Background = Background;
}).call(this);
