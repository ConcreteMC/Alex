using System.Numerics;

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