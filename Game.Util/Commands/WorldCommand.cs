namespace Game.Util.Commands
{
    using Game.API.Common.Models;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using TiledSharp;

    [Subcommand("shrink", typeof(Shrink))]
    [Subcommand("hook", typeof(Hook))]
    [Subcommand("create", typeof(Create))]
    [Subcommand("parse", typeof(Parse))]
    [Subcommand("delete", typeof(Delete))]
    class WorldCommand : CommandBase
    {
        class Create : CommandBase
        {
            [Argument(0)]
            public string WorldKey { get; set; }

            [Argument(1)]
            public string HookJSON { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                var hook = JsonConvert.DeserializeObject(HookJSON);
                hook = await API.World.PutWorldAsync(WorldKey, hook);

                Console.WriteLine(hook);
            }
        }

        class Delete : CommandBase
        {
            [Argument(0)]
            public string WorldKey { get; set; }

            protected async override Task ExecuteAsync()
            {
                await API.World.DeleteWorldAsync(WorldKey);
            }
        }

        class Hook : CommandBase
        {
            [Option]
            public string World { get; set; } = null;

            [Argument(0)]
            public string HookJSON { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                var hook = JsonConvert.DeserializeObject(HookJSON);
                hook = await API.World.PostHookAsync(hook, World);

                Console.WriteLine(hook);
            }
        }


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

                var spawnLocation =
                    map.ObjectGroups.SelectMany(g => g.Objects)
                        .Where(o => o.Type == "SpawnPoint")
                        .Select(p => new Vector2((float)p.X, (float)p.Y))
                        .FirstOrDefault();

                var mapOffset = new Vector2(-(map.Width * Size) / 2, -(map.Height * Size)/2);


                if (spawnLocation != null)
                    await API.World.PostHookAsync(new
                    {
                        SpawnLocation = spawnLocation/map.TileWidth * Size + mapOffset
                    }, WorldKey);

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
                            {
                                mapTileModel.Type = "turret";
                            }

                            if (tile.TerrainEdges.All(e => e?.Name == "Obstacle"))
                                mapTileModel.Type = "obstacle";

                            if (tile.TerrainEdges.Any(e => e?.Name == "Bouncy"))
                                mapTileModel.Type = "bouncy";

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
                    await API.World.PostHookAsync(new { WorldSize = i }, World);
                    await Task.Delay(100);
                }
            }
        }
    }
}
