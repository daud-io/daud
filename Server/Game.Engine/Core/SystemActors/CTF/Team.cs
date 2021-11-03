namespace Game.Engine.Core.SystemActors.CTF
{
    using System.Numerics;

    public class Team
    {
        public string ColorName { get; set; }
        public Vector2 BaseLocation { get; set; }
        public int Score { get; set; }
        public Flag Flag { get; set; }
        public Base Base { get; set; }
        public CaptureTheFlag CaptureTheFlag { get; set; }

        public void Scored()
        {
            Score++;

            CaptureTheFlag.TeamScored(this);
        }
    }
}
