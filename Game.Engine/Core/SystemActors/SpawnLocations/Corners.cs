namespace Game.Engine.Core.SystemActors
{
    using System;
    using System.Numerics;

    public static class Corners
    {
        public static Vector2 GeneratorCorners(Fleet fleet)
        {
            var r = new Random();

            var x = r.NextDouble() > .5
                ? 1
                : -1;
            var y = r.NextDouble() > .5
                ? 1
                : -1;

            return new Vector2
            {
                X = x * fleet.World.Hook.WorldSize * 0.95f,
                Y = y * fleet.World.Hook.WorldSize * 0.95f
            };
        }
    }
}