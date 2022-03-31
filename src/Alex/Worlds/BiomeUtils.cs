using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Alex.Blocks.Storage.Palette;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using MathF = System.MathF;

namespace Alex.Worlds
{
	public class Biome : IHasKey
	{
		private static readonly Color DefaultWaterColor = ColorHelper.HexToColor("#44AFF5");
		private static readonly Color DefaultWaterFogColor = ColorHelper.HexToColor("#44AFF5");
		public static readonly Color DefaultFogColor = ColorHelper.HexToColor("#ABD2FF");
		public static readonly Color DefaultSkyColor = ColorHelper.HexToColor("#ABD2FF");

		public uint Id { get; set; }
		public string Name;
		public float Temperature;
		public float Downfall;

		public Color? FoliageColor { get; set; } = null;
		public Color? GrassColor { get; set; } = null;
		public Color? SkyColor { get; set; } = null;
		public Color FogColor { get; set; } = DefaultFogColor;
		public Color Water { get; set; } = DefaultWaterColor;

		private bool _waterFogColorSet = false;
		private Color _waterFogColor = DefaultWaterFogColor;

		public Color WaterFogColor
		{
			get
			{
				return _waterFogColorSet ? _waterFogColor : Water;
			}
			set
			{
				_waterFogColorSet = true;
				_waterFogColor = value;
			}
		}

		public float WaterFogDistance { get; set; } = 15f;
		public float WaterSurfaceTransparency { get; set; } = 0.65f;

		//public float HeightScale = 100;
	}

