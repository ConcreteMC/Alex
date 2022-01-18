using System.Collections.ObjectModel;
using System.Numerics;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

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

    public List<TruModelMesh> Meshes
    {
        get => _meshes;
        private set => _meshes = value;
    }
    
    public string Name { get; set; }
    
    public TruModelBone Parent { get; set; }
    public TruModelBoneCollection Children { get; private set; }
    public TruModelTransform Transform { get; }

    public TruModelBone()
    {
        Transform = new TruModelTransform();
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
    
    public TruModelMeshCollection Meshes { get; private set; }
    
    public TruModelBone RootBone { get; set; }
    
    public object Tag { get; set; }
    
    public Vector2 TextureSize { get; }

    public VertexPositionTexture[] Vertices { get; }
    public short[] Indices { get; }
    
    public TruModel()
    {
        Transform = new TruModelTransform();
    }
}