using System;
using System.Linq;
using System.Numerics;

namespace Game.Engine.Core.SystemActors
{
    public class AdvanceRetreat : SystemActorBase
    {
        public AdvanceRetreat()
        {
        }

        protected override void CycleThink()
        {
            var livePlayers = Player.GetWorldPlayers(World).Where(p => p.IsAlive).ToList();

            foreach (var player in livePlayers)
            {
                var nearby = livePlayers
                    .Where(p => p != player) // that's not us
                    .Where(p => Vector2.Distance(p.Fleet.FleetCenter, player.Fleet.FleetCenter) < 2400);

                if (nearby.Count() > 0)
                {
                    foreach (var other in nearby)
                    {
                        var relative = other.Fleet.FleetCenter - player.Fleet.FleetCenter;
                        var approach = MathF.Atan2(relative.Y, relative.X);
                        var actual = MathF.Atan2(player.ControlInput.Position.Y, player.ControlInput.Position.X);
                        
                        var dp = Vector2.Dot(Vector2.Normalize(relative), Vector2.Normalize(player.ControlInput.Position));

                        player.Advance = (99 * player.Advance + dp) / 100f;
                    }
                }
            }
        }
    }
}