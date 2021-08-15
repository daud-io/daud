namespace Game.Engine.Physics
{
    /// <summary>
    /// Stores properties about a body in the tank demo.
    /// </summary>
    public struct WorldBodyProperties
    {
        /// <summary>
        /// Friction coefficient to use for the body.
        /// </summary>
        public float Friction;
        /// <summary>
        /// True if the body is a projectile and should explode on contact.
        /// </summary>
        public bool Projectile;
        /// <summary>
        /// True if the body is part of a tank.
        /// </summary>
    }
}
