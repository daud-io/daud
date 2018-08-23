namespace Game.Engine.Core
{
    using Game.Models;

    public interface ICollide
    {
        bool IsCollision(ProjectedBody projectedBody);
        void CollisionExecute(ProjectedBody projectedBody);
    }
}
