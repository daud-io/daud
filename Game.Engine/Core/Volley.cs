namespace Game.Engine.Core
{
    using System.Collections.Generic;

    public class Volley : ActorGroup
    {
        public Fleet FiredFrom { get; set; }
        public List<Bullet> NewBullets { get; set; } = new List<Bullet>();

        
    }
}