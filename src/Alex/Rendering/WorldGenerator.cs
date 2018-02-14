using System;
using log4net;
using LibNoise;
using LibNoise.Filter;
using Microsoft.Xna.Framework;
using MiNET.Utils;
using MiNET.Utils.Noise;
using MiNET.Worlds;
using MiNET.Worlds.Decorators;
using MiNET.Worlds.Generators;
using Voronoi = MiNET.Utils.Noise.Voronoi;

namespace Alex.Rendering
{
	public class OverworldGenerator : IWorldGenerator
	{
		private IModule2D RainNoise { get; }
		private IModule2D TempNoise { get; }

		private IModule2D BiomeModifierX { get; }
		private IModule2D BiomeModifierZ { get; }

		private readonly IModule2D _mainNoise;
		private readonly IModule3D _depthNoise;

		private int Seed { get; }

		public OverworldGenerator()
		{
			int seed = Config.GetProperty("seed", "YoHoMotherducker!").GetHashCode();
			Seed = seed;

			BiomeModifierX = new SimplexPerlin(seed + 3700);
			BiomeModifierZ = new SimplexPerlin(seed + 5000);

			var rainSimplex = new SimplexPerlin(seed);

			var rainNoise = new Voronoi();
			rainNoise.Primitive3D = rainSimplex;
			rainNoise.Primitive2D = rainSimplex;
			rainNoise.Distance = false;
			rainNoise.Frequency = RainFallFrequency;
			rainNoise.OctaveCount = 2;
			RainNoise = rainNoise;

			var tempSimplex = new SimplexPerlin(seed + 100);
			var tempNoise = new Voronoi();
			tempNoise.Primitive3D = tempSimplex;
			tempNoise.Primitive2D = tempSimplex;
			tempNoise.Distance = false;
			tempNoise.Frequency = TemperatureFrequency;
			tempNoise.OctaveCount = 2;
			//	tempNoise.Gain = 2.5f;
			TempNoise = tempNoise;

			var mainLimitNoise = new SimplexPerlin(seed + 200);

			var mainLimitFractal = new SumFractal()
			{
				Primitive3D = mainLimitNoise,
				Primitive2D = mainLimitNoise,
				Frequency = MainNoiseFrequency,
				OctaveCount = 2,
				Lacunarity = MainNoiseLacunarity,
				Gain = MainNoiseGain,
				SpectralExponent = MainNoiseSpectralExponent,
				Offset = MainNoiseOffset,
			};
			var mainScaler = new ScaleableNoise()
			{
				XScale = 1f / MainNoiseScaleX,
				YScale = 1f / MainNoiseScaleY,
				ZScale = 1f / MainNoiseScaleZ,
				Primitive3D = mainLimitFractal,
				Primitive2D = mainLimitFractal
			};
			//ModTurbulence turbulence = new ModTurbulence(mainLimitFractal, new ImprovedPerlin(seed - 350, NoiseQuality.Fast), new ImprovedPerlin(seed + 350, NoiseQuality.Fast), null, 0.0125F);
			_mainNoise = mainScaler; //turbulence;

			var mountainNoise = new SimplexPerlin(seed + 300);
			var mountainTerrain = new HybridMultiFractal()
			{
				Primitive3D = mountainNoise,
				Primitive2D = mountainNoise,
				Frequency = DepthFrequency,
				OctaveCount = 4,
				Lacunarity = DepthLacunarity,
				SpectralExponent = DepthNoiseScaleExponent,
				//Offset = 0.7f,
				Gain = DepthNoiseGain
			};

			ScaleableNoise scaling = new ScaleableNoise();
			scaling.Primitive2D = mountainTerrain;
			scaling.Primitive3D = mountainTerrain;
			scaling.YScale = 1f / HeightScale;
			scaling.XScale = 1f / DepthNoiseScaleX;
			scaling.ZScale = 1f / DepthNoiseScaleZ;

			_depthNoise = scaling;

		//	BiomeUtils.FixMinMaxHeight();
		}

		//1.83f;
		private const float TemperatureFrequency = 0.233f;

		private const float RainFallFrequency = 0.233f;

		private const float BiomeNoiseScale = 3.16312f;

		private const float MainNoiseScaleX = 80F;
		private const float MainNoiseScaleY = 160F;
		private const float MainNoiseScaleZ = 80F;
		private const float MainNoiseFrequency = 0.295f;
		private const float MainNoiseLacunarity = 2.127f;
		private const float MainNoiseGain = 2f;//0.256f;
		private const float MainNoiseSpectralExponent = 1f;//0.52f;//0.9f;//1.4f;
		private const float MainNoiseOffset = 1f;// 0.312f;

		private const float DepthNoiseScaleX = 200F;
		private const float DepthNoiseScaleZ = 200F;
		private const float DepthFrequency = 0.662f;
		private const float DepthLacunarity = 2.375f; //6f;
		private const float DepthNoiseGain = 2f;//0.256f;
		private const float DepthNoiseScaleExponent = 1f;//1;// 0.25f;//1.2f; //0.9f; //1.2F;

