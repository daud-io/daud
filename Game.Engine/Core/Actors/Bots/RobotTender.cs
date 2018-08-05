namespace Game.Engine.Core.Actors.Bots
{
    using System.Collections.Generic;
    using System.Linq;

    public class RobotTender : ActorBase
    {
        private readonly List<Robot> Robots = new List<Robot>();

        private void AddRobot()
        {
            var bot = new Robot()
            {
                Name = $"Daudelin #{this.Robots.Count}",
                Ship = "ship0"
            };
            bot.Init(world);
            bot.Spawn();

            this.Robots.Add(bot);
        }

        public override void Step()
        {
            var leader = world.Leaderboard?.Entries.FirstOrDefault();

            int desired = leader != null
                ? leader.Score / 500 + 1
                : 1;

            while (Robots.Count < desired)
                AddRobot();

            while (Robots.Count > desired)
            {
                var robot = Robots[Robots.Count - 1];
                Robots.Remove(robot);
                robot.Die();
                robot.Deinit();
            }
        }
    }
}
