namespace Game.Models.Messages
{
    public class ControlInput : MessageBase
    {
        public override MessageTypes Type => MessageTypes.ControlInput;
        public float Angle { get; set; }
        public bool BoostRequested { get; set; }
    }
}