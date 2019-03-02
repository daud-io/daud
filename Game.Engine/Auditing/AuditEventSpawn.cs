namespace Game.Engine.Auditing
{
    using Game.Engine.Core;

    public class AuditEventSpawn : AuditEventBase
    {
        public long GameTime { get; set; }
        public AuditModelPlayer Player { get; set; }

        public AuditEventSpawn(Player player)
        {
            GameTime = player?.World?.Time ?? 0;
            Player = new AuditModelPlayer(player);
        }
    }
}
