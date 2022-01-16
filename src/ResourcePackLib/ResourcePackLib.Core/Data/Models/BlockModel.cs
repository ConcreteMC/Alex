using System.Numerics;

namespace ResourcePackLib.Core.Data.Models;

public class BlockModel
{
    public IReadOnlyCollection<BlockModelPart> Parts { get; }
    
    public BlockModel() 
    {
        
    }
}

public class BlockModelPart : Cuboid
{
    public BlockModelPart(Vector3[] vertices, short[] indices) : base(vertices, indices)
    {
    }

    public BlockModelPart(Vector3 min, Vector3 max) : base(min, max)
    {
    }
}