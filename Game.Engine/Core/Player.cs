namespace Game.Engine.Core
{
    using Game.Models.Messages;
    using System.Collections.Generic;

    public class Player : IActor
    {
        public World World = null;
        public Fleet Fleet = null;

        public int Score { get; set; }

        public ControlInput ControlInput { get; set; }
        private bool IsControlNew = false;

        public List<string> Messages { get; set; } = new List<string>();

        public bool IsAlive { get; set; } = false;

        public void SetControl(ControlInput input)
        {
            this.ControlInput = input;
            this.IsControlNew = true;
        }

        public void Deinit()
        {
            Die();
            World.Actors.Remove(this);
        }

        public void Init(World world)
        {
            World = world;
            world.Actors.Add(this);
        }

        public void Step()
        {
            if (this.IsControlNew)
            {
                Fleet.Angle = ControlInput.Angle;
                Fleet.Caption = ControlInput.Name;
                Fleet.Sprite = ControlInput.Ship;
                Fleet.BoostRequested = ControlInput.BoostRequested;
                Fleet.ShootRequested = ControlInput.ShootRequested;
            }

            this.IsControlNew = false;
        }

        public void Spawn()
        {
            if (!IsAlive)
            {
                IsAlive = true;

                Fleet = new Fleet
                {
                    Owner = this
                };
                Fleet.Init(World);
            }
        }

        public void Die()
        {
            if (IsAlive)
            {
                Fleet.Deinit();
                Fleet = null;
                IsAlive = false;
            }
        }

        public List<string> GetMessages()
        {
            if (Messages.Count > 0)
            {
                var m = Messages;
                Messages = new List<string>();
                return m;
            }
            else
                return null;
        }
    }
}
