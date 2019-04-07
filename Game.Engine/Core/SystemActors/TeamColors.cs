﻿namespace Game.Engine.Core.SystemActors
{
    public class TeamColors : SystemActorBase
    {
        protected override void CycleThink()
        {
            if (World.Hook.TeamMode)
            {
                foreach (var player in Player.GetWorldPlayers(World))
                    switch (player.ShipSprite)
                    {
                        case API.Common.Sprites.ship0:
                        case API.Common.Sprites.ship_green:
                        case API.Common.Sprites.ship_yellow:
                        case API.Common.Sprites.ship_secret:
                            player.ShipSprite = API.Common.Sprites.ship_cyan;
                            player.Color = "cyan";
                            break;
                        case API.Common.Sprites.ship_orange:
                        case API.Common.Sprites.ship_pink:
                        case API.Common.Sprites.ship_red:
                        case API.Common.Sprites.ship_zed:
                            player.ShipSprite = API.Common.Sprites.ship_red;
                            player.Color = "red";
                            break;
                    }
            }
        }
    }
}