using System.Numerics;

namespace ResourcePackLib.ModelExplorer.Geometry;

public class ModelBoneBuilder
{
    internal readonly ModelBuilder _modelBuilder;
    private readonly string _name;
    internal string _parentName;
    private Vector3? _pivot;
    private Vector3? _bindPoseRotation;
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

    public ModelBoneBuilder Pivot(Vector3? pivot)
    {
        _pivot = pivot;
        return this;
    }

    public ModelBoneBuilder BindPoseRotation(Vector3? bindPoseRotation)
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

        _meshes = new (ModelVertexIndexPositionTexture[] vertices, short[] indices)[_cubeBuilders.Count];
        for (var i = 0; i < _cubeBuilders.Count; i++)
        {
            var cube = _cubeBuilders[i];
            var mesh = cube.Build();
            bone.AddMesh(mesh);
            _meshes[i] = cube._mesh;
            _nVertices += cube._mesh.vertices.Length;
            _nIndices += cube._mesh.indices.Length;
        }

        if (_pivot.HasValue)
            bone.Transform.RotationOrigin = _pivot.Value;
        
        if (_bindPoseRotation.HasValue)
            bone.Transform.Rotation = _bindPoseRotation.Value;

        _built = bone;
        return bone;
    }
}