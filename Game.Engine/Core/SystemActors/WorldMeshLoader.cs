namespace Game.Engine.Core.SystemActors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using SharpGLTF.Schema2;

    public class WorldMeshLoader : SystemActorBase
    {
        private List<StaticHandle> Statics = new List<StaticHandle>();
        private bool DoubleTriangles = false;

        public WorldMeshLoader(World world) : base(world)
        {
            LoadGLB();            
        }

        private void LoadNode(ModelRoot root, Node node)
        {

            foreach (var child in node.VisualChildren)
                LoadNode(root, child);

            Matrix4x4 transform = 
                node.WorldMatrix
                * Matrix4x4.CreateScale(new Vector3(-10, 10, 10)); // invert X because reasons

            if (node.Mesh != null)
            {
                foreach (var primitive in node.Mesh.Primitives)
                {
                    var vertices = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
                    var trianglesIndices = primitive.GetTriangleIndices().ToArray();
                    World.BufferPool.Take<Triangle>(trianglesIndices.Length * (DoubleTriangles ? 2 : 1), out var triangles);
                    int i=0;
                    foreach (var (a,b,c) in trianglesIndices)
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

                    var mesh = new BepuPhysics.Collidables.Mesh(triangles, new Vector3(1,1,1), World.BufferPool);

                    Statics.Add(World.Simulation.Statics.Add(
                        new StaticDescription(
                            new Vector3(0, -500, 0),
                            new CollidableDescription(World.Simulation.Shapes.Add(mesh), 0.1f))));
                }
            }
        }

        private void LoadScene(ModelRoot root, Scene scene)
        {
            Vector3 min, max;
            min = max = Vector3.Zero;

            foreach (var child in scene.VisualChildren)
                LoadNode(root, child);
        }

        private void LoadGLB()
        {
            var root = SharpGLTF.Schema2.ModelRoot.Load($"wwwroot/dist/assets/base/models/ffa.glb");

            foreach (var scene in root.LogicalScenes)
                LoadScene(root, scene);
            
        }

        protected override void CycleThink()
        {
            var hook = World.Hook;
            if (World.Hook.WorldMesh != null)
            {

            }
        }
    }
}