namespace Game.API.Common.Models.Auditing
{
    public class AuditEventDeath : AuditEventBase
    {
        public AuditModelPlayer Killer { get; set; }
        public AuditModelPlayer Victim { get; set; }
    }
}
