namespace Game.Engine.Auditing
{
    using Game.Engine.Core;

    public class AuditEventDeath : AuditEventBase
    {
        public AuditModelPlayer Killer { get; set; }
        public AuditModelPlayer Victim { get; set; }

        public AuditEventDeath(Player killer, Player victim, Fleet fleet)
            : base(fleet?.World)
        {
            Killer = new AuditModelPlayer(killer);
            Victim = new AuditModelPlayer(victim);
        }
    }
}
