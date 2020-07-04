using System.Collections.Generic;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
    public class MultiBlockModel : BlockModel
    {
		public BlockModel[] Models { get; }
		public MultiBlockModel(params BlockModel[] models)
		{
			Models = models;
		}

	    public override VerticesResult GetVertices(IBlockAccess world, Vector3 position, Block baseBlock)
	    {
			List<BlockShaderVertex> vertices = new List<BlockShaderVertex>();
			List<int> indexes = new List<int>();
		    List<int> animatedIndexes = new List<int>();
		    
		    for (var index = 0; index < Models.Length; index++)
		    {
			    var model = Models[index];
			    model.Scale = 1f - (index * 0.001f);

			    var verts = model.GetVertices(world, position, baseBlock);
			    
				for (int i = 0; i < verts.Indexes.Length; i++)
				{
					indexes.Add(vertices.Count + verts.Indexes[i]);
				}

				if (verts.AnimatedIndexes != null)
				{
					for (int i = 0; i < verts.AnimatedIndexes.Length; i++)
					{
						indexes.Add(vertices.Count + verts.AnimatedIndexes[i]);
					}
				}

				vertices.AddRange(verts.Vertices);
		    }

		    return new VerticesResult(vertices.ToArray(), indexes.ToArray(), animatedIndexes.Count > 0 ? animatedIndexes.ToArray() : null);
	    }
    }
}
