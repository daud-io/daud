(function () {
    var Interpolator = function (settings) {
        settings = settings || {};
    };

    Interpolator.prototype = {
        projectObject: function (object, time) {
            var timeShift = time - object.DefinitionTime;

            var newPoint = (object.OriginalPosition)
                ? {
                    X: (object.OriginalPosition.X + (timeShift * object.Momentum.X)),
                    Y: (object.OriginalPosition.Y + (timeShift * object.Momentum.Y)),
                    Angle: object.OriginalAngle + timeShift * object.AngularVelocity
                }
                : object.Position;

            return newPoint;
        }
    };

    this.Game.Interpolator = Interpolator;
}).call(this);
