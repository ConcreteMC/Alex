using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = System.Numerics.Vector2;

namespace ResourcePackLib.ModelExplorer.Geometry;

public class TruModel
{
    public TruModelTransform Transform { get; }
    
    public TruModelBoneCollection Bones { get; private set; }
    
    public TruModelBone RootBone { get; set; }
    
    public object Tag { get; set; }
    
    public VertexPositionColorTexture[] Vertices { get; }
    public short[] Indices { get; }
    
    public TruModel(ModelVertexIndexPositionTexture[] vertices, short[] indices, List<TruModelBone> bones)
    {
        Transform = new TruModelTransform();
        Vertices = vertices.Select(v => new VertexPositionColorTexture(
            new Microsoft.Xna.Framework.Vector3(v.Position.X, v.Position.Y, v.Position.Z),
            Color.WhiteSmoke,
            new Microsoft.Xna.Framework.Vector2(v.Uv.X, v.Uv.Y)))
            .ToArray();
        Indices = indices;
        Bones = new TruModelBoneCollection(bones);
        foreach (var bone in Bones)
        {
            if (bone.Parent == null)
                bone.Transform.ParentTransform = Transform;
        }
    }
}