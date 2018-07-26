namespace Game.Models.Messages
{
    public class View : MessageBase
    {
        public override MessageTypes Type => MessageTypes.View;

        public PlayerView PlayerView { get; set; }
    }
}
