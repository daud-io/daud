namespace Game.Engine.Core.SystemActors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Numerics;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using Newtonsoft.Json;
    using SharpGLTF.Schema2;

    public class WorldMeshLoader : SystemActorBase
    {
        private static HttpClient httpLoader = new();
        private List<StaticHandle> Statics = new List<StaticHandle>();

        private bool DoubleTriangles = false;
        private string loadedURL;

        private Vector3 ScaleVector = Vector3.One;
        private Vector3 TranslationVector = Vector3.Zero;

        public WorldMeshLoader(World world) : base(world)
        {
            ConfigureMeshes();
            CycleMS = 1000;
        }

        private bool IsSpawnPoint(Node node)
        {
            return node.Name != null 
                && Regex.Match(node.Name, @"^([a-zA-Z]+?)_spawn").Success;
        }

        private void RegisterSpawnPoint(Node node, Vector2 point)
        {
            var match = Regex.Match(node.Name, @"^([a-zA-Z]+?)_spawn");
            if (match.Success)
            {
                var spawnType = match.Groups[1].Value;

                if (spawnType == "player")
                    spawnType = "fleet";
                
                List<Vector2> typeList;
                if (!World.SpawnPoints.TryGetValue(spawnType, out typeList))
                    World.SpawnPoints.Add(spawnType, typeList = new List<Vector2>());
                
                typeList.Add(point);
            }
        }

        private void LoadNode(ModelRoot root, Node node)
        {
            Matrix4x4 transform = 
                  node.WorldMatrix
                * Matrix4x4.CreateScale(new Vector3(1,1,-1)) // invert Z
                * Matrix4x4.CreateScale(this.ScaleVector); // scale

            if (node.Extras.Content != null)
            {
                var tags = JsonConvert.DeserializeObject<Dictionary<string, string>>(node.Extras.ToJson());
                if (tags != null)
                {
                    if (tags.TryGetValue("physics", out string physics))
                        if (physics == "false")
                            return;
                }
            }

            if (node.Name != null)
            {
                if (IsSpawnPoint(node))
                {
                    Vector3 point = Vector3.Transform(Vector3.Zero, transform);
                    RegisterSpawnPoint(node, new Vector2(point.X, point.Z));
                    return;
                }
            }

            if (node.Mesh?.Primitives != null)
                foreach (var primitive in node.Mesh.Primitives)
                {
                    var vertices = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
                    var trianglesIndices = primitive.GetTriangleIndices().ToArray();
                    World.BufferPool.Take<Triangle>(trianglesIndices.Length * (DoubleTriangles ? 2 : 1), out var triangles);
                    int i = 0;
                    foreach (var (a, b, c) in trianglesIndices)
                    {
                        triangles[i++] = new Triangle(
                            Vector3.Transform(vertices[a], transform),
                            Vector3.Transform(vertices[b], transform),
                            Vector3.Transform(vertices[c], transform)
                        );

                        if (DoubleTriangles)
                            triangles[i++] = new Triangle(
                                Vector3.Transform(vertices[c], transform),
                                Vector3.Transform(vertices[b], transform),
                                Vector3.Transform(vertices[a], transform)
                            );
                    }

                    var mesh = new BepuPhysics.Collidables.Mesh(triangles, new Vector3(1, 1, 1), World.BufferPool);
                    //World.BufferPool.Return(ref triangles);

                    Statics.Add(World.Simulation.Statics.Add(
                        new StaticDescription(
                            this.TranslationVector,
                            new CollidableDescription(World.Simulation.Shapes.Add(mesh), 200f))));
                }

            foreach (var child in node.VisualChildren)
                LoadNode(root, child);
        }

        public Task<FileStream> GetMeshStreamAsync(string id)
        {
            return Task.FromResult(File.OpenRead(this.loadedURL));
        }

        private void LoadScene(ModelRoot root, Scene scene)
        {
            Vector3 min, max;
            min = max = Vector3.Zero;

            foreach (var child in scene.VisualChildren)
                LoadNode(root, child);
        }

        private void LoadGLB(string newURL)
        {
            this.loadedURL = newURL;
            httpLoader.GetStreamAsync(newURL).ContinueWith(t => {
                using var glbStream = t.Result;
                using var ms = new MemoryStream();
                    glbStream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                var root = SharpGLTF.Schema2.ModelRoot.ReadGLB(ms);
                foreach (var scene in root.LogicalScenes)
                    LoadScene(root, scene);
            });
            
        }

        private void UnloadMeshes()
        {
            foreach (var obj in Statics)
                World.Simulation.Statics.Remove(obj);

            Statics.Clear();
            World.SpawnPoints.Clear();
            this.loadedURL = null;
        }

        private void ConfigureMeshes()
        {
            var hook = World.Hook;
            if (World.Hook.Mesh.Enabled)
            { 
                var newURL = World.Hook.Mesh.MeshURL;
                
                if (newURL != loadedURL)
                {
                    this.UnloadMeshes();
                    this.ScaleVector = World.Hook.Mesh.Scale;
                    this.TranslationVector = World.Hook.Mesh.Translation;
                    LoadGLB(newURL);
                }
            }
            else if (this.loadedURL != null)
                this.UnloadMeshes();
        }

        protected override void CycleThink()
        {
            ConfigureMeshes();
        }
    }
}