		private const float CoordinateScale = 84.412F;
		private const float HeightScale = 684.412F;

		public const int WaterLevel = 64;

		private const int SmoothSize = 2;
		private static float[] GuassianKernel;

		static OverworldGenerator()
		{
			GuassianKernel = new float[256];
			float bellSize = 1f / SmoothSize;
			float bellHeight = 2f * SmoothSize;

			for (int sx = -SmoothSize; sx <= SmoothSize; ++sx)
			{
				for (int sz = -SmoothSize; sz <= SmoothSize; ++sz)
				{
					var bx = bellSize * sx;
					var bz = bellSize * sz;

					GuassianKernel[((sx + SmoothSize) << 4) + (sz + SmoothSize)] = (float)(bellHeight * Math.Exp(-(bx * bx + bz * bz) / 2));
				}
			}
		}

		public Vector3 GetSpawnPoint()
		{
			return new Vector3(0, 128, 0);
		}

		public void Initialize()
		{

		}

		public Chunk GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			//	Stopwatch sw = Stopwatch.StartNew();
			Chunk chunk = new Chunk(chunkCoordinates.X, 0, chunkCoordinates.Z);

			int x = chunkCoordinates.X;
			int z = chunkCoordinates.Z;
			var biomes = CalculateBiomes(chunkCoordinates.X, chunkCoordinates.Z);

			var heightMap = GenerateHeightMap(biomes, x, z);
			var thresholdMap = GetThresholdMap(x, z, biomes);

			CreateTerrainShape(chunk, heightMap, thresholdMap, biomes);
			DecorateChunk(chunk, heightMap, thresholdMap, biomes, new ChunkDecorator[0]);

			//chunk.isDirty = true;
			//chunk.NeedSave = true;

			for (int mx = 0; mx < 16; mx++)
			{
				for (int mz = 0; mz < 16; mz++)
				{
					for (int y = 0; y < 256; y++)
					{
						chunk.SetSkylight(mx, y, mz, 15);
					}
				}
			}
			//	sw.Stop();

			//	if (sw.ElapsedMilliseconds > previousTime)
			//{
			//	Debug.WriteLine("Chunk gen took " + sw.ElapsedMilliseconds + " ms");
			//previousTime = sw.ElapsedMilliseconds;
			//}

			return chunk;
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof(OverworldGenerator));
		private Biome GetBiome(float x, float z)
		{
			x /= CoordinateScale;
			z /= CoordinateScale;

			var mX = x;// + BiomeModifierX.GetValue(x, z);
			var mZ = z;// + BiomeModifierZ.GetValue(x, z);

			var temp = TempNoise.GetValue(mX, mZ) * 2.0F;
			var rain = (RainNoise.GetValue(mX, mZ) + 1f) / 2f;

			if (temp < -1f) temp = -(temp % 1);

			if (rain < 0) rain = -rain;

			return BiomeUtils.GetBiome(temp, rain);
		}

		private Biome[] CalculateBiomes(int chunkX, int chunkZ)
		{
			//cx *= 16;
			//cz *= 16;

			int minX = (chunkX * 16) - 1;
			int minZ = (chunkZ * 16) - 1;
			var maxX = ((chunkX + 1) << 4) - 1;
			var maxZ = ((chunkZ + 1) << 4) - 1;

			Biome[] rb = new Biome[16 * 16];

			for (int x = 0; x < 16; x++)
			{
				float rx = MathHelpers.Lerp(minX, maxX, (1f / 15f) * x);
				for (int z = 0; z < 16; z++)
				{
					rb[(x << 4) + z] = GetBiome(rx, MathHelpers.Lerp(minZ, maxZ, (1f / 15f) * z));
				}
			}

			return rb;
		}

		private float CalculateHeight(Biome[] biomes, int x, int z)
		{
			float minSum = 0f;
			float maxSum = 0f;
			float weightSum = 0f;

			Biome adjacent;
			for (int sx = -SmoothSize; sx <= SmoothSize; ++sx)
			{
				for (int sz = -SmoothSize; sz <= SmoothSize; ++sz)
				{
					var weight = GuassianKernel[((sx + SmoothSize) << 4) + (sz + SmoothSize)];

					adjacent = GetBiome(x + sx, z + sz);

					minSum += (adjacent.MinHeight - 1) * weight;
					maxSum += adjacent.MaxHeight * weight;

					weightSum += weight;
				}
			}

			minSum /= weightSum;
			maxSum /= weightSum;

			return (maxSum - minSum) / 2f;
		}

