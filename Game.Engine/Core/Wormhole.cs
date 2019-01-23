namespace Game.Engine.Core
{
    public class Wormhole : Obstacle
    {
        public string TargetWorld { get; set; }

        public override void Init(World world)
        {
            base.Init(world);
            Sprite = API.Common.Sprites.wormhole;
            AngularVelocity = 0.000f;
        }

        public Wormhole()
        {
        }

        public Wormhole(string targetWorld = null)
        {
            TargetWorld = targetWorld ?? World.Hook.WormholesDestination;
        }

        public override void Think()
        {
            base.Think();
            this.Size = 200;
        }

        public override void CollisionExecute(Body projectedBody)
        {
            if (projectedBody is Ship ship)
                if (ship?.Fleet?.Owner != null)
                    ship.Fleet.Owner.SendMessage(TargetWorld ?? "default", "join");

            base.CollisionExecute(projectedBody);
        }

        public override bool IsCollision(Body projectedBody)
        {
            if (projectedBody is Ship ship)
                if (ship?.Fleet?.Owner != null)
                    return true;

            return base.IsCollision(projectedBody); ;
        }
    }
}
