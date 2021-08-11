using System;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Elements.Map
{
	public class RenderedMap : Utils.Map, IDisposable
	{
		private const int Size = 16;
		private const int Multiplier = Size / 16;
		
		public bool IsDirty { get; private set; }
		public bool Invalidated { get; private set; } = false;

		public Texture2D Texture { get; private set; }

		public ChunkCoordinates Coordinates { get; }
		public RenderedMap(ChunkCoordinates coordinates) : base(Size,Size, 1)
		{
			Coordinates = coordinates;
		}

		private void Init(GraphicsDevice device)
		{
			if (Texture != null)
				return;

			Texture = new Texture2D(device, Size, Size);
		}

		public void Update(World world, ChunkColumn target, GraphicsDevice device)
		{
			if (target == null)
			{
				Invalidated = true;

				return;
			}

			if (Texture == null)
				Init(device);

			var cx = target.X * 16;
			var cz = target.Z * 16;
			var maxHeight = 0;

			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					BlockState state;

					var height = target.GetHeight(x, z);

					do
					{
						height--;
						state = target.GetBlockState(x, height, z);
						maxHeight = Math.Max(height, maxHeight);
					} while (height > 0 && (state.Block.BlockMaterial.MapColor.BaseColor.A <= 0));
					var blockMaterial = state?.Block?.BlockMaterial;
					
					if (blockMaterial == null)
						continue;
						
					var blockNorth = world.GetHeight(new BlockCoordinates((x + cx) + 1, height, (z + cz))) - 1;
					var offsetNorth = GetOffset(blockNorth, height);
					
					//var blockEast = world.GetHeight(new BlockCoordinates((x + cx), height, (z + cz) + 1)) - 1;
					//var offsetEast = GetOffset(blockEast, height);


					var bx = x * Multiplier;
					var bz = x * Multiplier;
					this[bx, bz] = blockMaterial.MapColor.Index * 4 + offsetNorth;
					this[bx, bz] = blockMaterial.MapColor.Index * 4 + offsetNorth;
					
					//this[bx, bz] = blockMaterial.MapColor.Index * 4 + offsetEast;
					//this[bx, bz] = blockMaterial.MapColor.Index * 4 + offsetEast;
				}
			}

			Texture.SetData(this.GetData());
			IsDirty = false;
		}

		private int GetOffset(int neighbor, int self)
		{
			var offset = 1;

			if (neighbor > self)
			{
				offset = 0;
			}
			else if (neighbor < self)
			{
				offset = 2;
			}

			return offset;
		}

		public void MarkDirty()
		{
			IsDirty = true;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			base.Dispose();
			//_map?.Dispose();
			// _map = null;
			Texture?.Dispose();
			Texture = null;
		}
	}
}