namespace Game.API.Common.Models.Auditing
{
    public class AuditEventSpawn : AuditEventBase
    {
        public AuditModelPlayer Player { get; set; }
    }
}
