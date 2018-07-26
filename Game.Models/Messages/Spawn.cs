namespace Game.Models.Messages
{
    public class Spawn : MessageBase
    {
        public override MessageTypes Type => MessageTypes.Spawn;

        public string Name { get; set; }
    }
}
