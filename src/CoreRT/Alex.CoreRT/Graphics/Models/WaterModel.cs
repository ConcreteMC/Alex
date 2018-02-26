using Alex.CoreRT.API.Graphics;
using Alex.CoreRT.API.World;
using Alex.CoreRT.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.CoreRT.Graphics.Models
{

	//TODO:
	//https://github.com/Thinkofname/steven-go/blob/master/blockliquid.go
	public class WaterModel : BlockModel
	{
		public bool IsLava = false;
		public int Level = 8;
		public WaterModel()
		{

		}

		public override VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
		{
			int tl = 0, tr = 0, bl = 0, br = 0;

			return new VertexPositionNormalTextureColor[0];
		}

		protected int GetAverageLiquidLevels(IWorld world, Vector3 position)
		{
			int level = 0;
			for (int xx = -1; xx <= 0; xx++)
			{
				for (int zz = -1; zz <= 0; zz++)
				{
					var b = (Block)world.GetBlock(position.X + xx, position.Y + 1, position.Z + zz);
					if (b.BlockModel is WaterModel m && m.IsLava == IsLava)
					{
						return 8;
					}

					b = (Block)world.GetBlock(position.X + xx, position.Y, position.Z + zz);
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
