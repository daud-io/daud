namespace Game.Engine.Core.SystemActors
{
    using System.Numerics;

    public static class Static
    {
        public static Vector2 GeneratorStatic(Fleet fleet)
        {
            return fleet.World.Hook.SpawnLocation;
        }
    }
}