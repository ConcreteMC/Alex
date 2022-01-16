using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using ResourcePackLib.Core.Data;
using ResourcePackLib.Core.Json.Converters;
using ResourcePackLib.ModelExplorer.Abstractions;
using RocketUI;
using Vector3 = System.Numerics.Vector3;

namespace ResourcePackLib.ModelExplorer.Entities;

public class MCEntityGeometryBoneCube
{
    [JsonConverter(typeof(Vector3ArrayJsonConverter))]
    public Vector3 Origin { get; set; }
    
    [JsonConverter(typeof(Vector3ArrayJsonConverter))]
    public Vector3 Size { get; set; }
    
    [JsonConverter(typeof(Vector2ArrayJsonConverter))]
    public Vector2 UV { get; set; }
}
public class MCEntityGeometryBone
{
    public string Name { get; set; }
    public string Parent { get; set; }
    
    [JsonConverter(typeof(Vector3ArrayJsonConverter))]
    public Vector3 Pivot { get; set; }
    
    [JsonConverter(typeof(Vector3ArrayJsonConverter))]
    public Vector3? BindPoseRotation { get; set; }
    
    public MCEntityGeometryBoneCube[] Cubes { get; set; }
}
public class MCEntityGeometry
{
    public int TextureWidth { get; set; }
    public int TextureHeight { get; set; }
    public MCEntityGeometryBone[] Bones { get; set; }
}


public class TruMesh
{
    
}

public class TruBone
{
    public string Name { get; }
    public TruBone Parent { get; set; }
    
    public Transform3D Transform { get; } = new Transform3D();
    
    public TruBone(string name)
    {
        Name = name;
    }
}

public class TruModel
{
    public string Name { get; }
    public List<TruMesh> Meshes { get; }
    public List<TruBone> Bones { get; }
    public Transform3D Transform { get; } = new Transform3D();

    public TruModel(string name)
    {
        Name = name;
    }
}

public class MCEntity : DrawableEntity
{
    private MCEntityGeometry Geometry { get; set; }
    
    
    public MCEntity(IGame game) : base(game)
    {
    }

    private readonly List<IDisposable> _disposables = new List<IDisposable>();
    protected override void LoadContent()
    {
        base.LoadContent();

        var entityDef = "fox.geo.json";
        var defPath = Path.Join("S:\\Temp\\resource_packs\\bedrock-1.18.1\\models\\entity\\", entityDef);
        Geometry = JsonConvert.DeserializeObject<MCEntityGeometry>(File.ReadAllText(defPath));

        var north = Game.Content.Load<Texture2D>("blocks/face_north"); _disposables.Add(north);
        var east = Game.Content.Load<Texture2D>("blocks/face_east");_disposables.Add(east);
        var south = Game.Content.Load<Texture2D>("blocks/face_south");_disposables.Add(south);
        var west = Game.Content.Load<Texture2D>("blocks/face_west");_disposables.Add(west);
        var up = Game.Content.Load<Texture2D>("blocks/face_up");_disposables.Add(up);
        var down = Game.Content.Load<Texture2D>("blocks/face_down");_disposables.Add(down);
        var textures = new[]
        {
            east,
            up,
            south,
            west,
            down,
            north
        };

        var cuboid = new Cuboid(Vector3.Zero, Vector3.One);
        
        var vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionTexture.VertexDeclaration, cuboid.Vertices.Length, BufferUsage.None);_disposables.Add(vertexBuffer);
        var indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, cuboid.Indices.Length, BufferUsage.None);_disposables.Add(indexBuffer);

        var vertexPositionTextures = cuboid.Faces.SelectMany(f =>
        {
            return new[]
            {
                new VertexPositionTexture(cuboid.Vertices[f.VertexOffset + 0], new Vector2(0, 0)),
                new VertexPositionTexture(cuboid.Vertices[f.VertexOffset + 1],new Vector2(1, 0)),
                new VertexPositionTexture(cuboid.Vertices[f.VertexOffset + 2], new Vector2(1, 1)),
                new VertexPositionTexture(cuboid.Vertices[f.VertexOffset + 3], new Vector2(0, 1)),
            };
        }).ToArray();
        vertexBuffer.SetData(vertexPositionTextures);
        indexBuffer.SetData(cuboid.Indices);

        var meshes = new List<ModelMesh>();
        var bones = new List<ModelBone>();
        var parts = new List<ModelMeshPart>();

        foreach(var face in cuboid.Faces) 
        {
            var meshpart = new ModelMeshPart()
            {
                VertexBuffer = vertexBuffer,
                NumVertices = 4,
                IndexBuffer = indexBuffer,
                VertexOffset = 0,

                PrimitiveCount = 2,
                StartIndex = face.IndexOffset,
            };
            parts.Add(meshpart);
        }

        var bone = new ModelBone();
        bone.Transform = Matrix.Identity;
        bone.ModelTransform = Matrix.Identity;
        var modelmesh = new ModelMesh(GraphicsDevice, parts);
        
        modelmesh.ParentBone = bone;
        meshes.Add(modelmesh);

        bone.AddMesh(modelmesh);
        bones.Add(bone);
        var model = new Model(GraphicsDevice, bones, meshes)
        {
            Root = bone
        };
        
        for (int i = 0; i < 6; i++)
        {
            var effect = new BasicEffect(GraphicsDevice)
            {
                Texture = textures[i],
                TextureEnabled = true,
                LightingEnabled = true,
                VertexColorEnabled = true,
            };
            effect.EnableDefaultLighting();
            modelmesh.MeshParts[i].Effect = effect;
        }

        Model = model;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var d in _disposables.ToArray())
        {
            d.Dispose();
            _disposables.Remove(d);
        }
    }
}