	public class BiomeUtils
	{
		public static Biome[] Biomes =
		{
			new Biome
			{
				Id = 0,
				Name = "Ocean",
				Temperature = 0.5f,
				Downfall = 0.5f,
				Water = ColorHelper.HexToColor("#1787D4"),
				WaterFogDistance = 60
				//	SurfaceBlock = 12,
				//	SoilBlock = 24
			}, // default values of temp and rain
			new Biome
			{
				Id = 1,
				Name = "Plains",
				Temperature = 0.8f,
				Downfall = 0.4f, //TODO
				Water = ColorHelper.HexToColor("#44AFF5")
			},
			new Biome
			{
				Id = 2,
				Name = "Desert",
				Temperature = 2.0f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#32A598")
			},
			new Biome
			{
				Id = 3,
				Name = "Extreme Hills",
				Temperature = 0.2f,
				Downfall = 0.3f,
				Water = ColorHelper.HexToColor("#007BF7")
			},
			new Biome
			{
				Id = 4,
				Name = "Forest",
				Temperature = 0.7f,
				Downfall = 0.8f,
				Water = ColorHelper.HexToColor("#1E97F2")
			},
			new Biome
			{
				Id = 5,
				Name = "Taiga",
				Temperature = 0.05f,
				Downfall = 0.8f,
				Water = ColorHelper.HexToColor("#287082")
			},
			new Biome
			{
				Id = 6,
				Name = "Swampland",
				Temperature = 0.8f,
				Downfall = 0.9f,
				Water = ColorHelper.HexToColor("#4c6559"),
				WaterFogDistance = 8
			},
			new Biome
			{
				Id = 7,
				Name = "River",
				Temperature = 0.5f,
				Downfall = 0.5f,
				Water = ColorHelper.HexToColor("#0084FF"),
				WaterFogDistance = 30
			}, // default values of temp and rain
			new Biome
			{
				Id = 8,
				Name = "Nether",
				Temperature = 2.0f,
				Downfall = 0.0f, //TODO!
				Water = ColorHelper.HexToColor("#905957")
			},
			new Biome
			{
				Id = 9,
				Name = "End",
				Temperature = 0.5f,
				Downfall = 0.5f, //TODO!
				Water = ColorHelper.HexToColor("#62529e")
			}, // default values of temp and rain
			new Biome
			{
				Id = 10,
				Name = "Frozen Ocean",
				Temperature = 0.0f,
				Downfall = 0.5f,
				Water = ColorHelper.HexToColor("#2570B5"),
				WaterFogDistance = 20
			},
			new Biome
			{
				Id = 11,
				Name = "Frozen River",
				Temperature = 0.0f,
				Downfall = 0.5f,
				Water = ColorHelper.HexToColor("#185390")
			},
			new Biome
			{
				Id = 12,
				Name = "Ice Plains",
				Temperature = 0.0f,
				Downfall = 0.5f, //TODO
				Water = ColorHelper.HexToColor("#14559b")
			},
			new Biome
			{
				Id = 13,
				Name = "Ice Mountains",
				Temperature = 0.0f,
				Downfall = 0.5f,
				Water = ColorHelper.HexToColor("#1156a7")
			},
			new Biome
			{
				Id = 14,
				Name = "Mushroom Island",
				Temperature = 0.9f,
				Downfall = 1.0f,
				Water = ColorHelper.HexToColor("#8a8997")
			},
			new Biome
			{
				Id = 15,
				Name = "Mushroom Island Shore",
				Temperature = 0.9f,
				Downfall = 1.0f,
				Water = ColorHelper.HexToColor("#818193")
			},
			new Biome
			{
				Id = 16,
				Name = "Beach",
				Temperature = 0.8f,
				Downfall = 0.4f,
				Water = ColorHelper.HexToColor("#157cab"),
				WaterFogDistance = 60
			},
			new Biome
			{
				Id = 17,
				Name = "Desert Hills",
				Temperature = 2.0f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#1a7aa1")
			},
			new Biome
			{
				Id = 18,
				Name = "Forest Hills",
				Temperature = 0.7f,
				Downfall = 0.8f,
				Water = ColorHelper.HexToColor("#056bd1")
			},
			new Biome
			{
				Id = 19,
				Name = "Taiga Hills",
				Temperature = 0.2f,
				Downfall = 0.7f,
				Water = ColorHelper.HexToColor("#236583")
			},
			new Biome
			{
				Id = 20,
				Name = "Extreme Hills Edge",
				Temperature = 0.2f,
				Downfall = 0.3f,
				Water = ColorHelper.HexToColor("#045cd5")
			},
			new Biome
			{
				Id = 21,
				Name = "Jungle",
				Temperature = 1.2f,
				Downfall = 0.9f,
				Water = ColorHelper.HexToColor("#14A2C5")
			},
			new Biome
			{
				Id = 22,
				Name = "Jungle Hills",
				Temperature = 1.2f,
				Downfall = 0.9f,
				Water = ColorHelper.HexToColor("#1B9ED8")
			},

			//TODO: The rest of min/max
			new Biome
			{
				Id = 23,
				Name = "Jungle Edge",
				Temperature = 0.95f,
				Downfall = 0.8f,
				Water = ColorHelper.HexToColor("#0D8AE3")
			},
			new Biome
			{
				Id = 24,
				Name = "Deep Ocean",
				Temperature = 0.5f,
				Downfall = 0.5f,
				Water = ColorHelper.HexToColor("#1787D4"),
				WaterFogDistance = 60
			},
			new Biome
			{
				Id = 25,
				Name = "Stone Beach",
				Temperature = 0.2f,
				Downfall = 0.3f,
				Water = ColorHelper.HexToColor("#0d67bb")
			},
			new Biome
			{
				Id = 26,
				Name = "Cold Beach",
				Temperature = 0.05f,
				Downfall = 0.3f,
				Water = ColorHelper.HexToColor("#1463a5"),
				WaterFogDistance = 50
			},
			new Biome
			{
				Id = 27,
				Name = "Birch Forest",
				Temperature = 0.6f,
				Downfall = 0.6f,
				Water = ColorHelper.HexToColor("#0677ce")
			},
			new Biome
			{
				Id = 28,
				Name = "Birch Forest Hills",
				Temperature = 0.6f,
				Downfall = 0.6f,
				Water = ColorHelper.HexToColor("#0677ce")
			},
			new Biome
			{
				Id = 29,
				Name = "Roofed Forest",
				Temperature = 0.7f,
				Downfall = 0.8f,
				Water = ColorHelper.HexToColor("#3B6CD1")
			},
			new Biome
			{
				Id = 30,
				Name = "Cold Taiga",
				Temperature = -0.5f,
				Downfall = 0.4f,
				Water = ColorHelper.HexToColor("#205e83")
			},
			new Biome
			{
				Id = 31,
				Name = "Cold Taiga Hills",
				Temperature = -0.5f,
				Downfall = 0.4f,
				Water = ColorHelper.HexToColor("#245b78")
			},
			new Biome
			{
				Id = 32,
				Name = "Mega Taiga",
				Temperature = 0.3f,
				Downfall = 0.8f,
				Water = ColorHelper.HexToColor("#2d6d77")
			},
			new Biome
			{
				Id = 33,
				Name = "Mega Taiga Hills",
				Temperature = 0.3f,
				Downfall = 0.8f,
				Water = ColorHelper.HexToColor("#286378")
			},
			new Biome
			{
				Id = 34,
				Name = "Extreme Hills+",
				Temperature = 0.2f,
				Downfall = 0.3f,
				Water = ColorHelper.HexToColor("#0E63AB")
			},
			new Biome
			{
				Id = 35,
				Name = "Savanna",
				Temperature = 1.2f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#2C8B9C")
			},
			new Biome
			{
				Id = 36,
				Name = "Savanna Plateau",
				Temperature = 1.0f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#2590A8")
			},
			new Biome
			{
				Id = 37,
				Name = "Mesa",
				Temperature = 2.0f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#4E7F81")
			},
			new Biome
			{
				Id = 38,
				Name = "Mesa Plateau F",
				Temperature = 2.0f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#55809E")
			},
			new Biome
			{
				Id = 39,
				Name = "Mesa Plateau",
				Temperature = 2.0f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#55809E")
			},
			new Biome()
			{
				Id = 46,
				Name = "Cold Ocean",
				Temperature = 0.5f,
				Water = ColorHelper.HexToColor("#2080C9"),
				WaterFogDistance = 60
			},
			new Biome()
			{
				Id = 49,
				Name = "Cold Deep Ocean",
				Temperature = 0.5f,
				Water = ColorHelper.HexToColor("#2080C9"),
				WaterFogDistance = 60
			},
			new Biome { Id = 127, Name = "The Void", Temperature = 0.8f, Downfall = 0.4f },
			new Biome { Id = 128, Name = "Unknown Biome", Temperature = 0.8f, Downfall = 0.4f },
			new Biome { Id = 129, Name = "Sunflower Plains", Temperature = 0.8f, Downfall = 0.4f },
			new Biome { Id = 130, Name = "Desert M", Temperature = 2.0f, Downfall = 0.0f },
			new Biome { Id = 131, Name = "Extreme Hills M", Temperature = 0.2f, Downfall = 0.3f },
			new Biome
			{
				Id = 132,
				Name = "Flower Forest",
				Temperature = 0.7f,
				Downfall = 0.8f,
				Water = ColorHelper.HexToColor("#20A3CC")
			},
			new Biome { Id = 133, Name = "Taiga M", Temperature = 0.05f, Downfall = 0.8f },
			new Biome
			{
				Id = 134,
				Name = "Swampland M",
				Temperature = 0.8f,
				Downfall = 0.9f,
				WaterFogDistance = 8
			},
			new Biome { Id = 140, Name = "Ice Plains Spikes", Temperature = 0.0f, Downfall = 0.5f },
			new Biome { Id = 149, Name = "Jungle M", Temperature = 1.2f, Downfall = 0.9f },
			new Biome { Id = 150, Name = "Unknown Biome", Temperature = 0.8f, Downfall = 0.4f },
			new Biome { Id = 151, Name = "JungleEdge M", Temperature = 0.95f, Downfall = 0.8f },
			new Biome { Id = 155, Name = "Birch Forest M", Temperature = 0.6f, Downfall = 0.6f },
			new Biome { Id = 156, Name = "Birch Forest Hills M", Temperature = 0.6f, Downfall = 0.6f },
			new Biome { Id = 157, Name = "Roofed Forest M", Temperature = 0.7f, Downfall = 0.8f },
			new Biome { Id = 158, Name = "Cold Taiga M", Temperature = -0.5f, Downfall = 0.4f },
			new Biome { Id = 160, Name = "Mega Spruce Taiga", Temperature = 0.25f, Downfall = 0.8f },
			// special exception, temperature not 0.3
			new Biome { Id = 161, Name = "Mega Spruce Taiga Hills", Temperature = 0.3f, Downfall = 0.8f },
			new Biome { Id = 162, Name = "Extreme Hills+ M", Temperature = 0.2f, Downfall = 0.3f },
			new Biome
			{
				Id = 163,
				Name = "Savanna M",
				Temperature = 1.2f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#2C8B9C")
			},
			new Biome
			{
				Id = 164,
				Name = "Savanna Plateau M",
				Temperature = 1.0f,
				Downfall = 0.0f,
				Water = ColorHelper.HexToColor("#2590A8")
			},
			new Biome { Id = 165, Name = "Mesa (Bryce)", Temperature = 2.0f, Downfall = 0.0f },
			new Biome { Id = 166, Name = "Mesa Plateau F M", Temperature = 2.0f, Downfall = 0.0f },
			new Biome { Id = 167, Name = "Mesa Plateau M", Temperature = 2.0f, Downfall = 0.0f },
		};

		public static ConcurrentDictionary<uint, Biome> Overrides { get; } = new ConcurrentDictionary<uint, Biome>();
		public static int BiomeCount => Biomes.Count(x => !Overrides.ContainsKey(x.Id)) + Overrides.Count;

		public static Biome GetBiome(int biomeId)
		{
			return GetBiome((uint)biomeId);
		}

		public static Biome GetBiome(uint biomeId)
		{
			Biome first = null;

			if (!Overrides.TryGetValue(biomeId, out first))
			{
				foreach (var biome in Biomes)
				{
					if (biome.Id == biomeId)
					{
						first = biome;

						break;
					}
				}
			}

			return first ?? new Biome { Id = biomeId };
		}

		public static Biome GetBiome(string name)
		{
			var values = Overrides.Values.ToArray();

			var firstBiome =
				values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			if (firstBiome != default)
				return firstBiome;
			
			foreach (var biome in Biomes)
			{
				if (biome.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return biome;
				}
			}

			return GetBiome(0);
		}
	}
}