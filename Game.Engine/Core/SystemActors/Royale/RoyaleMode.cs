namespace Game.Engine.Core.SystemActors.Royale
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class RoyaleMode : SystemActorBase
    {
        public uint GameRestartTime { get; set; } = 0;
        public uint GameEmptySince { get; set; } = 0;

        public uint NextAnnounceTime { get; set; } = 0;

        private bool Initialized = false;

        private uint CountdownUntil = 0;
        private int RestartDelayMS = 10000;
        private int OriginalWorldsizeDeltaPerPlayer = 0;
        private int OriginalWorldResizeSpeed = 0;

        private GameStateEnum GameState = GameStateEnum.Prestart;
        enum GameStateEnum
        {
            Prestart,
            Countdown,
            Running,
            Waiting
        }

        private int StartingArenaSize;
        private StartingBlock StartingBlock;

        public RoyaleMode()
        {
            CycleMS = 0;
        }

        public override void Init(World world)
        {
            base.Init(world);
        }


        public override void CreateDestroy()
        {
            base.CreateDestroy();

            if (World.Hook.RoyaleMode && !Initialized)
            {
                // setup
                OriginalWorldsizeDeltaPerPlayer = World.Hook.WorldSizeDeltaPerPlayer;
                OriginalWorldResizeSpeed = World.Hook.WorldResizeSpeed;
                World.Hook.WorldMinPlayersToResize = 0;
                World.Hook.WorldResizeEnabled = true;

                StartingBlock = new StartingBlock
                {
                    ParentGame = this
                };
                StartingBlock.Init(World);
                ResetGame();
                Initialized = true;

            }

            if (!World.Hook.RoyaleMode && Initialized)
            {
                // tear down

                StartingBlock.Destroy();
                World.Hook.WorldSizeDeltaPerPlayer = OriginalWorldsizeDeltaPerPlayer;
                World.CanSpawn = true;
                Initialized = false;
            }
        }

        public void ResetGame()
        {
            GameRestartTime = 0;
            GameState = GameStateEnum.Prestart;
            World.CanSpawn = true;
            StartingBlock.Position = Vector2.Zero;

            var players = Player.GetWorldPlayers(World).ToList();
            foreach (var player in players)
                player.Score = 0;
        }

        public void StartCountdown()
        {
            InRoomAnnouncement($"Game Starting in {World.Hook.RoyaleCountdownDurationSeconds} seconds");
            GameState = GameStateEnum.Countdown;

            // moving it out of the world
            StartingBlock.Position = new Vector2(-10000,-10000);

            CountdownUntil = World.Time + (uint)(World.Hook.RoyaleCountdownDurationSeconds * 1000);
        }

        private void StartGame()
        {
            InRoomAnnouncement("3.. 2.. 1.. GO!!");
            GameState = GameStateEnum.Running;

            StartingArenaSize = World.Hook.WorldSize;
            World.CanSpawn = false;
            World.CanSpawnReason = "You can't join this game right now. Wait for the next one.";
            World.Hook.WorldSizeDeltaPerPlayer = 0;
            World.Hook.WorldResizeSpeed = World.Hook.RoyaleResizeSpeed;
        }

        private void StepGame(List<Player> players)
        {
            var playerCount = players
                .Where(p => p.IsAlive).Count();

            if (playerCount == 1)
            {
                // someone won
                InRoomAnnouncement($"GAME OVER!: {players?.FirstOrDefault(p => p.IsAlive)?.Name} wins!");
                GameOver();
            }
            else if (playerCount == 0)
            {
                // everyone died
                InRoomAnnouncement($"GAME OVER! Everyone Loses!");
                GameOver();
            }
        }

        private void GameOver()
        {
            GameState = GameStateEnum.Waiting;
            GameRestartTime = (uint)(World.Time + RestartDelayMS);
            World.Hook.WorldSizeDeltaPerPlayer = OriginalWorldsizeDeltaPerPlayer;
            World.Hook.WorldResizeSpeed = OriginalWorldResizeSpeed;
        }

        private void InRoomAnnouncement(string message)
        {
            foreach (var player in Player.GetWorldPlayers(World))
                player.SendMessage(message, type: "announce");
        }

        protected override void CycleThink()
        {
            var players = Player.GetWorldPlayers(World).ToList();

            switch (GameState)
            {
                case GameStateEnum.Prestart:
                    /*if (Initialized)
                        if (players.Count(p => p.IsAlive) > 0)
                            StartCountdown();*/
                    break;

                case GameStateEnum.Countdown:

                    if (World.Time > CountdownUntil)
                        StartGame();
                    break;
                case GameStateEnum.Running:
                    StepGame(players);
                    break;
                case GameStateEnum.Waiting:
                    break;
            }

            if (GameRestartTime > 0 && World.Time > GameRestartTime)
                ResetGame();

            var playerCount = players
                .Where(p => p.IsAlive).Count();

            if (playerCount == 0)
            {
                if (GameEmptySince == 0)
                    GameEmptySince = World.Time;
            }

            if (playerCount > 0)
            {

                GameEmptySince = 0;
            }

        }
    }
}
