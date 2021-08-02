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
		public bool IsDirty { get; private set; }
		public bool Invalidated { get; private set; } = false;

		public Texture2D Texture { get; private set; }

		public ChunkCoordinates Coordinates { get; }
		public RenderedMap(ChunkCoordinates coordinates) : base(16,16)
		{
			Coordinates = coordinates;
		}

		private void Init(GraphicsDevice device)
		{
			if (Texture != null)
				return;

			Texture = new Texture2D(device, 16, 16);
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
					} while (height > 0 && state.Block.BlockMaterial.MapColor.BaseColor.A <= 0);

					var blockNorth = world.GetHeight(new BlockCoordinates((x + cx), height, (z + cz) - 1)) - 1;

					var offset = 1;

					if (blockNorth > height)
					{
						offset = 0;
					}
					else if (blockNorth < height)
					{
						offset = 2;
					}

					var blockMaterial = state?.Block?.BlockMaterial;

					if (blockMaterial != null)
					{
						this[x, z] = blockMaterial.MapColor.Index * 4 + offset;
					}
				}
			}

			Texture.SetData(this.GetData());
			IsDirty = false;
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