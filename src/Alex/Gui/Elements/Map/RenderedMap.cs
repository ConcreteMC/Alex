using System;
using System.Collections.Generic;
using Alex.Blocks.Materials;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using ChunkColumn = Alex.Worlds.Chunks.ChunkColumn;

namespace Alex.Gui.Elements.Map
{
	public class RenderedMap : Utils.Map, IDisposable
	{
		private int Size { get; set; }
		public bool IsDirty { get; private set; }
		public bool Invalidated { get; private set; } = false;

		public bool PendingChanges { get; private set; } = false;
		public ChunkCoordinates Coordinates { get; }
		public RenderedMap(ChunkCoordinates coordinates, int size = 16) : base(size, size)
		{
			Size = size;
			Coordinates = coordinates;
		}

		public void Invalidate()
		{
			Invalidated = true;
		}
		
		public void Update(World world)
		{
			try
			{
				var target = world.GetChunkColumn(Coordinates.X, Coordinates.Z);

				if (target == null)
				{
					Invalidated = true;

					return;
				}

				var cx = target.X * 16;
				var cz = target.Z * 16;

				var scale = Size / 16;
				for (int x = 0; x < 16; x++)
				{
					var rx = cx + x;
					for (int z = 0; z < 16; z++)
					{
						var rz = cz + z;
						
						BlockState state;

						var height = target.GetHeight(x, z);

						state = GetHighestBlock(
							target, x, height, z, (s) => s.Block.BlockMaterial.MapColor.BaseColor.A > 0, out height);
						
						var blockMaterial = state?.Block?.BlockMaterial;

						if (blockMaterial == null || blockMaterial.MapColor.BaseColor.A <= 0)
							continue;

						Color color = GetColorForBlock(
							world, blockMaterial, rx, height, rz);

						//Blend transparent layers
						while (color.A < 255 && height > target.WorldSettings.MinY)
						{
							//Hmmm..
							//Should we do a `s.Block.BlockMaterial.MapColor.Index != blockMaterial.MapColor.Index &&`
							var bs = GetHighestBlock( 
								target, x, height, z, (s) => s.Block.BlockMaterial.MapColor.BaseColor.A > 0, out height);

							color = color.Blend(
								GetColorForBlock(world, bs.Block.BlockMaterial, rx, height, rz),
								color.A);
						}

						color.A = 255;
						for (int xOffset = 0; xOffset < scale; xOffset++)
						{
							for (int zOffset = 0; zOffset < scale; zOffset++)
							{
								this[(x * scale) + xOffset, (z * scale) + zOffset] = color;
							}
						}
					}
				}
			}
			finally
			{
				IsDirty = false;
				PendingChanges = true;
			}
		}

		private BlockState GetHighestBlock(ChunkColumn target, int x, int height, int z, Predicate<BlockState> predicate, out int finalHeight)
		{
			BlockState state;
			
			do
			{
				height--;
				state = target.GetBlockState(x, height, z);
			} while (height > target.WorldSettings.MinY && !predicate(state));

			finalHeight = height;
			return state;
		}

		private Color GetColorForBlock(World world, IMaterial blockMaterial, int x, int height, int z)
		{
			var north = world.GetHeight(new BlockCoordinates(x, height, z - 1)) - 1;
			var northWest = world.GetHeight(new BlockCoordinates(x - 1, height, z)) - 1;

			var offset = 1;

			if (north > height && northWest <= height)
			{
				offset = 0; //Darker
			}
			else if (north > height && northWest > height)
			{
				offset = 3; //Darkest
			}
			else if (north < height && northWest < height)
			{
				offset = 2; //Lighter
			}

			var color = blockMaterial.MapColor.GetMapColor(offset);

			return color;
		}

		/// <inheritdoc />
		public override uint[] GetData()
		{
			if (Invalidated)
			{
				PendingChanges = false;
				return null;
			}

			var data = base.GetData();
			PendingChanges = false;

			return data;
		}

		public void MarkDirty()
		{
			IsDirty = true;
		}

		/// <inheritdoc />
		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}