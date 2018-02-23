using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex.Blocks;
using Alex.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{

	//TODO:
	//https://github.com/Thinkofname/steven-go/blob/master/blockliquid.go
	public class WaterModel : Model
	{
		public bool IsLava = false;
		public int Level = 8;
		public WaterModel()
		{

		}

		public override VertexPositionNormalTextureColor[] GetVertices(World world, Vector3 position, Block baseBlock)
		{
			int tl = 0, tr = 0, bl = 0, br = 0;

			return new VertexPositionNormalTextureColor[0];
		}

		protected int GetAverageLiquidLevels(World world, Vector3 position)
		{
			int level = 0;
			for (int xx = -1; xx <= 0; xx++)
			{
				for (int zz = -1; zz <= 0; zz++)
				{
					var b = world.GetBlock(position.X + xx, position.Y + 1, position.Z + zz);
					if (b.BlockModel is WaterModel m && m.IsLava == IsLava)
					{
						return 8;
					}

					b = world.GetBlock(position.X + xx, position.Y, position.Z + zz);
					if (b.BlockModel is WaterModel l && l.IsLava == IsLava)
					{
						var nl = 7 - (Level & 0x7);
						if (nl > level)
						{
							level = nl;
						}
					}
				}
			}

			return level;
		}
	}
}
