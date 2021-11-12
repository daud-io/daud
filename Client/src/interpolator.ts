import { ClientBody } from "./cache";

function shortAngleDist(a0: number, a1: number): number {
    const max = Math.PI * 2;
    const da = (a1 - a0) % max;
    return ((2 * da) % max) - da;
}

export function angleLerp(a0: number, a1: number, t: number): number {
    return a0 + shortAngleDist(a0, a1) * t;
}

export function projectObject(object: ClientBody, time: number): void {
    const timeShift = time - object.DefinitionTime;
    if (object.AngularVelocity == 0)
        //object.Angle = object.OriginalAngle;
        object.Angle = angleLerp(object.Angle, object.OriginalAngle, 0.7);
    else object.Angle = object.OriginalAngle + timeShift * object.AngularVelocity;

    object.Position.x = object.OriginalPosition.x + timeShift * object.Velocity.x;
    object.Position.y = object.OriginalPosition.y + timeShift * object.Velocity.y;
}

