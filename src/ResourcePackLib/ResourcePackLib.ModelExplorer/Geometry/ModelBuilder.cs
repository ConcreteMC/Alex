using System.Numerics;
using ResourcePackLib.Core.Data;

namespace ResourcePackLib.ModelExplorer.Geometry;

public struct ModelVertexIndexPositionTexture
{
    public Vector3 Position;
    public Vector2 Uv;

    public ModelVertexIndexPositionTexture(Vector3 position, Vector2 uv)
    {
        Position = position;
        Uv = uv;
    }
}
public class ModelCubeMeshBuilder
{
    private readonly ModelBoneBuilder _parentBone;
    private Vector3 _origin;
    private Vector3 _size;
    private Vector2 _uv;

    internal Cuboid _cuboid;

    public ModelCubeMeshBuilder(ModelBoneBuilder parentBone)
    {
        _parentBone = parentBone;
    }

    public ModelCubeMeshBuilder Origin(Vector3 origin)
    {
        _origin = origin;
        return this;
    }

    public ModelCubeMeshBuilder Size(Vector3 size)
    {
        _size = size;
        return this;
    }

    public ModelCubeMeshBuilder Uv(Vector2 uv)
    {
        _uv = uv;
        return this;
    }

    private (ModelVertexIndexPositionTexture[] vertices, short[] indices) CreateCuboid(Vector3 min, Vector3 max, Vector2 uvSize)
    {
        var baseVertices = new Vector3[8]; // 8 distinct coordinates

        // Binary notation represents 0bXYZ ;)
        baseVertices[0b000] = new Vector3(min.X, min.Y, min.Z); // 0 0 0 
        baseVertices[0b001] = new Vector3(min.X, min.Y, max.Z); // 0 0 1 
        baseVertices[0b010] = new Vector3(min.X, max.Y, min.Z); // 0 1 0 
        baseVertices[0b011] = new Vector3(min.X, max.Y, max.Z); // 0 1 1 
        baseVertices[0b100] = new Vector3(max.X, min.Y, min.Z); // 1 0 0 
        baseVertices[0b101] = new Vector3(max.X, min.Y, max.Z); // 1 0 1 
        baseVertices[0b110] = new Vector3(max.X, max.Y, min.Z); // 1 1 0 
        baseVertices[0b111] = new Vector3(max.X, max.Y, max.Z); // 1 1 1 

        short[][] baseFaceIndices =
        {
            new short[] { 0b111, 0b110, 0b100, 0b101 }, // East   | Right?
            new short[] { 0b010, 0b110, 0b111, 0b011 }, // Up     | Top?
            new short[] { 0b011, 0b111, 0b101, 0b001 }, // South  | Back?
            new short[] { 0b010, 0b011, 0b001, 0b000 }, // West   | Left?
            new short[] { 0b100, 0b000, 0b001, 0b101 }, // Down   | Bottom?
            new short[] { 0b110, 0b010, 0b000, 0b100 }, // North  | Front?
        };

        var vertices = new ModelVertexIndexPositionTexture[24];
        var indices = new short[36];
        short b = 0, c = 0;

        var size = max - min;
        var o1 = new Vector2((2 * size.Y) + (2 * size.X), (size.Y + size.Z));
        
        var defineFace = (short[] vertexIndices, Vector2 uvMin, Vector2 uvSize) =>
        {
            var uvMax = uvMin + uvSize;
            //for (int i = 0; i < baseFaceIndices.Length; i++)
            //{
            var d = b;
            vertices[b++] = new ModelVertexIndexPositionTexture(baseVertices[vertexIndices[0]], new Vector2(uvMin.X, uvMin.Y) / o1);
            vertices[b++] = new ModelVertexIndexPositionTexture(baseVertices[vertexIndices[1]], new Vector2(uvMax.X, uvMin.Y) / o1);
            vertices[b++] = new ModelVertexIndexPositionTexture(baseVertices[vertexIndices[2]], new Vector2(uvMax.X, uvMax.Y) / o1);
            vertices[b++] = new ModelVertexIndexPositionTexture(baseVertices[vertexIndices[3]], new Vector2(uvMin.X, uvMax.Y) / o1);

            indices[c++] = (short)(d + 0);
            indices[c++] = (short)(d + 1);
            indices[c++] = (short)(d + 2);
            indices[c++] = (short)(d + 0);
            indices[c++] = (short)(d + 2);
            indices[c++] = (short)(d + 3);
            //}
        };


        /* East | Right? */
        defineFace(new short[] { 0b111, 0b110, 0b100, 0b101 }, new Vector2(0f, size.Y), new Vector2(size.Y, size.Z));
        
        /* Up | Top? */
        defineFace(new short[] { 0b010, 0b110, 0b111, 0b011 }, new Vector2((2*size.Y)+size.X, size.Y), new Vector2(size.X, size.Z));
        
        /* South  | Back? */
        defineFace(new short[] { 0b011, 0b111, 0b101, 0b001 }, new Vector2(size.Y+size.X, 0f), new Vector2(size.X, size.Y));
        
        /*  West   | Left? */
        defineFace(new short[] { 0b010, 0b011, 0b001, 0b000 }, new Vector2(size.Y+size.X, size.Y), new Vector2(size.Y, size.Z));
        
        /* Down   | Bottom? */
        defineFace(new short[] { 0b100, 0b000, 0b001, 0b101 }, new Vector2(size.Y, size.Y), new Vector2(size.X, size.Z));
        
        /* North  | Front? */
        defineFace(new short[] { 0b110, 0b010, 0b000, 0b100 }, new Vector2(size.Y, 0f), new Vector2(size.X, size.Y));

        return (vertices, indices);
    }

