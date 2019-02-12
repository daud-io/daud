namespace Game.Engine.Core.Maps
{
    using Game.Engine.Core.Weapons;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class TileTurret : TileBase, ICollide
    {
        private long ShotCooldown = 0;
        private List<ShipWeaponBullet> NewBullets = new List<ShipWeaponBullet>();

        public bool IsCollision(Body projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
                if (bullet.Group != WorldMap.WeaponGroup)
                    return Vector2.Distance(projectedBody.Position, Position) < projectedBody.Size + Size;

            return false;
        }

        public void CollisionExecute(Body projectedBody)
        {
            if (projectedBody is ShipWeaponBullet bullet)
            {
                this.Sprite += 1;
            }
        }

        public override void Think()
        {
            if (ShotCooldown < World.Time)
            {
                var target = World.BodiesNear(this.Position, 2500)
                    .OfType<Ship>()
                    .Where(s => s.Abandoned == false)
                    .Where(s => s.Sprite != API.Common.Sprites.fish)
                    .FirstOrDefault();

                if (target != null)
                {
                    var bullet = new ShipWeaponBullet();
                    var toTarget = target.Position - Position;
                    bullet.FireFrom(this, MathF.Atan2(toTarget.Y, toTarget.X));

                    NewBullets.Add(bullet);

                    ShotCooldown = World.Time + 300;
                }
            }
        }

        public override void CreateDestroy()
        {
            base.CreateDestroy();

            foreach (var bullet in NewBullets)
                bullet.Init(World);

            NewBullets.Clear();
        }
    }
}
