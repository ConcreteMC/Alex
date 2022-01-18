using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using ResourcePackLib.Core.Data;
using ResourcePackLib.Core.Json.Converters;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Geometry;
using ResourcePackLib.ModelExplorer.Utilities.Extensions;
using RocketUI;
using Vector2 = System.Numerics.Vector2;
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

public class MCEntityGeometryCollection
{
    [JsonProperty("format_version")] public string FormatVersion { get; set; }

    [JsonExtensionData] public Dictionary<string, JToken> Geometries { get; set; } = new Dictionary<string, JToken>();

    public MCEntityGeometry GetGeometry(string name)
    {
        if (Geometries.TryGetValue(name, out var gToken))
        {
            var jsonSerializer = JsonSerializer.CreateDefault();

            using (var reader = gToken.CreateReader())
            {
                return jsonSerializer.Deserialize<MCEntityGeometry>(reader);
            }
        }

        return null;
    }
}

public static class MCEntityModelBuilder
{
    public static TruModel BuildEntityModel(MCEntityGeometry geometry)
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.TextureSize(new Vector2(geometry.TextureWidth, geometry.TextureHeight));

        foreach (var bone in geometry.Bones)
        {
            var bb = modelBuilder
                .AddBone(bone.Name)
                .Pivot(bone.Pivot);
            
            if (bone.BindPoseRotation.HasValue)
                bb.BindPoseRotation(bone.BindPoseRotation.Value);

            if (!string.IsNullOrEmpty(bone.Parent))
                bb.Parent(bone.Parent);

            if (bone.Cubes.Length > 0)
            {
                foreach (var boneCube in bone.Cubes)
                {
                    bb.AddCube(boneCube.Origin, boneCube.Size)
                        .Uv(boneCube.UV);
                }
            }
        }

        return modelBuilder.Build();
    }

    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    public static void ResetBonePositions(TruModel model)
    {
        foreach (var bone in model.Bones)
        {
            bone.Transform.LocalPosition = Microsoft.Xna.Framework.Vector3.Zero;
        }
    }
    
    public static void LogBoneLocations(TruModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Bone Name,Parent,MinX,MinY,MinZ,MaxX,MaxY,MaxZ");


        foreach (var bone in model.Bones)
        {
            var wlrd = bone.Transform.World;
            
            if(!bone.Meshes.Any()) continue;
            
            var boneMinMesh = bone.Meshes.SelectMany(m =>
                    Enumerable.Range(m.VertexOffset, m.NumVertices)
                        .Select(i => model.Vertices[i])
                )
                .Select(v =>
                    Microsoft.Xna.Framework.Vector3.Transform(v.Position, wlrd))
                .OrderBy(x => x.LengthSquared())
                .ToArray();
            var boneMin = boneMinMesh.FirstOrDefault();
            var boneMax = boneMinMesh.LastOrDefault();
            sb.AppendLine($"{bone.Name},{bone.Parent?.Name},{boneMin.X},{boneMin.Y},{boneMin.Z},{boneMax.X},{boneMax.Y},{boneMax.Z}");
        }
        
        Log.Info($"\n\n{sb.ToString()}\n\n");
        File.WriteAllText("bonedebug.csv", sb.ToString());
    }
}

public class MCEntity : DrawableEntity
{
    private MCEntityGeometry Geometry { get; set; }
    private TruModel TruModel { get; set; }

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;

    public MCEntity(IGame game) : base(game)
    {
    }

    private readonly List<IDisposable> _disposables = new List<IDisposable>();

    protected override void LoadContent()
    {
        base.LoadContent();

        var entityDef = "creeper.geo.json";
        var entityTex = "creeper\\creeper.png";
        var defPath = Path.Join("S:\\Temp\\resource_packs\\bedrock-1.18.1\\models\\entity\\", entityDef);
        var texPath = Path.Join("S:\\Temp\\resource_packs\\bedrock-1.18.1\\textures\\entity\\", entityTex);
        var json = File.ReadAllText(defPath);
        var geometryCollection = JsonConvert.DeserializeObject<MCEntityGeometryCollection>(json);

        Geometry = geometryCollection.GetGeometry(geometryCollection.Geometries.Keys.FirstOrDefault());

        var model = MCEntityModelBuilder.BuildEntityModel(Geometry);

        _vertexBuffer = new VertexBuffer(GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, model.Vertices.Length, BufferUsage.None);
        _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, model.Indices.Length, BufferUsage.None);

        _vertexBuffer.SetData(model.Vertices);
        _indexBuffer.SetData(model.Indices);

        _effect = new BasicEffect(GraphicsDevice);

        var texture = Texture2D.FromFile(GraphicsDevice, texPath);;
        //var texture = new ColorTexture2D(GraphicsDevice, Color.HotPink);
        _effect.Texture = texture;
        _effect.TextureEnabled = true;
        _effect.LightingEnabled = false;
        _effect.VertexColorEnabled = true;
        //_effect.EnableDefaultLighting();

        //_effect.DiffuseColor = Color.OrangeRed.ToVector3();
        //_effect.Alpha = 0.75f;

        _disposables.Add(_effect);
        _disposables.Add(texture);
        _disposables.Add(_vertexBuffer);
        _disposables.Add(_indexBuffer);

        TruModel = model;
        TruModel.Transform.ParentTransform = Transform;
    }

    public override void Draw(GameTime gameTime)
    {
        if (!Visible) return;

        DrawTruModel(TruModel);
    }

    private BasicEffect _effect;

    private void DrawTruModel(TruModel model)
    {
        if (model != null)
        {
            using (var cxt = GraphicsContext.CreateContext(GraphicsDevice, BlendState.AlphaBlend, DepthStencilState.Default))
            {
                var r = cxt.RasterizerState.Copy();
                r.FillMode = FillMode.Solid;
                //r.CullMode = CullMode.None;
                //r.DepthClipEnable = false;
                cxt.RasterizerState = r;

                cxt.SamplerState = SamplerState.PointWrap;
                var camera = ((IGame)Game).Camera;

                _effect.View = camera.View;
                _effect.Projection = camera.Projection;
                //_effect.DiffuseColor = Vector3.One;
                //_effect.LightingEnabled = false;
                //_effect.Alpha = 1f;
                GraphicsDevice.SetVertexBuffer(_vertexBuffer);
                GraphicsDevice.Indices = _indexBuffer;

                foreach (var bone in model.Bones)
                {
                    //_effect.World = Transform.World;
                    _effect.World = bone.Transform.World;

                    // if (bone.Name.Contains("body", StringComparison.OrdinalIgnoreCase))
                    // {
                    //     _effect.DiffuseColor = Color.Blue.ToVector3();
                    // }
                    // else if (bone.Name.Contains("head", StringComparison.OrdinalIgnoreCase))
                    // {
                    //     _effect.DiffuseColor = Color.Red.ToVector3();
                    // }
                    // else
                    // {
                    //     _effect.DiffuseColor = Microsoft.Xna.Framework.Vector3.One;
                    // }

                    foreach (var mesh in bone.Meshes)
                    {
                        for (var i = 0; i < _effect.CurrentTechnique.Passes.Count; i++)
                        {
                            _effect.CurrentTechnique.Passes[i].Apply();
                            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, mesh.IndexOffset, mesh.PrimitiveCount);
                        }
                    }
                }
            }
        }
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