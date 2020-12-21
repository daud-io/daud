using System.Linq;
using System.Numerics;

namespace Game.Engine.Core.SystemActors
{
    public class AdvanceRetreat : SystemActorBase
    {
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
                        var dp = Vector2.Dot(Vector2.Normalize(relative), Vector2.Normalize(player.Fleet.FleetMomentum));

                        player.Advance = (99 * player.Advance + dp) / 100f;
                    }
                }
            }
        }
    }
}