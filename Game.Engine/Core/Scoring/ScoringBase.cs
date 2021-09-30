﻿using Game.API.Common.Models.Auditing;
using Game.Engine.Auditing;

namespace Game.Engine.Core.Scoring
{
    public abstract class ScoringBase
    {
        public virtual void ShipDied(Player killer, Player victim, Ship ship)
        {

        }

        public virtual void FleetDied(Player killer, Player victim, Fleet fleet)
        {
            /*RemoteEventLog.SendEvent(new AuditEventDeath
            {
                Killer = killer?.ToAuditModelPlayer(),
                Victim = victim?.ToAuditModelPlayer()
            }, fleet.World);*/
        }
    }
}
