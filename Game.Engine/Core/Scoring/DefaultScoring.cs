namespace Game.Engine.Core.Scoring
{
    using System;

    public class DefaultScoring : ScoringBase
    {
        public override void ShipDied(Player killer, Player victim, Ship ship)
        {
            if (killer != null && !(ship is Fish))
                killer.Score += killer.World.Hook.PointsPerKillShip;
        }

        public override void FleetDied(Player killer, Player victim, Fleet fleet)
        {
            var hook = fleet.World.Hook;
            var time = fleet.World.Time;

            if (killer != null && victim != null)
            {
                try
                {
                    var comboTxt = "";
                    var comboPlusScore = 0;

                    if (killer.IsAlive)
                    {
                        if (time - killer.LastKillTime < hook.ComboDelay)
                        {
                            killer.ComboCounter += 1;
                            comboTxt = $"x{killer.ComboCounter} combo!";
                            comboPlusScore = (killer.ComboCounter - 1) * hook.ComboPointsStep;
                            killer.Score += comboPlusScore;
                        }
                        else
                            killer.ComboCounter = 1;

                        killer.MaxCombo = (killer.MaxCombo < killer.ComboCounter) ? killer.ComboCounter : killer.MaxCombo;

                        var PreviousKillTime = killer.LastKillTime;
                        killer.LastKillTime = time;

                        int plusScore = (int)(hook.PointsPerKillFleetStep * (MathF.Floor(victim.Score / hook.PointsPerKillFleetPerStep) + 1));
                        plusScore = (plusScore < hook.PointsPerKillFleetMax) ? plusScore : hook.PointsPerKillFleetMax;
                        killer.Score += plusScore;

                        killer.SendMessage($"You Killed {victim.Name}", "kill",
                            plusScore,
                            new
                            {
                                ping = new
                                {
                                    you = killer?.Connection?.Latency ?? 0,
                                    them = victim?.Connection?.Latency ?? 0
                                },
                                combo = new
                                {
                                    text = comboTxt,
                                    score = comboPlusScore
                                }
                            }
                        );

                        killer.KillCounter += 1;
                    }

                    victim.SendMessage($"Killed by {killer.Name}", "killed",
                        victim.Score,
                        new
                        {
                            score = victim.Score,
                            kills = victim.KillCounter,
                            gameTime = time - victim.AliveSince,
                            maxCombo = victim.MaxCombo,
                            ping = new
                            {
                                you = victim.Connection?.Latency ?? 0,
                                them = killer?.Connection?.Latency ?? 0
                            }
                        }
                    );
                    victim.Score = (int)Math.Max(victim.Score * hook.PointsMultiplierDeath, 0);

                    victim.KillCounter = 0;
                    victim.MaxCombo = 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception while scoring and sending messages: {e}");
                }
            }
            else
            {
                if (victim != null)
                {
                    victim.SendMessage($"Killed by the universe", "universeDeath", hook.PointsPerUniverseDeath,
                        new
                        {
                            score = victim.Score,
                            kills = victim.KillCounter,
                            gameTime = fleet.World.Time - victim.AliveSince,
                            maxCombo = victim.MaxCombo
                        }
                    );
                    victim.Score += hook.PointsPerUniverseDeath;
                    victim.KillCounter = 0;
                    victim.MaxCombo = 0;
                }
            }
        }
    }
}
