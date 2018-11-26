export class Interpolator {
    constructor(settings) {
        settings = settings || {};
    }

    shortAngleDist(a0, a1) {
        var max = Math.PI * 2;
        var da = (a1 - a0) % max;
        return ((2 * da) % max) - da;
    }

    angleLerp(a0, a1, t) {
        return a0 + this.shortAngleDist(a0, a1) * t;
    }

    lerp(value1, value2, amount) {
        amount = amount < 0 ? 0 : amount;
        amount = amount > 1 ? 1 : amount;
        return value1 + (value2 - value1) * amount;
    }

    projectObject(object, time) {
        var timeShift = time - object.DefinitionTime;
        object.Angle = object.OriginalAngle + timeShift * object.AngularVelocity;
        object.Position = {
            X: object.OriginalPosition.X + timeShift * object.Momentum.X,
            Y: object.OriginalPosition.Y + timeShift * object.Momentum.Y
        };

        if (object.previous && object.previous.Position) {
            //var lerpAmount = Math.max(0.0, Math.min((time-object.previous.obsolete) / 400.0, 1.0));
            var lerpAmount = 0.7;

            if (lerpAmount > 0 && lerpAmount < 1) {
                var x = 1;
            }

            // disable position lerping
            object.previous.Position = object.Position;
            //object.previous.Position.X = this.lerp(object.previous.Position.X, object.Position.X, lerpAmount);
            //object.previous.Position.Y = this.lerp(object.previous.Position.Y, object.Position.Y, lerpAmount);

            object.previous.Angle = this.angleLerp(object.previous.Angle, object.Angle, lerpAmount);

            var newPoint = {
                X: object.previous.Position.X,
                Y: object.previous.Position.Y,
                Angle: object.previous.Angle
            };

            return newPoint;
        } else {
            var newPoint = {
                X: object.Position.X,
                Y: object.Position.Y,
                Angle: object.Angle
            };

            return newPoint;
        }
    }
}
