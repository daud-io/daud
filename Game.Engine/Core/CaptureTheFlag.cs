namespace Game.Engine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class CaptureTheFlag : IActor
    {
        private World World = null;
        private List<Flag> Flags = new List<Flag>();
        private List<Base> Bases = new List<Base>();
        private List<Team> Teams = new List<Team>();

        public Leaderboard LeaderboardGenerator()
        {
            return new Leaderboard
            {
                Type = "Team",
                Time = World.Time,
                Entries = Teams.Select(t => new Leaderboard.Entry
                {
                    Color = t.ColorName,
                    Name = t.ColorName,
                    Score = t.Score
                }).ToList()
            };
        }

        public Vector2 FleetSpawnPosition(Fleet fleet)
        {
            var color = fleet?.Owner?.Color;

            if (color != null)
            {
                var team = Teams.FirstOrDefault(t => t.ColorName == color);
                if (team != null)
                {
                    return team.BaseLocation;
                }
            }

            return Vector2.Zero;
        }

        private void CreateTeam(string teamName, string flagSpriteBase, Vector2 basePosition)
        {
            var team = new Team
            {
                BaseLocation = basePosition,
                ColorName = teamName
            };

            Teams.Add(team);

            var b = new Base(basePosition, team);
            var flag = new Flag(flagSpriteBase, team, b);
            b.Flag = flag;
            flag.Init(World);
            b.Init(World);
            Flags.Add(flag);
            Bases.Add(b);


            flag.ReturnToBase();
        }

        void IActor.CreateDestroy()
        {
            if (World.Hook.CTFMode)
                World.Hook.TeamMode = true;

            if (World.Hook.CTFMode && Flags.Count == 0)
            {
                CreateTeam("cyan", "flag_blue", new Vector2(World.Hook.WorldSize, World.Hook.WorldSize));
                CreateTeam("red", "flag_red", new Vector2(-World.Hook.WorldSize, -World.Hook.WorldSize));
                World.FleetSpawnPositionGenerator = this.FleetSpawnPosition;
                World.LeaderboardGenerator = this.LeaderboardGenerator;
            }

            if (!World.Hook.CTFMode && Flags.Count > 0)
            {
                foreach (var flag in Flags)
                    flag.Destroy();

                foreach (var b in Bases)
                    b.Destroy();

                Flags.Clear();
                Bases.Clear();

                World.FleetSpawnPositionGenerator = null;
                World.LeaderboardGenerator = null;
            }
        }

        void IActor.Destroy()
        {
            World.Actors.Remove(this);
        }

        void IActor.Init(World world)
        {
            World = world;
            World.Actors.Add(this);
        }

        void IActor.Think()
        {
        }

        private class Team
        {
            public string ColorName { get; set; }
            public Vector2 BaseLocation { get; set; }
            public int Score { get; set; }

            public void Scored()
            {
                Score++;
            }
        }

        private class Base : ActorBody, ICollide
        {
            private readonly Team Team;
            public Flag Flag { get; set; }

            public Base(Vector2 position, Team team)
            {
                this.Team = team;
                this.Position = position;
                this.Sprite = Sprites.ctf_base;
                this.AngularVelocity = 0.03f;
                this.Size = 200;
            }

            void ICollide.CollisionExecute(Body projectedBody)
            {
                var flag = projectedBody as Flag;

                this.Team.Scored();

                flag.ReturnToBase();
            }

            private bool FlagIsHome()
            {
                return Vector2.Distance(Flag.Position, this.Position)
                    < (Flag.Size + this.Size);
            }

            bool ICollide.IsCollision(Body projectedBody)
            {
                if (projectedBody is Flag flag)
                {
                    if (flag.Team == this.Team)
                        return false;

                    if (!FlagIsHome())
                        return false;

                    return Vector2.Distance(projectedBody.Position, this.Position)
                        < (projectedBody.Size + this.Size);
                }
                return false;
            }
        }

        private class Flag : ActorBody, ICollide
        {
            private readonly uint SpriteAnimationInterval = 100;
            public readonly Team Team;
            private readonly Base Base;

            private ActorGroup FlagGroup = new ActorGroup();
            private List<Sprites> SpriteSet = new List<Sprites>();
            private uint NextSpriteTime = 0;
            private int SpriteIndex = 0;
            private Fleet CarriedBy = null;

            public Flag(string baseSpriteName, Team team, Base b)
            {
                Size = 200;
                Team = team;
                Base = b;

                var i = 0;
                var done = false;

                while (!done)
                {
                    if (Enum.TryParse<Sprites>($"{baseSpriteName}_{i++}", out var result))
                        SpriteSet.Add(result);
                    else
                        done = true;
                }

                if (SpriteSet.Any())
                    Sprite = SpriteSet[0];
            }

            public override void Init(World world)
            {
                base.Init(world);

                FlagGroup.Init(world);
                FlagGroup.ZIndex = 10;
                this.Group = FlagGroup;
                Position = world.RandomPosition();
            }

            public override void Destroy()
            {
                base.Destroy();
                FlagGroup.Destroy();
            }

            public override void Think()
            {
                base.Think();

                if (CarriedBy?.Owner?.IsAlive ?? false)
                {
                    this.Position = CarriedBy.FleetCenter;
                    this.Momentum = CarriedBy.FleetMomentum;

                    Console.WriteLine($"X:{CarriedBy.FleetMomentum.X} Y:{CarriedBy.FleetMomentum.Y}");
                }
                else
                {
                    CarriedBy = null;
                    this.Momentum = new Vector2(0, 0);
                }

                if (World.Time > NextSpriteTime)
                {
                    SpriteIndex = (SpriteIndex + 1) % SpriteSet.Count;

                    Sprite = SpriteSet[SpriteIndex];

                    NextSpriteTime = World.Time + SpriteAnimationInterval;
                }
            }

            public void ReturnToBase()
            {
                this.Position = Base.Position;
                this.CarriedBy = null;
            }

            void ICollide.CollisionExecute(Body projectedBody)
            {
                var ship = projectedBody as Ship;
                var fleet = ship.Fleet;

                if (fleet != null && CarriedBy == null && !(fleet.Owner is Robot))
                {
                    if (fleet.Owner.Color == Team.ColorName)
                        ReturnToBase();
                    else
                        CarriedBy = fleet;
                }
            }

            bool ICollide.IsCollision(Body projectedBody)
            {
                if (projectedBody is Ship ship)
                {
                    if (ship.Abandoned)
                        return false;

                    return Vector2.Distance(projectedBody.Position, this.Position)
                        < (projectedBody.Size + this.Size);
                }
                return false;
            }
        }
    }
}
