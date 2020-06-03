using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alex.API.Blocks;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Networking.Java.Util;
using Alex.ResourcePackLib.Json;
using fNbt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Worlds;
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

	public class ChunkColumn
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		public const int ChunkHeight = 256;
		public const int ChunkWidth = 16;
		public const int ChunkDepth = 16;

		public int X { get; set; }
		public int Z { get; set; }

		public bool IsNew { get; set; } = true;
		public bool IsDirty { get; set; }
		public bool SkyLightDirty { get; set; }
		public bool BlockLightDirty { get; set; }

		public ChunkSection[] Sections { get; set; } = new ChunkSection[16];
		public int[] BiomeId = ArrayOf<int>.Create(256, 1);
		public short[] Height = new short[256];
		
		public object UpdateLock { get; set; } = new object();
		public ScheduleType Scheduled { get; set; } = ScheduleType.Unscheduled;

		public ChunkColumn()
		{
			IsDirty = true;
			SkyLightDirty = true;
			BlockLightDirty = true;

			for (int i = 0; i < Sections.Length; i++)
			{
				//var b = new ExtendedBlockStorage(i, true);
				Sections[i] = null;
			}
		}

		public IEnumerable<BlockCoordinates> GetLightSources()
		{
			for (int i = 0; i < Sections.Length; i++)
			{
				var section = Sections[i];
				if (section == null)
					continue;
				
				foreach (var ls in section.LightSources)
				{
					yield return new BlockCoordinates(ls.X, (i * 16) + ls.Y, ls.Z);
				}
			}
		}

		private void SetDirty()
		{
			IsDirty = true;
		}

		public void ScheduleBlockUpdate(int x, int y, int z)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			var section = Sections[y >> 4];
			if (section == null) return;
			section.SetScheduled(x, y - ((y >> 4) << 4), z, true);
			// _scheduledUpdates[y << 8 | z << 4 | x] = true;
			//_scheduledUpdates.Add(y << 8 | z << 4 | x);
		}

		public ChunkSection GetSection(int y)
		{
			var section = Sections[y >> 4];
			if (section == null)
			{
				var storage = new ChunkSection(this, y, true, 2);
				Sections[y >> 4] = storage;
				return storage;
			}

			return (ChunkSection) section;
		}

		public void SetBlockState(int x, int y, int z, BlockState blockState)
		{
			SetBlockState(x, y, z, blockState, 0);

			//var section = Sections[y >> 4];
			//if (section == null) return;
			//section.ScheduledUpdates[(y >> 4) << 8 | z << 4 | x] = true;
			// _scheduledUpdates[y << 8 | z << 4 | x] = true;
		}

		public void SetBlockState(int x, int y, int z, BlockState state, int storage)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			GetSection(y).Set(storage, x, y - ((y >> 4) << 4), z, state);
			SetDirty();

			//RecalculateHeight(x, z);

			_heightDirty = true;
		}

		public void RecalculateHeight(int x, int z, bool doLighting = true)
		{
			bool inLight = doLighting;
			//bool inAir = true;

			for (int y = 255; y > 0; y--)
			{
				if (inLight)
				{
					//var block = GetBlock(x, y, z);
					var section = GetSection(y);
					var block = section.Get(x, y - ((@y >> 4) << 4), z).Block;

					if (!block.Renderable || (!block.BlockMaterial.BlocksLight()))
					{
						SetSkyLight(x, y, z, 15);
					}
					else
					{
						SetHeight(x, z, (short) (y + 1));
						SetSkyLight(x, y, z, 0);
						inLight = false;
					}
				}
				else
				{
					SetSkyLight(x, y, z, (byte) (doLighting ? 0 : 15));
				}
			}
		}

		public int GetRecalculatedHeight(int x, int z)
		{
			bool isInAir = true;

			for (int y = 255; y >= 0; y--)
			{
				{
					var chunk = GetSection(y);
					if (isInAir && chunk.IsAllAir)
					{
						if (chunk.IsDirty) Array.Fill<byte>(chunk.SkyLight.Data, 0xff);
						y -= 15;
						continue;
					}

					isInAir = false;

					var block = GetBlockState(x, y, z).Block;

					if (!block.Renderable || (block.Transparent && !block.BlockMaterial.BlocksLight()))
						continue;

					return y + 1;
				}
			}

			return 0;
		}

		private static BlockState Air = BlockFactory.GetBlockState("minecraft:air");

		public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
			{
				yield return new ChunkSection.BlockEntry(Air, 0);
				yield break;
			}

			var chunk = Sections[by >> 4];
			if (chunk == null)
			{
				yield return new ChunkSection.BlockEntry(Air, 0);
				yield break;
			}

			by = by - ((@by >> 4) << 4);

			foreach (var bs in chunk.GetAll(bx, by, bz))
			{
				yield return bs;
			}
		}

		public BlockState GetBlockState(int bx, int by, int bz)
		{
			return GetBlockState(bx, by, bz, 0);
		}

		public BlockState GetBlockState(int bx, int by, int bz, int storage)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return Air;

			var chunk = Sections[by >> 4];
			if (chunk == null) return Air;

			by = by - ((@by >> 4) << 4);

			return chunk.Get(bx, by, bz, storage);
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
				return 255;

			return (byte) Height[((bz << 4) + (bx))];
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

			var section = Sections[by >> 4];
			if (section == null) return 0;

			return (byte) section.GetBlocklight(bx, by - ((@by >> 4) << 4), bz);
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			var yMod = ((@by >> 4) << 4);

			GetSection(by).SetBlocklight(bx, by - yMod, bz, data);

			BlockLightDirty = true;

			//_scheduledLightingUpdates[by << 8 | bz << 4 | bx] = true;
			var section = (ChunkSection) Sections[by >> 4];
			if (section == null) return;
			//section.ScheduledSkylightUpdates[by << 8 | bz << 4 | bx] = true;
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 0xff;

			var section = Sections[by >> 4];
			if (section == null) return 0xff;

			return section.GetSkylight(bx, by - 16 * (by >> 4), bz);
		}

		public void SetSkyLight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			GetSection(by).SetSkylight(bx, by - 16 * (by >> 4), bz, data);
			SkyLightDirty = true;

			//	_scheduledLightingUpdates[by << 8 | bz << 4 | bx] = true;
			// var section = Sections[by >> 4];
			//  if (section == null) return;
			// section.ScheduledSkylightUpdates[(by - 16 * (by >> 4)) << 8 | bz << 4 | bx] = true;
		}

		private Vector3 Position => new Vector3(X * 16, 0, Z * 16);

		public NbtCompound[] Entities { get; internal set; }

		public bool HasDirtySubChunks
		{
			get { return Sections != null && Sections.Any(s => s != null && s.IsDirty); }
		}

		private bool _isHighPriority = false;

		public bool HighPriority
		{
			get { return _isHighPriority; }
			set
			{
				_isHighPriority = value;
				if (!value)
				{
				}
				else
				{
				
				}
			}
		}

		private bool _heightDirty = true;
		private int _heighest = 256;

		public int GetHeighest()
		{
			if (_heightDirty)
			{
				_heighest = Height.Max();
				_heightDirty = false;
			}

			return _heighest;
		}

		public bool IsTransparent(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			var section = Sections[@by >> 4];
			if (section == null) return true;


			return section.IsTransparent(bx, @by & 0xf, bz);
		}

		public bool IsSolid(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			var section = Sections[@by >> 4];
			if (section == null) return true;

			return section.IsSolid(bx, @by & 0xf, bz);
		}

		public bool IsScheduled(int bx, int @by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			var section = Sections[@by >> 4];
			if (section == null) return false;

			return section.IsScheduled(bx, @by & 0xf, bz);
		}

		public void GetBlockData(int bx, int by, int bz, out bool transparent, out bool solid)
		{
			transparent = false;
			solid = false;
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			var section = Sections[@by >> 4];
			if (section == null) return;

			section.GetBlockData(bx, @by & 0xf, bz, out transparent, out solid);
		}

		public void Dispose()
		{
			/*if (TransparentVertexBuffer != null)
			{
				if (!TransparentVertexBuffer.IsDisposed)
				{
					TransparentVertexBuffer.Dispose();
				}

				TransparentVertexBuffer = null;
			}*/

			foreach (var chunksSection in Sections)
			{
				chunksSection?.Dispose();
			}
			
			//	if (Mesh != null)
			//{
			//	Mesh = null;
			//}
		}

		public void CalculateHeight(bool doLighting = true)
		{
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					RecalculateHeight(x, z, doLighting);
				}
			}

			GetHeighest();

			foreach (var section in Sections)
			{
				section?.RemoveInvalidBlocks();
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
					var storage = (ChunkSection) this.Sections[sectionY];
					if ((availableSections & (1 << sectionY)) != 0)
					{
						if (storage == null)
						{
							storage = new ChunkSection(this, sectionY, readSkylight);
						}

						storage.Read(ms);
						//var blockCount = ms.ReadShort();
						//byte bitsPerBlock = (byte) ms.ReadByte();
						//storage.
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
						if (groundUp && (storage == null || !storage.IsEmpty()))
						{
							if (storage == null)
								storage = new ChunkSection(this, sectionY, readSkylight, 2);
						}
					}

					storage.IsDirty = true;
					
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
					Sections[i]?.RemoveInvalidBlocks();
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
