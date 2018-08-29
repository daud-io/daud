namespace Game.Engine.Core
{
    public interface ICollide
    {
        bool IsCollision(ProjectedBody projectedBody);
        void CollisionExecute(ProjectedBody projectedBody);
    }
}
