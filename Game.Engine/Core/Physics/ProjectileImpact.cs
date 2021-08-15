using BepuPhysics;

namespace Game.Engine.Physics
{
    public struct BodyImpacts
    {
        public BodyHandle BodyHandleA; // will be dynamic
        public BodyHandle BodyHandleB; // might -1 for static object
    }
}
