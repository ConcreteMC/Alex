using System.Collections.Generic;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Worlds;
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

	    public override (VertexPositionNormalTextureColor[] vertices, int[] indexes) GetVertices(IBlockAccess world, Vector3 position, Block baseBlock)
	    {
			List<VertexPositionNormalTextureColor> vertices = new List<VertexPositionNormalTextureColor>();
			List<int> indexes = new List<int>();
			
		    for (var index = 0; index < Models.Length; index++)
		    {
			    var model = Models[index];
			    model.Scale = 1f - (index * 0.001f);

			    var verts = model.GetVertices(world, position, baseBlock);
			    
				for (int i = 0; i < verts.indexes.Length; i++)
				{
					indexes.Add(vertices.Count + verts.indexes[i]);
				}
				
				vertices.AddRange(verts.vertices);
		    }

		    return (vertices.ToArray(), indexes.ToArray());
	    }
    }
}
