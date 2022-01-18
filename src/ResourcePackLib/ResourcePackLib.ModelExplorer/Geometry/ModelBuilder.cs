using System.Numerics;
using ResourcePackLib.Core.Data;
using ResourcePackLib.ModelExplorer.Utilities.Extensions;
using Quaternion = Microsoft.Xna.Framework.Quaternion;

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
    internal readonly ModelBoneBuilder _parentBone;
    private Vector3 _origin;
    private Vector3 _size;
    internal Vector2 _uv;
    internal Vector2 _uvSize;

    internal (ModelVertexIndexPositionTexture[] vertices, short[] indices) _mesh;

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

    private (ModelVertexIndexPositionTexture[] vertices, short[] indices) CreateCuboid(Vector3 min, Vector3 max)
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
        _uvSize = new Vector2((2 * size.Y) + (2 * size.X), (size.Y + size.Z));
        var uvOffset = _uv;
        var globUvSize = _parentBone._modelBuilder._textureSize;
        
        var defineFace = (short[] vertexIndices, Vector2 uvMin, Vector2 uvSize) =>
        {
            var uvMax = uvMin + uvSize;
            //for (int i = 0; i < baseFaceIndices.Length; i++)
            //{
            var d = b;
            vertices[b++] = new ModelVertexIndexPositionTexture(baseVertices[vertexIndices[0]], (uvOffset + new Vector2(uvMin.X, uvMin.Y)) / globUvSize);
            vertices[b++] = new ModelVertexIndexPositionTexture(baseVertices[vertexIndices[1]], (uvOffset + new Vector2(uvMax.X, uvMin.Y)) / globUvSize);
            vertices[b++] = new ModelVertexIndexPositionTexture(baseVertices[vertexIndices[2]], (uvOffset + new Vector2(uvMax.X, uvMax.Y)) / globUvSize);
            vertices[b++] = new ModelVertexIndexPositionTexture(baseVertices[vertexIndices[3]], (uvOffset + new Vector2(uvMin.X, uvMax.Y)) / globUvSize);

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
        _mesh = CreateCuboid(_origin, _origin+_size);
        return new TruModelMesh()
        {
            NumVertices = _mesh.vertices.Length,
            PrimitiveCount = _mesh.indices.Length/3,
            IndexOffset = 0,
            VertexOffset = 0
        };
    }
}

public class ModelBoneBuilder
{
    internal readonly ModelBuilder _modelBuilder;
    private readonly string _name;
    internal string _parentName;
    private Vector3 _pivot;
    private Vector3 _bindPoseRotation;
    private List<ModelCubeMeshBuilder> _cubeBuilders = new List<ModelCubeMeshBuilder>();
    internal TruModelBone _built;
    public int _nVertices;
    public int _nIndices;
    internal (ModelVertexIndexPositionTexture[] vertices, short[] indices)[] _meshes;

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
    
    public ModelBoneBuilder BindPoseRotation(Vector3 bindPoseRotation)
    {
        _bindPoseRotation = bindPoseRotation;
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
        _nVertices = 0;
        _nIndices = 0;

        _meshes = new  (ModelVertexIndexPositionTexture[] vertices, short[] indices)[_cubeBuilders.Count];
        for (var i = 0; i < _cubeBuilders.Count; i++)
        {
            var cube = _cubeBuilders[i];
            var mesh = cube.Build();
            bone.AddMesh(mesh);
            _meshes[i] = cube._mesh;
            _nVertices += cube._mesh.vertices.Length;
            _nIndices += cube._mesh.indices.Length;
        }

        bone.Transform.LocalRotationOrigin = _pivot;
        bone.Transform.LocalRotation = _bindPoseRotation;

        _built = bone;
        return bone;
    }
}

public class ModelBuilder
{
    internal Vector2 _textureSize;
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

        var nVertices = 0;
        var nIndices = 0;
        
        foreach (var boneBuilder in _boneBuilders)
        {
            var bone = boneBuilder.Build();
            nVertices += boneBuilder._nVertices;
            nIndices += boneBuilder._nIndices;
            bones.Add(bone);
        }

        var vertices = new ModelVertexIndexPositionTexture[nVertices];
        var indices = new short[nIndices];

        var verticesIndex = 0;
        var indicesIndex = 0;
        
        for (int i = 0; i < _boneBuilders.Count; i++)
        {
            var bb = _boneBuilders[i];
            for (int j = 0; j < bb._meshes.Length; j++)
            {
                var bbm = bb._meshes[j];
                
                for (var k = 0; k < bbm.vertices.Length; k++)
                {
                    vertices[verticesIndex + k] = bbm.vertices[k];
                }
                
                for(var k = 0; k < bbm.indices.Length; k++)
                {
                    indices[indicesIndex + k] = (short)(verticesIndex + bbm.indices[k]);
                }

                var m = bb._built.Meshes[j];
                m.VertexOffset = verticesIndex;
                m.IndexOffset = indicesIndex;
                
                verticesIndex += bbm.vertices.Length;
                indicesIndex += bbm.indices.Length;
            }
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
                        bone.AddChild(boneBuilder._built);
                        break;
                    }
                }
            }
        }

        // var textureUvScale = 
        
        var model = new TruModel(vertices, indices, bones);
        return model;
    }
}