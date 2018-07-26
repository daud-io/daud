namespace Game.Models.Messages
{
    public class Steering : MessageBase
    {
        public override MessageTypes Type => MessageTypes.Steering;
        public float Angle { get; set; }
    }
}