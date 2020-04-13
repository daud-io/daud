import { Vector2 } from "./Vector2";

export class Interpolator {
    constructor(settings = {}) {}

    shortAngleDist(a0: number, a1: number) {
        const max = Math.PI * 2;
        const da = (a1 - a0) % max;
        return ((2 * da) % max) - da;
    }

    angleLerp(a0: number, a1: number, t: number) {
        return a0 + this.shortAngleDist(a0, a1) * t;
    }

    lerp(value1: number, value2: number, amount: number) {
        amount = amount < 0 ? 0 : amount;
        amount = amount > 1 ? 1 : amount;
        return value1 + (value2 - value1) * amount;
    }

    projectObject(object, time: number) {
        const timeShift = time - object.DefinitionTime;
        object.Angle = object.OriginalAngle + timeShift * object.AngularVelocity;
        object.Position = new Vector2(Math.floor(object.OriginalPosition.x + timeShift * object.Momentum.x), Math.floor(object.OriginalPosition.y + timeShift * object.Momentum.y));

        if (object.previous && object.previous.Position) {
            //var lerpAmount = Math.max(0.0, Math.min((time-object.previous.obsolete) / 400.0, 1.0));
            const lerpAmount = 0.7;

            if (lerpAmount > 0 && lerpAmount < 1) {
                const x = 1;
            }

            // disable position lerping
            object.previous.Position = object.Position;
            //object.previous.Position.X = this.lerp(object.previous.Position.X, object.Position.X, lerpAmount);
            //object.previous.Position.Y = this.lerp(object.previous.Position.Y, object.Position.Y, lerpAmount);

            object.previous.Angle = this.angleLerp(object.previous.Angle, object.Angle, lerpAmount);

            const newPoint = {
                x: object.previous.Position.x,
                y: object.previous.Position.y,
                Angle: object.previous.Angle,
            };

            return newPoint;
        } else {
            const newPoint = {
                x: object.Position.x,
                y: object.Position.y,
                Angle: object.Angle,
            };

            return newPoint;
        }
    }
}
