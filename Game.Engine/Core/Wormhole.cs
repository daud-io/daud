namespace Game.Engine.Core
{
    public class Wormhole : Obstacle
    {
        public string TargetWorld { get; set; }

        public Wormhole()
        {
            AngularVelocity = 0.005f;
        }

        public Wormhole(string targetWorld = null)
        {
            TargetWorld = targetWorld ?? World.Hook.WormholesDestination;
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
