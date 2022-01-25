using System.Numerics;
using ResourcePackLib.Core.Data;
using ResourcePackLib.ModelExplorer.Utilities.Extensions;
using Quaternion = Microsoft.Xna.Framework.Quaternion;

namespace ResourcePackLib.ModelExplorer.Geometry;

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