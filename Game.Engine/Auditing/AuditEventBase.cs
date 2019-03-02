namespace Game.Engine.Auditing
{
    public class AuditEventBase
    {
        public string Type { get => this.GetType().Name; }
    }
}
