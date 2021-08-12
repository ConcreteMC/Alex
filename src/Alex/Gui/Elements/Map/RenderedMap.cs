using System;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Worlds;
using ChunkColumn = Alex.Worlds.Chunks.ChunkColumn;

namespace Alex.Gui.Elements.Map
{
	public class RenderedMap : Utils.Map, IDisposable
	{
		private static readonly uint[] DefaultData;

		static RenderedMap()
		{
			DefaultData = ArrayOf<uint>.Create(Size * Size, 0);
		}
		
		private const int Size = 16;
		
		public bool IsDirty { get; private set; }
		public bool Invalidated { get; private set; } = false;

		public bool PendingChanges { get; private set; } = false;
		public ChunkCoordinates Coordinates { get; }
		public RenderedMap(ChunkCoordinates coordinates) : base(Size,Size, 1)
		{
			Coordinates = coordinates;
		}

		public void Invalidate()
		{
			Invalidated = true;
		}
		
		public void Update(World world, ChunkColumn target)
		{
			if (target == null)
			{
				Invalidated = true;
				return;
			}

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

					var north = world.GetHeight(new BlockCoordinates((x + cx), height, (z + cz - 1))) - 1;
					var northWest = world.GetHeight(new BlockCoordinates((x + cx - 1), height, (z + cz - 1))) - 1;

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

					var blockMaterial = state?.Block?.BlockMaterial;

					if (blockMaterial != null)
					{
						this[x, z] = blockMaterial.MapColor.Index * 4 + offset;
					}
				}
			}

			//Texture.SetData(this.GetData());
			IsDirty = false;
			PendingChanges = true;
		}

		/// <inheritdoc />
		public override uint[] GetData()
		{
			if (Invalidated)
			{
				PendingChanges = false;
				return DefaultData;
			}

			var data = base.GetData();
			PendingChanges = false;

			return data;
		}

		/// <inheritdoc />
		//public override Texture2D GetTexture(GraphicsDevice device)
		//{
		//	return Texture;
		//}

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
		//	Texture?.Dispose();
		//	Texture = null;
		}
	}
}