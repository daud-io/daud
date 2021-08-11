namespace Game.Engine.Core.SystemActors
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public class SpawnLocationsActor : SystemActorBase
    {
        private Dictionary<string, Func<Fleet, Vector2>> SpawnLocationModeMap =
            new Dictionary<string, Func<Fleet, Vector2>>();

        public void GeneratorAdd(string modeName, Func<Fleet, Vector2> generator)
        {
            lock (SpawnLocationModeMap)
                SpawnLocationModeMap.Add(modeName, generator);
        }
        public void GeneratorRemove(string modeName)
        {
            lock (SpawnLocationModeMap)
                SpawnLocationModeMap.Remove(modeName);
        }

        public SpawnLocationsActor(World world): base(world)
        {
            GeneratorAdd("Corners", Corners.GeneratorCorners);
            GeneratorAdd("Static", Static.GeneratorStatic);
            GeneratorAdd("QuietSpot", QuietSpot.GeneratorQuietSpot);

            GeneratorAdd("Default", QuietSpot.GeneratorQuietSpot);

        }

        protected override void CycleThink()
        {
            World.FleetSpawnPositionGenerator =
                SpawnLocationModeMap.ContainsKey(World.Hook.SpawnLocationMode)
                    ? SpawnLocationModeMap[World.Hook.SpawnLocationMode]
                    : SpawnLocationModeMap["Default"];
        }
    }
}