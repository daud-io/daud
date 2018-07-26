namespace Game.Models.Messages
{
    public class Ping : MessageBase
    {
        public override MessageTypes Type => MessageTypes.Ping;
    }
}
