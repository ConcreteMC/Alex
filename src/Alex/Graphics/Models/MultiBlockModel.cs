using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
    public class MultiBlockModel : BlockModel
    {
		private BlockModel[] Models { get; }
		public MultiBlockModel(params BlockModel[] models)
		{
			Models = models;
		}

	    public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
	    {
			List<VertexPositionNormalTextureColor> vertices = new List<VertexPositionNormalTextureColor>();
		    foreach (var model in Models)
		    {
				vertices.AddRange(model.GetVertices(world, position, baseBlock));
		    }

		    return vertices.ToArray();
	    }
    }
}
