using System;
using Alex.Blocks.State;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Elements.Map.Processing
{
	public class MapProcessingLayer
	{
		protected MapProcessingLayer()
		{
			
		}

		public virtual Color Apply(Color color, int x, int y, int z, BlockState state)
		{
			return color;
		}
	}

	public class LightShadingLayer : MapProcessingLayer
	{
		private readonly World _world;

		public LightShadingLayer(World world)
		{
			_world = world;
		}
		
		/// <inheritdoc />
		public override Color Apply(Color color,  int x, int y, int z, BlockState state)
		{
			_world.GetLight(new BlockCoordinates(x, y, z), out var blockLight, out var skyLight);
			var adjusted = (1f / 15) * blockLight;
			
			return color.Lighten(adjusted);
		}
	}
}