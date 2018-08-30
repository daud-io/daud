namespace Game.Engine.Core
{
    using System.Numerics;

    public class Obstacle : ActorBody, ICollide
    {
        public void CollisionExecute(ProjectedBody projectedBody)
        {
        }

        public bool IsCollision(ProjectedBody projectedBody)
        {
            return Vector2.Distance(projectedBody.Position, this.Position) 
                < (projectedBody.Size + this.Size);
        }

        public override void Step()
        {
            if (World.DistanceOutOfBounds(Position) > 0)
            {
                var speed = Momentum.Length();
                Momentum = Vector2.Normalize(Vector2.Zero - Position) * speed;
            }
        }
    }
}
