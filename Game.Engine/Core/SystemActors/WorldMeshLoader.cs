namespace Game.Engine.Core.SystemActors
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Numerics;
    using BepuPhysics;
    using BepuPhysics.Collidables;
    using ObjLoader.Loader.Loaders;

    public class WorldMeshLoader : SystemActorBase
    {

        public WorldMeshLoader(World world) : base(world)
        {
            LoadGLB();
        }
        private void LoadOBJ()
        {
            using (var file = File.OpenRead("wwwroot/public/assets/base/models/grid.obj"))
            {
                var meshContent = Build(file);
                
                World.BufferPool.Take<Triangle>(meshContent.Triangles.Length, out var triangles);
                for (int i = 0; i < meshContent.Triangles.Length; ++i)
                {
                    // invert normals? reverse order of A,B,C
                    triangles[i] = new Triangle(meshContent.Triangles[i].C, meshContent.Triangles[i].B, meshContent.Triangles[i].A);
                }
                var mesh = new Mesh(triangles, new Vector3(1,1,1), World.BufferPool);

                World.Simulation.Statics.Add(
                    new StaticDescription(
                        new Vector3(0, -500, 0),
                        new CollidableDescription(World.Simulation.Shapes.Add(mesh), 10f)));
            }            
        }


        private void LoadGLB()
        {
            var root = SharpGLTF.Schema2.ModelRoot.Load($"wwwroot/public/assets/base/models/partycity.glb");

            foreach(var logicialMesh in root.LogicalMeshes)
            {
                foreach (var primitive in logicialMesh.Primitives)
                {
                    var vertices = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
                    var trianglesIndices = primitive.GetTriangleIndices().ToArray();
                    World.BufferPool.Take<Triangle>(trianglesIndices.Length * 2, out var triangles);
                    int i=0;
                    foreach (var (a,b,c) in trianglesIndices)
                    {
                        triangles[i] = new Triangle(vertices[a],vertices[b],vertices[c]);
                        triangles[i].A.X *= -1; 
                        triangles[i].B.X *= -1; 
                        triangles[i].C.X *= -1; 

                        i++;
                    }

                    var mesh = new Mesh(triangles, new Vector3(1,1,1), World.BufferPool);

                    World.Simulation.Statics.Add(
                        new StaticDescription(
                            new Vector3(0, -500, 0),
                            new CollidableDescription(World.Simulation.Shapes.Add(mesh), 0.1f)));
                }
            }
        }

        protected override void CycleThink()
        {
            var hook = World.Hook;
            if (World.Hook.WorldMesh != null)
            {


            }
        }

        class MaterialStubLoader : IMaterialStreamProvider
        {
            public Stream Open(string materialFilePath)
            {
                return null;
            }
        }


        public unsafe static MeshContent Build(Stream dataStream)
        {
            
            var result = new ObjLoaderFactory().Create(new MaterialStubLoader()).Load(dataStream);
            var triangles = new List<TriangleContent>();
            for (int i = 0; i < result.Groups.Count; ++i)
            {
                var group = result.Groups[i];
                for (int j = 0; j < group.Faces.Count; ++j)
                {
                    var face = group.Faces[j];
                    var a = result.Vertices[face[0].VertexIndex - 1];
                    for (int k = 1; k < face.Count - 1; ++k)
                    {
                        var b = result.Vertices[face[k].VertexIndex - 1];
                        var c = result.Vertices[face[k + 1].VertexIndex - 1];
                        triangles.Add(new TriangleContent
                        {
                            A = new Vector3(a.X, a.Y, a.Z),
                            B = new Vector3(b.X, b.Y, b.Z),
                            C = new Vector3(c.X, c.Y, c.Z)
                        });
                    }
                }
            }
            return new MeshContent(triangles.ToArray());
        }

        public class MeshContent
        {
            public TriangleContent[] Triangles;

            public MeshContent(TriangleContent[] triangles)
            {
                Triangles = triangles;
            }
        }


        public struct TriangleContent
        {
            public Vector3 A;
            public Vector3 B;
            public Vector3 C;
        }


    }
}