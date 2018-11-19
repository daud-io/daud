namespace Game.API.Common.Models
{
    public class GameConnection
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public int Score { get; set; }
        public bool IsAlive { get; set; }
    }
}
