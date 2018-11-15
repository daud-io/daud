namespace Game.Engine.Core
{
    public interface ICollide
    {
        bool IsCollision(Body projectedBody);
        void CollisionExecute(Body projectedBody);
    }
}
