namespace Game.Util.Commands
{
    using Game.API.Common.Models;
    using McMaster.Extensions.CommandLineUtils;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using TiledSharp;

    [Subcommand("shrink", typeof(Shrink))]
    [Subcommand("parse", typeof(Parse))]
    class WorldCommand : CommandBase
    {
        class Parse : CommandBase
        {
            [Argument(0)]
            public string WorldKey { get; set; }

            [Argument(1)]
            public string File { get; set; } = null;

            [Option]
            public int Size { get; set; } = 100;

            [Option]
            public bool Clear { get; set; } = false;


            protected async override Task ExecuteAsync()
            {

                if (Clear)
                {
                    await API.World.SetMapTiles(WorldKey, new MapTileModel[0]);
                    return;
                }

                var map = new TmxMap(File);
                var groundLayer = map.Layers.FirstOrDefault(l => l.Name == "Ground");

                var spawnLocation = map.ObjectGroups.SelectMany(g => g.Objects).FirstOrDefault(o => o.Type == "SpawnPoint");
                if (spawnLocation != null)
                {
                    await API.World.HookAsync(new
                    {
                        SpawnLocation = spawnLocation
                    });
                }

                var mapOffset = new Vector2(-(map.Width * Size) / 2, -(map.Height * Size)/2);

                
                if (groundLayer != null)
                {
                    var tileSet = map.Tilesets[0];


                    var tileModels = groundLayer.Tiles.Select(t =>
                    {
                        var gridID = t.Gid - tileSet.FirstGid;

                        var tile = tileSet.Tiles.ContainsKey(gridID)
                            ? tileSet.Tiles[gridID]
                            : null;

                        var mapTileModel = new MapTileModel
                        {
                            Position = new Vector2(t.X * Size, t.Y * Size) + mapOffset,
                            Size = Size/2,
                            TileGridID = gridID
                        };

                        Console.WriteLine($"grid: {gridID}");

                        if (tile != null)
                        {

                            if (tile.TerrainEdges.All(e => e?.Name == "Water"))
                                mapTileModel.Type = "deadly";

                            if (tile.TerrainEdges.Any(e => e?.Name == "Dirt"))
                                mapTileModel.Type = "obstacle";
                            if (tile.TerrainEdges.Any(e => e?.Name == "Dark Dirt"))
                                mapTileModel.Type = "obstacle";

                            if (tile.Properties.ContainsKey("drag"))
                                mapTileModel.Drag = float.Parse(tile.Properties["drag"]);

                        }

                        return mapTileModel;
                    });

                    await API.World.SetMapTiles(WorldKey, tileModels);
                }
            }
        }


        class Shrink : CommandBase
        {
            [Option]
            public string World { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                for (int i = 4200; i > 0; i -= 10)
                {
                    await API.Server.HookAsync(new { WorldSize = i }, World);
                    await Task.Delay(100);
                }
            }
        }
    }
}
