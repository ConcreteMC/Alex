using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using Vector2 = System.Numerics.Vector2;

namespace ResourcePackLib.ModelExplorer.Geometry;

public class TruModelMesh
{
    public int NumVertices { get; internal set; }
    public int VertexOffset { get; internal set; }
    public int PrimitiveCount { get; internal set; }
    public int IndexOffset { get; internal set; }
    
    public TruModelMesh()
    {
        
    }
}

public class TruModelTransform : Transform3D
{
    public TruModelTransform()
    {
    }
}

public class TruModelMeshCollection : ReadOnlyCollection<TruModelMesh>
{
    public TruModelMeshCollection([NotNull] IList<TruModelMesh> list) : base(list)
    {
        
    }
}
public class TruModelBoneCollection : ReadOnlyCollection<TruModelBone>
{
    public TruModelBoneCollection([NotNull] IList<TruModelBone> list) : base(list)
    {
        
    }
}
public class TruModelBone
{
    private List<TruModelBone> _children = new List<TruModelBone>();
    private List<TruModelMesh> _meshes = new List<TruModelMesh>();
    private TruModelBone _parent;

    public TruModelMeshCollection Meshes { get; }
    
    public string Name { get; set; }

    public TruModelBone Parent
    {
        get => _parent;
        private set
        {
            _parent = value;
            Transform.ParentTransform = value?.Transform;
        }
    }

    public TruModelBoneCollection Children { get; }
    public TruModelTransform Transform { get; }

    public TruModelBone()
    {
        Transform = new TruModelTransform();
        Meshes = new TruModelMeshCollection(_meshes);
        Children = new TruModelBoneCollection(_children);
    }
		
    public void AddMesh(TruModelMesh mesh)
    {
        _meshes.Add(mesh);
    }

    public void AddChild(TruModelBone modelBone)
    {
        _children.Add(modelBone);
        modelBone.Parent = this;
    }
}

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