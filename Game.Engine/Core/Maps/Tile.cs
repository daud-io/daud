namespace Game.Engine.Core.Maps
{
    public class Tile : ActorBody, ICollide
    {
        public bool IsDeadly { get; set; } = false;

        public void CollisionExecute(Body projectedBody)
        {
            if (projectedBody is Ship ship)
                if (ship.Fleet != null
                    && !ship.Fleet.Owner.IsInvulnerable
                    && ship.Fleet.BoostUntil <= World.Time - 1000
                    )
                        ship.Fleet.AbandonShip(ship);
        }

        public bool IsCollision(Body projectedBody)
        {
            if (!IsDeadly)
                return false;
            else
                return (projectedBody is Ship);
        }

    }
}
