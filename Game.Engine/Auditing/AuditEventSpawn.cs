namespace Game.Engine.Auditing
{
    using Game.Engine.Core;

    public class AuditEventSpawn : AuditEventBase
    {
        public AuditModelPlayer Player { get; set; }

        public AuditEventSpawn(Player player)
            :base(player?.World)
        {
            Player = new AuditModelPlayer(player);
        }
    }
}
