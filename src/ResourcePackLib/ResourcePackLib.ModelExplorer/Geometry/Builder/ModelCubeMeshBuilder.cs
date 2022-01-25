using System.Numerics;

namespace ResourcePackLib.ModelExplorer.Geometry;

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
        _uvSize = new Vector2((2 * size.Z) + (2 * size.X), (size.Y + size.Z));
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
        defineFace(new short[] { 0b111, 0b110, 0b100, 0b101 }, new Vector2(0f, size.Z), new Vector2(size.Z, size.Y));
        
        /* Up | Top? */
        defineFace(new short[] { 0b010, 0b110, 0b111, 0b011 }, new Vector2(size.Z, 0f), new Vector2(size.X, size.Z));
        
        /* South  | Back? */
        defineFace(new short[] { 0b011, 0b111, 0b101, 0b001 }, new Vector2((2*size.Z)+size.X, size.Z), new Vector2(size.X, size.Y));
        
        /*  West   | Left? */
        defineFace(new short[] { 0b010, 0b011, 0b001, 0b000 }, new Vector2(size.Z+size.X, size.Z), new Vector2(size.Z, size.Y));
        
        /* Down   | Bottom? */
        defineFace(new short[] { 0b100, 0b000, 0b001, 0b101 }, new Vector2(size.Z+size.X, 0f), new Vector2(size.X, size.Z));
        
        /* North  | Front? */
        defineFace(new short[] { 0b110, 0b010, 0b000, 0b100 }, new Vector2(size.Z, size.Z), new Vector2(size.X, size.Y));

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