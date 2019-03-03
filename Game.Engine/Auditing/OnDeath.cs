namespace Game.Engine.Auditing
{
    public class OnDeath
    {
#pragma warning disable IDE1006 // Naming Styles
        public string token { get; set; }
        public string name { get; set; }
        public string killedBy { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
