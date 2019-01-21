namespace Game.Robots.Senses
{
    using Game.Robots.Models;

    public class SensorTeam : ISense
    {
        private readonly ContextRobot Robot;

        public enum Teams
        {
            Cyan,
            Red
        }

        public Teams Team { get; private set; }

        public SensorTeam(ContextRobot robot)
        {
            this.Robot = robot;
        }

        public void Sense()
        {
            if (this.Robot.Color == "cyan")
                this.Team = Teams.Cyan;
            else
                this.Team = Teams.Red;
        }
    }
}
