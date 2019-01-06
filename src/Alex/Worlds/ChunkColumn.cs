using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Networking.Java.Util;
using Alex.Utils;
using fNbt;
using fNbt.Tags;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using NLog;

namespace Alex.Worlds
{
	public static class ArrayOf<T> where T : new()
	{
		public static T[] Create(int size, T initialValue)
		{
			T[] array = (T[])Array.CreateInstance(typeof(T), size);
			for (int i = 0; i < array.Length; i++)
				array[i] = initialValue;
			return array;
		}

		public static T[] Create(int size)
		{
			T[] array = (T[])Array.CreateInstance(typeof(T), size);
			for (int i = 0; i < array.Length; i++)
				array[i] = new T();
			return array;
		}
	}

	public class ChunkColumn : IChunkColumn
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		public const int ChunkHeight = 256;
		public const int ChunkWidth = 16;
		public const int ChunkDepth = 16;

		public int X { get; set; }
		public int Z { get; set; }

		public bool IsAllAir = false;
		public bool IsNew = true;
		public bool IsLoaded = false;
		public bool NeedSave = false;
		public bool IsDirty { get; set; }
		public bool SkyLightDirty { get; set; }

		public ExtendedBlockStorage[] Sections = new ExtendedBlockStorage[16];
		public int[] BiomeId = ArrayOf<int>.Create(256, 1);
		public short[] Height = new short[256];

		public VertexBuffer VertexBuffer { get; set; } = null;
		public VertexBuffer TransparentVertexBuffer { get; set; } = null;
		public object VertexLock { get; set; } = new object();
		public object UpdateLock { get; set; } = new object();
		public ScheduleType Scheduled { get; set; } = ScheduleType.Unscheduled;

		public ChunkColumn()
		{
			IsDirty = true;
			SkyLightDirty = true;

			for (int i = 0; i < Sections.Length; i++)
			{
				var b = new ExtendedBlockStorage(i, true);
				Sections[i] = b;
			}
		}

		private void SetDirty()
		{
			IsDirty = true;
			NeedSave = true;
		}

		public void SetBlockState(int x, int y, int z, IBlockState blockState)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			Sections[y >> 4].Set(x, y - 16 * (y >> 4), z, blockState);
			SetDirty();

			RecalculateHeight(x, z);

