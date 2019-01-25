namespace Game.Engine.Core.Maps
{
    using System.Collections.Generic;

    public class WorldMap : ActorGroup
    {
        private List<Tile> Tiles = new List<Tile>();

        public override void Init(World world)
        {
            base.Init(world);
            this.GroupType = API.Common.GroupTypes.Map;
            this.ZIndex = 255;
        }

        public override void CreateDestroy()
        {
            base.CreateDestroy();
        }

        public void DestroyTiles()
        {
            foreach (var tile in Tiles)
                tile.PendingDestruction = true;

            Tiles.Clear();
        }

        public void AddTile(Tile tile)
        {
            tile.Init(World);
            tile.Group = this;
            Tiles.Add(tile);
        }
    }
}
