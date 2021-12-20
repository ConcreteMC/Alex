using System;
using System.Collections.Generic;
using Alex.Blocks.Materials;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChunkColumn = Alex.Worlds.Chunks.ChunkColumn;

namespace Alex.Gui.Elements.Map
{
	public class RenderedMap : Utils.Map, IMapElement, IDisposable
	{
		private int Size { get; set; }
		public bool IsDirty { get; private set; }
		public bool Invalidated { get; private set; } = false;
		
		public ChunkCoordinates Coordinates { get; }
		
		/// <inheritdoc />
		public Vector3 Position { get; }
		public RenderedMap(ChunkCoordinates coordinates, int size = 16) : base(size, size)
		{
			Position = new Vector3(coordinates.X * 16, 0f, coordinates.Z * 16);
			Size = size;
			Coordinates = coordinates;
		}

		public void Invalidate()
		{
			Invalidated = true;
		}
		
		public void MarkDirty()
		{
			IsDirty = true;
		}

		public void Tick(World world, bool alphaBlend)
		{
			if (Invalidated)
				return;
			
			try
			{
				var target = world.GetChunkColumn(Coordinates.X, Coordinates.Z);

				if (target == null)
				{
					Invalidate();
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

						if (alphaBlend)
						{
							//Blend transparent layers
							while (color.A < 255 && height > target.WorldSettings.MinY)
							{
								//Hmmm..
								//Should we do a `s.Block.BlockMaterial.MapColor.Index != blockMaterial.MapColor.Index &&`
								var bs = GetHighestBlock(
									target, x, height, z, (s) => s.Block.BlockMaterial.MapColor.BaseColor.A > 0,
									out height);
								
								if (bs == null)
									break;

								color = color.Blend(
									GetColorForBlock(world, bs.Block.BlockMaterial, rx, height, rz), color.A);
							}
						}

						color.A = 255;
						this[(x * scale), (z * scale)] = color;
					}
				}
			}
			finally
			{
				IsDirty = false;
			}
		}

		private BlockState GetHighestBlock(ChunkColumn target, int x, int height, int z, Predicate<BlockState> predicate, out int finalHeight)
		{
			finalHeight = height;
			
			if (height > target.WorldSettings.MinY)
			{
				for (int y = height - 1; y > target.WorldSettings.MinY; y--)
				{
					var state = target.GetBlockState(x, y, z);

					if (predicate(state))
					{
						finalHeight = y;
						return state;
					}
				}
			}

			return null;
		}

		private Color GetColorForBlock(World world, IMaterial blockMaterial, int x, int height, int z)
		{
			var north = world.GetHeight(new BlockCoordinates(x, height, z - 1)) - 1;
			var northWest = world.GetHeight(new BlockCoordinates(x - 1, height, z - 1)) - 1;

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
				HasChanges = false;
				return null;
			}

			return base.GetData();
		}

		private Texture2D _texture = null;
		/// <inheritdoc />
		public override Texture2D GetTexture(GraphicsDevice device)
		{
			var texture = _texture;
			if (texture == null)
			{
				var data = GetData();

				texture = new Texture2D(device, Width, Height);

				if (data != null)
				{
					texture.SetData(data);
				}
			}

			if (HasChanges && !IsDirty)
			{
				var data = GetData();
				
				if (data != null)
					texture.SetData(data);
			}

			_texture = texture;
			return texture;	
		}

		/// <inheritdoc />
		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_texture?.Dispose();
				_texture = null;
			}
		}
	}
}