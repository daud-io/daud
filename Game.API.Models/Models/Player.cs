namespace Game.API.Common.Models
{
    public class GameConnection
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int Score { get; set; }
        public bool IsAlive { get; set; }

        public bool Backgrounded { get; set; }
        public uint ClientFPS { get; set; }
        public uint ClientVPS { get; set; }
        public uint ClientUPS { get; set; }
        public uint ClientCS { get; set; }
        public uint Bandwidth { get; set; }
    }
}
