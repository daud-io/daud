namespace Game.Engine.Core.SystemActors
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using System.Threading.Tasks;
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using Newtonsoft.Json;
    using SharpGLTF.Schema2;

    public class WorldMeshLoader : SystemActorBase
    {
        private List<StaticHandle> Statics = new List<StaticHandle>();

        private bool DoubleTriangles = false;
        private string loadedURL;

        public WorldMeshLoader(World world) : base(world)
        {
            ConfigureMeshes();
            CycleMS = 1000;
        }

        private void LoadNode(ModelRoot root, Node node)
        {
            Matrix4x4 transform = Matrix4x4.Identity
                * Matrix4x4.CreateScale(new Vector3(1,1,-1))
                * node.WorldMatrix
                * Matrix4x4.CreateScale(new Vector3(10, 10, 10));

            if (node.Extras.Content != null)
            {
                var tags = JsonConvert.DeserializeObject<Dictionary<string, string>>(node.Extras.ToJson());
                if (tags != null)
                    if (tags.TryGetValue("physics", out string physics))
                        if (physics == "false")
                            return;
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
                            new Vector3(0, -500, 0),
                            new CollidableDescription(World.Simulation.Shapes.Add(mesh), 0.1f))));
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
            var root = SharpGLTF.Schema2.ModelRoot.Load(newURL);

            foreach (var scene in root.LogicalScenes)
                LoadScene(root, scene);
        }

        private void UnloadMeshes()
        {
            foreach (var obj in Statics)
                World.Simulation.Statics.Remove(obj);
            this.loadedURL = null;
        }

        private void ConfigureMeshes()
        {
            var hook = World.Hook;

            if (World.Hook.Mesh.Enabled)
            {
                var newURL = World.Hook.Mesh.MeshURL;
                if (newURL != loadedURL && newURL != $"{World.WorldKey}/server.glb")
                {
                    this.UnloadMeshes();
                    LoadGLB(newURL);
                    World.Hook.Mesh.MeshURL = $"{World.WorldKey}/server.glb";
                }
            }
        }

        protected override void CycleThink()
        {
            ConfigureMeshes();
        }
    }
}