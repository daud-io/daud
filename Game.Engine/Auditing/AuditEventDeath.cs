namespace Game.Engine.Auditing
{
    using Game.Engine.Core;

    public class AuditEventDeath : AuditEventBase
    {
        public long GameTime { get; set; }
        public AuditModelPlayer Killer { get; set; }
        public AuditModelPlayer Victim { get; set; }

        public AuditEventDeath(Player killer, Player victim, Fleet fleet)
        {
            GameTime = fleet?.World.Time ?? 0;
            PublicURL = fleet?.World?.GameConfiguration?.PublicURL;
            WorldKey = fleet?.World?.WorldKey;

            Killer = new AuditModelPlayer(killer);
            Victim = new AuditModelPlayer(victim);
        }
    }
}
