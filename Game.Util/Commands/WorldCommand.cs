namespace Game.Util.Commands
{
    using Game.API.Common.Models;
    using McMaster.Extensions.CommandLineUtils;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
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
            public string World { get; set; }

            [Argument(1)]
            public string HookJSON { get; set; } = null;

            [Option]
            public string File { get; set; } = null;

            protected async override Task ExecuteAsync()
            {
                var hook = JsonConvert.DeserializeObject(HookJSON ?? System.IO.File.ReadAllText(File));
                hook = await API.World.PutWorldAsync(World, hook);

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
            [Argument(0)]
            public string World { get; set; }

            [Argument(1)]
            public string HookJSON { get; set; } = null;

            [Option]
            public string File { get; set; } = null;

            protected async override Task ExecuteAsync()
            {

                var hook = JsonConvert.DeserializeObject(HookJSON ?? System.IO.File.ReadAllText(File));
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
                    await API.World.SetMap(WorldKey, new MapModel
                    {
                        Rows = 0,
                        Columns = 0
                    });
                    return;
                }

                var map = new TmxMap(File);
                var groundLayer = map.Layers.FirstOrDefault(l => l.Name == "Ground");

                var spawnLocation =
                    map.ObjectGroups.SelectMany(g => g.Objects)
                        .Where(o => o.Type == "SpawnPoint")
                        .Select(p => new Vector2((float)p.X, (float)p.Y))
                        .FirstOrDefault();

                var mapOffset = new Vector2(-(map.Width * Size) / 2, -(map.Height * Size) / 2);

                if (spawnLocation != null)
                    await API.World.PostHookAsync(new
                    {
                        SpawnLocation = spawnLocation / map.TileWidth * Size + mapOffset
                    }, WorldKey);

                if (groundLayer != null)
                {
                    var tileSet = map.Tilesets[0];

                    var mapModel = new MapModel
                    {
                        Rows = map.Height,
                        Columns = map.Width,
                        TileSize = Size,
                        Tiles = groundLayer.Tiles.Select(t =>
                        {
                            var gridID = t.Gid - tileSet.FirstGid;

                            var tile = tileSet.Tiles.ContainsKey(gridID)
                                ? tileSet.Tiles[gridID]
                                : null;

                            var mapTileModel = new MapTileModel
                            {
                                Row = t.Y,
                                Column = t.X,
                                TileGridID = gridID
                            };

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
                        }).ToList()
                    };

                    await API.World.SetMap(WorldKey, mapModel);
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
