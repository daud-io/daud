namespace Game.Models.Messages
{
    public class ControlInput : MessageBase
    {
        public override MessageTypes Type => MessageTypes.ControlInput;
        public float Angle { get; set; }
        public bool BoostRequested { get; set; }
        public bool ShootRequested { get; set; }

        public string Name { get; set; }
        public string Ship { get; set; }
    }
}