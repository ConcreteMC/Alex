using System;
using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;

namespace Alex.Worlds
{
	public class BlockBreakProgress
	{
		public BlockCoordinates Coordinates { get; }
		public BoundingBox BoundingBox { get; set; }

		public byte Stage { get; private set; } = 0;
		public float TimeRequired { get; }
		
		private int _destroyingTick = 0;
		public BlockBreakProgress(BlockCoordinates position, double requiredTime)
		{
			Coordinates = position;
			BoundingBox = new BoundingBox(position, new Vector3(position.X + 1f, position.Y + 1f, position.Z + 1f));
			TimeRequired = (float) requiredTime;
		}

		public void Tick()
		{
			if (TimeRequired > 0)
			{
				_destroyingTick++;

				Stage = (byte) Math.Clamp(MathF.Ceiling(((1f / TimeRequired) * _destroyingTick) * 10), 0, 9);
			}
		}

		public void SetStage(byte destroyStage)
		{
			Stage = destroyStage;
		}
	}
}