    public TruModelMesh Build()
    {
        var cuboid = new Cuboid(_origin, _origin + _size);
        _cuboid = cuboid;

        return new TruModelMesh()
        {
        };
    }
}

public class ModelBoneBuilder
{
    private readonly ModelBuilder _modelBuilder;
    private readonly string _name;
    internal string _parentName;
    private Vector3 _pivot;
    private List<ModelCubeMeshBuilder> _cubeBuilders = new List<ModelCubeMeshBuilder>();
    internal TruModelBone _built;

    public ModelBoneBuilder(ModelBuilder modelBuilder, string name)
    {
        _modelBuilder = modelBuilder;
        _name = name;
    }

    public ModelBoneBuilder Parent(string parentName)
    {
        _parentName = parentName;
        return this;
    }

    public ModelBoneBuilder Pivot(Vector3 pivot)
    {
        _pivot = pivot;
        return this;
    }

    public ModelCubeMeshBuilder AddCube(Vector3 origin, Vector3 size)
    {
        var builder = new ModelCubeMeshBuilder(this);
        _cubeBuilders.Add(builder);

        return builder
            .Origin(origin)
            .Size(size);
    }


    internal TruModelBone Build()
    {
        var bone = new TruModelBone()
        {
            Name = _name,
        };

        foreach (var cube in _cubeBuilders)
        {
            bone.AddMesh(cube.Build());
        }

        bone.Transform.LocalRotationOrigin = _pivot;

        _built = bone;
        return bone;
    }
}

public class ModelBuilder
{
    private Vector2 _textureSize;
    private readonly List<ModelBoneBuilder> _boneBuilders = new List<ModelBoneBuilder>();

    public ModelBuilder()
    {
    }

    public ModelBuilder TextureSize(Vector2 size)
    {
        _textureSize = size;
        return this;
    }

    public ModelBoneBuilder AddBone(string name)
    {
        var builder = new ModelBoneBuilder(this, name);
        _boneBuilders.Add(builder);
        return builder;
    }

    public TruModel Build()
    {
        var bones = new List<TruModelBone>();

        foreach (var boneBuilder in _boneBuilders)
        {
            var bone = boneBuilder.Build();
            bones.Add(bone);
        }

        foreach (var boneBuilder in _boneBuilders)
        {
            if (!string.IsNullOrEmpty(boneBuilder._parentName))
            {
                // try resolve parent
                foreach (var bone in bones)
                {
                    if (string.Equals(bone.Name, boneBuilder._parentName, StringComparison.OrdinalIgnoreCase))
                    {
                        boneBuilder._built.Parent = bone;
                        break;
                    }
                }
            }
        }

        var model = new TruModel();
        return model;
    }
}