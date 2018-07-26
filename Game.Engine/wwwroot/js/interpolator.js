(function () {
    var Interpolator = function (settings) {
        settings = settings || {};
        this.MS_PER_FRAME = 40;
        this.newFrame();
    };

    Interpolator.prototype = {
        newFrame: function (view) {
            this.time = performance.now();
        },
        projectObject: function (object, time) {
            var timeShift = time - this.time;

            var frameScale = timeShift / this.MS_PER_FRAME;

            var newPoint = 
            {
                X: (object.LastPosition.X * (1 - frameScale) + object.Position.X * frameScale),
                Y: (object.LastPosition.Y * (1 - frameScale) + object.Position.Y * frameScale)
            };

            return newPoint;
        }
    };

    this.Game.Interpolator = Interpolator;
}).call(this);