            _heightDirty = true;
		}

		private void RecalculateHeight(int x, int z)
		{
			for (int y = 256 - 1; y > 0; --y)
			{
				if (GetBlock(x, y, z).Renderable)
				{
					SetHeight(x, z, (byte) y);
					break;
				}
			}

			GetHeighest();
		}

		private static IBlockState Air = BlockFactory.GetBlockState("minecraft:air");
		public IBlockState GetBlockState(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return Air;

			var chunk = Sections[by >> 4];
			if (chunk == null) return Air;

			by = by - 16 * (by >> 4);
			
			return chunk.Get(bx, by, bz);
		}

		public IBlock GetBlock(int bx, int by, int bz)
		{
			var bs = GetBlockState(bx, by, bz);

			if (bs == null) return new Air();

			return bs.Block;
		}

		public void SetBlock(int bx, int by, int bz, IBlock block)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			Sections[by >> 4].Set(bx, by - 16 * (by >> 4), bz, block.BlockState);
			SetDirty();

			RecalculateHeight(bx, bz);

            _heightDirty = true;
		}

		public void SetHeight(int bx, int bz, short h)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			Height[((bz << 4) + (bx))] = h;
			SetDirty();
		}

		public byte GetHeight(int bx, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 0;

			return (byte)Height[((bz << 4) + (bx))];
		}

		public void SetBiome(int bx, int bz, int biome)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			BiomeId[(bz << 4) + (bx)] = biome;
			SetDirty();
		}

		public int GetBiome(int bx, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 0;

			return BiomeId[(bz << 4) + (bx)];
		}

		public byte GetBlocklight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 0;

			return (byte) Sections[@by >> 4].GetExtBlocklightValue(bx, @by - 16 * (@by >> 4), bz);
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			Sections[@by >> 4].SetExtBlocklightValue(bx, @by - 16 * (@by >> 4), bz, data);
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 16;

			return Sections[@by >> 4].GetExtSkylightValue(bx, @by - 16 * (@by >> 4), bz);
		}

		public void SetSkyLight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			Sections[@by >> 4].SetExtSkylightValue(bx, @by - 16 * (@by >> 4), bz, data);
			SkyLightDirty = true;
		}

		private Vector3 Position => new Vector3(X * 16, 0, Z*16);

		public NbtCompound[] Entities { get; internal set; }

		public void GenerateMeshes(IWorld world, out ChunkMesh mesh)
		{
			List<VertexPositionNormalTextureColor> solidVertices = new List<VertexPositionNormalTextureColor>();
			List<VertexPositionNormalTextureColor> transparentVertices = new List<VertexPositionNormalTextureColor>();

			//if (Scheduled == ScheduleType.Full || PositionCache == null)
			{
				for (var index = 0; index < Sections.Length; index++)
				{
					var chunk = Sections[index];
					if (chunk.IsEmpty()) continue;

					for (var x = 0; x < ChunkWidth; x++)
					for (var z = 0; z < ChunkDepth; z++)
					for (var y = 0; y < 16; y++)
					{
						Update(world, chunk,
							index,
							x, y, z,
							solidVertices,
							transparentVertices);
					}
				}
			}

			mesh = new ChunkMesh(solidVertices.ToArray(), transparentVertices.ToArray());
		//	PositionCache = mesh.EntryPositions;
		}

		private void Update(IWorld world, ExtendedBlockStorage chunk,
			int index, int x, int y, int z,
			List<VertexPositionNormalTextureColor> solidVertices,
			List<VertexPositionNormalTextureColor> transparentVertices)
		{
			var stateId = chunk.Get(x, y, z);

			if (stateId == null)
			{
				Log.Warn($"State is null!");
				return;
			}

			IBlock block = stateId.Block;// BlockFactory.GetBlock(stateId);

			if (!block.Renderable) return;

			var blockPosition = new Vector3(x, y + (index * 16), z) + Position;

			var vert = stateId.Model.GetVertices(world, blockPosition, block);
		//	var result = new ChunkMesh.Entry(block.BlockStateResource.ID, vert, blockPosition);

			if (block.Transparent)
			{
				transparentVertices.AddRange(vert);
				//transparentVertices.AddRange(vert);
			}
			else
			{
				solidVertices.AddRange(vert);
				//solidVertices.AddRange(vert);
			}
		}

		private bool _heightDirty = true;
		private int _heighest = -1;
		public int GetHeighest()
		{
			if (_heightDirty)
			{
				_heighest = Height.Max();
				_heightDirty = false;
			}

			return _heighest;
		}

		public void Dispose()
		{
			if (VertexBuffer != null)
			{
				if (!VertexBuffer.IsDisposed)
				{
					VertexBuffer.Dispose();
				}

				VertexBuffer = null;
			}

			if (TransparentVertexBuffer != null)
			{
				if (!TransparentVertexBuffer.IsDisposed)
				{
					TransparentVertexBuffer.Dispose();
				}

				TransparentVertexBuffer = null;
			}
		}

		public void CalculateHeight()
		{
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 256 - 1; y > 0; --y)
					{
						if (GetBlock(x, y, z).Renderable)
						{
							SetHeight(x, z, (byte)y);
							break;
						}
					}
				}
			}

			GetHeighest();
		}

		public void CalculateSkylight()
		{
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					int light = 15;
					for (int y = 256 - 1; y > 0; --y)
					{
						var block = GetBlock(x, y, z);
						if (!block.Renderable) continue;

						if (block.Solid && !block.Transparent)
						{
							light = 0;
						}
						else
						{
							var lightOpacity = block.LightOpacity;
							if (lightOpacity == 0 && light != 15)
							{
								lightOpacity = 1;
							}

							light -= lightOpacity;
							//(int) Math.Round(prevLight *  (1D - block.LightOpacity));
						}

						if (light > 0)
						{
							SetSkyLight(x, y, z, (byte) light);
						}

					}
				}
			}
		}

		public void Read(MinecraftStream ms, int availableSections, bool groundUp, bool readSkylight)
		{
			try
			{
				//	Stopwatch s = Stopwatch.StartNew();
				//	Log.Debug($"Reading chunk data...");

				for (int sectionY = 0; sectionY < this.Sections.Length; sectionY++)
				{
					var storage = this.Sections[sectionY];
					if ((availableSections & (1 << sectionY)) != 0)
					{
						if (storage == null)
						{
							storage = new ExtendedBlockStorage(sectionY, readSkylight);
						}

						storage.Data.Read(ms);
						/*
						for (int y = 0; y < 16; y++)
						{
							for (int z = 0; z < 16; z++)
							{
								for (int x = 0; x < 16; x += 2)
								{
									// Note: x += 2 above; we read 2 values along x each time
									byte value = (byte)ms.ReadByte();

									storage.SetExtBlocklightValue(x, y, z, (byte)(value & 0xF));
									storage.SetExtBlocklightValue(x + 1, y, z, (byte)((value >> 4) & 0xF));
								}
							}
						}

						//if (currentDimension.HasSkylight())
						if (readSkylight)
						{
							for (int y = 0; y < 16; y++)
							{
								for (int z = 0; z < 16; z++)
								{
									for (int x = 0; x < 16; x += 2)
									{
										// Note: x += 2 above; we read 2 values along x each time
										byte value = (byte)ms.ReadByte();

										storage.SetExtSkylightValue(x, y, z, value & 0xF);
										storage.SetExtSkylightValue(x + 1, y, z, (value >> 4) & 0xF);
									}
								}
							}
						}*/
					}
					else
					{
						if (groundUp && !storage.IsEmpty())
						{
							storage = new ExtendedBlockStorage(sectionY, readSkylight);
						}
					}

					this.Sections[sectionY] = storage;
				}

				if (groundUp)
				{
					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							var biomeId = ms.ReadInt();
							SetBiome(x, z, biomeId);
						}
					}
				}

				for (int i = 0; i < Sections.Length; i++)
				{
					Sections[i].RemoveInvalidBlocks();
				}

				CalculateHeight();
			}
			catch (Exception e)
			{
				Log.Warn($"Received supposedly corrupted chunk:" + e);
			}
		}
	}
}
