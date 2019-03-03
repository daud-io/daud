namespace Game.Engine.Auditing
{
    public class AuditEventBase
    {
        public string Type { get => this.GetType().Name; }
        public string PublicURL { get; set; }
        public string WorldKey { get; set; }
    }
}
