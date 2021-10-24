namespace Game.Robots.Boost
{
    public abstract class BoostBehavior
    {
        protected readonly ConfigTurret Robot;

        public BoostBehavior(ConfigTurret robot)
        {
            this.Robot = robot;
        }


        public abstract void Behave();
    }
}
