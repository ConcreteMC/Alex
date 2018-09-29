using System.Collections.Generic;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Blocks
{
    public class MultiBlockModel : BlockModel
    {
		private BlockModel[] Models { get; }
		public MultiBlockModel(params BlockModel[] models)
		{
			Models = models;
		}

	    public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, IBlock baseBlock)
	    {
			List<VertexPositionNormalTextureColor> vertices = new List<VertexPositionNormalTextureColor>();
		    for (var index = 0; index < Models.Length; index++)
		    {
			    var model = Models[index];
			    model.Scale = 1f - (index * 0.001f);
				vertices.AddRange(model.GetVertices(world, position, baseBlock));
		    }

		    return vertices.ToArray();
	    }
    }
}
