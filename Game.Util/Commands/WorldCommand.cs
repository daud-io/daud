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


            protected async override Task ExecuteAsync()
            {
                var map = new TmxMap(File);
                var groundLayer = map.Layers.FirstOrDefault(l => l.Name == "Ground");

                var mapOffset = new Vector2(-map.Width/2 * Size, -map.Height/2 * Size);

                
                if (groundLayer != null)
                {
                    var tileSet = map.Tilesets[0];


                    var tileModels = groundLayer.Tiles.Select(t =>
                    {

                        var tile = tileSet.Tiles[t.Gid-1];

                        return new MapTileModel
                        {
                            Position = new Vector2(t.X * Size, t.Y * Size) + mapOffset,
                            Size = Size,
                            TileGridID = t.Gid - 1,
                            Type = tile.TerrainEdges.All(e => e.Name == "Water")
                                ? "deadly"
                                : null
                        };
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
