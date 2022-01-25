using System.Diagnostics;
using System.Numerics;

namespace ResourcePackLib.ModelExplorer.Geometry;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
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

    private string DebuggerDisplay
    {
        get
        {
            return $"TruModelBone{{{Name}, LocalRotationOrigin={{{Transform.RotationOrigin}}}, LocalScaleOrigin={{{Transform.ScaleOrigin}}}}}";
        }
    }
}