		private float[] GenerateHeightMap(Biome[] biomes, int chunkX, int chunkZ)
		{
			int minX = (chunkX * 16) - 1;
			int minZ = (chunkZ * 16) - 1;
			var maxX = ((chunkX + 1) << 4) - 1;
			var maxZ = ((chunkZ + 1) << 4) - 1;

			int cx = (chunkX * 16);
			int cz = (chunkZ * 16);


			float q11 = MathHelpers.Abs(WaterLevel + (128f * CalculateHeight(biomes, minX, minZ)) * _mainNoise.GetValue(minX, minZ));
			float q12 = MathHelpers.Abs(WaterLevel + (128f * CalculateHeight(biomes, minX, maxZ)) * _mainNoise.GetValue(minX, maxZ));

			float q21 = MathHelpers.Abs(WaterLevel + (128f * CalculateHeight(biomes, maxX, minZ)) * _mainNoise.GetValue(maxX, minZ));
			float q22 = MathHelpers.Abs(WaterLevel + (128f * CalculateHeight(biomes, maxX, maxZ)) * _mainNoise.GetValue(maxX, maxZ));

			float[] heightMap = new float[16 * 16];

			for (int x = 0; x < 16; x++)
			{
				float rx = cx + x;

				for (int z = 0; z < 16; z++)
				{
					float rz = cz + z;

					//heightMap[(x << 4) + z] = MathHelpers.Abs(WaterLevel + (128f * CalculateHeight(biomes, cx + x, cz + z)) * _mainNoise.GetValue(rx, rz));

					var baseNoise = MathHelpers.BilinearCmr(
						rx, rz,
						q11,
						q12,
						q21,
						q22,
						minX, maxX, minZ, maxZ);

					heightMap[(x << 4) + z] = baseNoise; //WaterLevel + ((128f * baseNoise));
				}


			}
			return heightMap;
		}

		private float[] GetThresholdMap(int cx, int cz, Biome[] biomes)
		{
			cx *= 16;
			cz *= 16;

			float[] thresholdMap = new float[16 * 16 * 256];

			for (int x = 0; x < 16; x++)
			{
				float rx = cx + x;
				for (int z = 0; z < 16; z++)
				{
					float rz = cz + z;

					for (int y = 255; y > 0; y--)
					{
						thresholdMap[x + ((y + (z << 8)) << 4)] = 1f; //_depthNoise.GetValue(rx, y, rz);
					}
				}
			}
			return thresholdMap;
		}

		public const float Threshold = -0.1f;
		private const int Width = 16;
		private const int Depth = 16;
		private const int Height = 256;

		private void CreateTerrainShape(Chunk chunk, float[] heightMap, float[] thresholdMap, Biome[] biomes)
		{
			for (int x = 0; x < Width; x++)
			{
				for (int z = 0; z < Depth; z++)
				{
					var idx = (x << 4) + z;
					Biome biome = biomes[idx];
				//	chunk.biomeId[idx] = (byte)biome.Id;// SetBiome(x, z, (byte)biome.Id);
					float stoneHeight = heightMap[idx];
					/*	if (stoneHeight > 200 || stoneHeight < 0)
						{
							Debug.WriteLine("MaxHeight: " + stoneHeight);
						}*/

					var maxY = 0;
					for (int y = 0; y < stoneHeight && y < 255; y++)
					{
						float density = thresholdMap[x + ((y + (z << 8)) << 4)];

						if (y < WaterLevel || (density > Threshold && y >= WaterLevel))
						{
							chunk.SetBlock(x, y, z, BlockFactory.GetBlock(1, 0));
							maxY = y;
						}
					}

					chunk.SetBlock(x, 0, z, BlockFactory.GetBlock(7,0)); //Bedrock
					heightMap[idx] = maxY;
					//chunk.height[idx] = (short)maxY;
					//chunk.SetHeight(x, z, (byte)maxY);
				}
			}
		}

		private void DecorateChunk(Chunk chunk, float[] heightMap, float[] thresholdMap, Biome[] biomes, ChunkDecorator[] decorators)
		{
			for (int x = 0; x < Width; x++)
			{
				for (int z = 0; z < Depth; z++)
				{
					var height = heightMap[(x << 4) + z];
					var biome = biomes[(x << 4) + z];

					for (int y = 0; y < Height; y++)
					{
						bool isSurface = false;
						if (y <= height)
						{
							if (y < Chunk.ChunkHeight && chunk.GetBlock(x, y, z).BlockId == 1 && chunk.GetBlock(x, y + 1, z).BlockId == 0)
							{
								isSurface = true;
							}

							if (isSurface)
							{
								if (y >= WaterLevel)
								{
									chunk.SetBlock(x, y, z, BlockFactory.GetBlock(biome.SurfaceBlock, biome.SurfaceMetadata));
									//chunk.SetMetadata(x, y, z, biome.SurfaceMetadata);

									chunk.SetBlock(x, y - 1, z, BlockFactory.GetBlock(biome.SoilBlock, biome.SoilMetadata));
									//chunk.SetBlock(x, y - 1, z, biome.SoilBlock);
									//chunk.SetMetadata(x, y - 1, z, biome.SoilMetadata);
								}
							}
						}

						/*for (int i = 0; i < decorators.Length; i++)
						{
							decorators[i].Decorate(chunk, biome, thresholdMap, x, y, z, isSurface, y < height - 1);
						}*/
					}
				}
			}
		}
	}
}
