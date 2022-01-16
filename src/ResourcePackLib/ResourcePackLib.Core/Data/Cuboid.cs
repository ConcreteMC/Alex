using System.Numerics;

namespace ResourcePackLib.Core.Data;

public class CuboidFace
{
    public CubeFace Face { get; }
    public short[] Indices { get; }
    public short VertexOffset { get; }
    public short IndexOffset { get; }

    public CuboidFace(CubeFace face, short[] indices, short vertexOffset, short indexOffset)
    {
        Face = face;
        Indices = indices;
        VertexOffset = vertexOffset;
        IndexOffset = indexOffset;
    }
}

public class Cuboid
{
    public Vector3[] Vertices { get; }
    public short[] Indices { get; }
    public CuboidFace[] Faces { get; }

    public Cuboid(Vector3[] vertices, short[] indices)
    {
        Vertices = vertices;
        Indices = indices;
    }
    
    public Cuboid(Vector3 min, Vector3 max)
    {
        
        var baseVertices = new Vector3[8];  // 8 distinct coordinates

        // Binary notation represents 0bXYZ ;)
        baseVertices[0b000] = new Vector3(min.X, min.Y, min.Z); // 0 0 0 
        baseVertices[0b001] = new Vector3(min.X, min.Y, max.Z); // 0 0 1 
        baseVertices[0b010] = new Vector3(min.X, max.Y, min.Z); // 0 1 0 
        baseVertices[0b011] = new Vector3(min.X, max.Y, max.Z); // 0 1 1 
        baseVertices[0b100] = new Vector3(max.X, min.Y, min.Z); // 1 0 0 
        baseVertices[0b101] = new Vector3(max.X, min.Y, max.Z); // 1 0 1 
        baseVertices[0b110] = new Vector3(max.X, max.Y, min.Z); // 1 1 0 
        baseVertices[0b111] = new Vector3(max.X, max.Y, max.Z); // 1 1 1 

        var baseFaceIndices = new short[][]
        {
            new short[] { 0b111, 0b110, 0b100, 0b101 },
            new short[] { 0b010, 0b110, 0b111, 0b011 },
            new short[] { 0b011, 0b111, 0b101, 0b001 },
            new short[] { 0b010, 0b011, 0b001, 0b000 },
            new short[] { 0b100, 0b000, 0b001, 0b101 },
            new short[] { 0b110, 0b010, 0b000, 0b100 },
        };

        var vertices = new Vector3[24];
        short vertices_i = 0;
        var indices = new short[36];
        short indices_i = 0;
        
        var faces = new CuboidFace[6];
        for(int i = 0; i < baseFaceIndices.Length; i++)
        {
            var b = vertices_i;
            vertices[b + 0] = baseVertices[baseFaceIndices[i][0]];
            vertices[b + 1] = baseVertices[baseFaceIndices[i][1]];
            vertices[b + 2] = baseVertices[baseFaceIndices[i][2]];
            vertices[b + 3] = baseVertices[baseFaceIndices[i][3]];
            
            var c = indices_i;
            indices[c + 0] = (short)(b + 0);
            indices[c + 1] = (short)(b + 1);
            indices[c + 2] = (short)(b + 2);
            indices[c + 3] = (short)(b + 0);
            indices[c + 4] = (short)(b + 2);
            indices[c + 5] = (short)(b + 3);
            
            var faceIndices = new short[6]
            {
                (short)(b + 0),
                (short)(b + 1),
                (short)(b + 2),
                (short)(b + 0),
                (short)(b + 2),
                (short)(b + 3)
            };

            faces[i] = new CuboidFace((CubeFace)(i), faceIndices, b, c);
            vertices_i += 4;
            indices_i  += 6;
        }

        Vertices = vertices;
        Indices = indices;
        Faces = faces;
    }
    
}