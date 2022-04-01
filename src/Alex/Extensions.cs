using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Alex.Blocks.Minecraft.Signs;
using Alex.Blocks.Minecraft.Terracotta;
using Alex.Common.Blocks;
using Alex.Common.Gui.Graphics;
using Alex.Common.Utils.Vectors;
using Alex.Entities.BlockEntities;
using Alex.Gui;
using Alex.Gui.Elements.Map;
using Alex.Interfaces;
using Alex.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

namespace Alex
{
	public static class Extensions
	{
		static Extensions() { }

		public static Color Blend(this Color color, Color backColor, byte amount)
		{
			amount = (byte)(255 - amount);
			byte r = (byte)((color.R * amount / 255) + backColor.R * (255 - amount) / 255);
			byte g = (byte)((color.G * amount / 255) + backColor.G * (255 - amount) / 255);
			byte b = (byte)((color.B * amount / 255) + backColor.B * (255 - amount) / 255);

			return new Color(r, g, b, backColor.A);
		}

		/// <summary>
		/// Calculate percentage similarity of two strings
		/// <param name="source">Source String to Compare with</param>
		/// <param name="target">Targeted String to Compare</param>
		/// <returns>Return Similarity between two strings from 0 to 1.0</returns>
		/// </summary>
		public static double CalculateSimilarity(this string source, string target)
		{
			if ((source == null) || (target == null)) return 0.0;
			if ((source.Length == 0) || (target.Length == 0)) return 0.0;
			if (source == target) return 1.0;

			int stepsToSame = ComputeLevenshteinDistance(source, target);

			return (1.0 - (stepsToSame / (double)Math.Max(source.Length, target.Length)));
		}

		/// <summary>
		/// Returns the number of steps required to transform the source string
		/// into the target string.
		/// </summary>
		static int ComputeLevenshteinDistance(string source, string target)
		{
			if ((source == null) || (target == null)) return 0;
			if ((source.Length == 0) || (target.Length == 0)) return 0;
			if (source == target) return source.Length;

			int sourceWordCount = source.Length;
			int targetWordCount = target.Length;

			// Step 1
			if (sourceWordCount == 0)
				return targetWordCount;

			if (targetWordCount == 0)
				return sourceWordCount;

			int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

			// Step 2
			for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
			for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

			for (int i = 1; i <= sourceWordCount; i++)
			{
				for (int j = 1; j <= targetWordCount; j++)
				{
					// Step 3
					int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

					// Step 4
					distance[i, j] = Math.Min(
						Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
				}
			}

			return distance[sourceWordCount, targetWordCount];
		}

		public static void InitMarkers(GuiRenderer renderer)
		{
			foreach (var m in Enum.GetValues<MapMarker>())
			{
				_mapTextures[m] = GetTexture2D(m, renderer);
			}
		}

		private static GuiTexture2D GetTexture2D(MapMarker marker, IGuiRenderer renderer)
		{
			GuiTextures texture;

			switch (marker)
			{
				case MapMarker.None:
					return null;

				case MapMarker.WhitePointer:
					texture = AlexGuiTextures.MapMarkers.WhitePointer;

					break;

				case MapMarker.RedPointer:
					texture = AlexGuiTextures.MapMarkers.RedPointer;

					break;

				case MapMarker.GreenPointer:
					texture = AlexGuiTextures.MapMarkers.GreenPointer;

					break;

				case MapMarker.BluePointer:
					texture = AlexGuiTextures.MapMarkers.BluePointer;

					break;

				case MapMarker.Cross:
					texture = AlexGuiTextures.MapMarkers.Cross;

					break;

				case MapMarker.RedThing:
					texture = AlexGuiTextures.MapMarkers.RedThing;

					break;

				case MapMarker.BigBlip:
					texture = AlexGuiTextures.MapMarkers.BigDot;

					break;

				case MapMarker.SmallBlip:
					texture = AlexGuiTextures.MapMarkers.SmallDot;

					break;

				case MapMarker.House:
					texture = AlexGuiTextures.MapMarkers.House;

					break;

				case MapMarker.BlueStructure:
					texture = AlexGuiTextures.MapMarkers.BlueStructure;

					break;

				case MapMarker.RedCross:
					texture = AlexGuiTextures.MapMarkers.RedCross;

					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(marker), marker, null);
			}

			GuiTexture2D value = texture;
			value.RepeatMode = TextureRepeatMode.Stretch;
			value.TryResolveTexture(renderer);

			return value;
		}

		private static Dictionary<MapMarker, GuiTexture2D> _mapTextures = new Dictionary<MapMarker, GuiTexture2D>();

		public static GuiTexture2D ToTexture(this MapMarker marker)
		{
			return _mapTextures[marker];
			//	Value = value;
		}

		public static IMapColor ToMapColor(this WoodType woodType)
		{
			MapColor mapColor = MapColor.Wood;

			switch (woodType)
			{
				case WoodType.Oak:
					mapColor = MapColor.Wood;

					break;

				case WoodType.Spruce:
					mapColor = MapColor.Podzol;

					break;

				case WoodType.Birch:
					mapColor = MapColor.Sand;

					break;

				case WoodType.Jungle:
					mapColor = MapColor.Dirt;

					break;

				case WoodType.Acacia:
					mapColor = MapColor.Orange;

					break;

				case WoodType.DarkOak:
					mapColor = MapColor.Brown;

					break;

				case WoodType.Crimson:
					mapColor = MapColor.CrimsonStem;

					break;

				case WoodType.Warped:
					mapColor = MapColor.WarpedStem;

					break;
			}

			return mapColor;
		}

		public static IMapColor ToMapColor(this BlockColor color)
		{
			switch (color)
			{
				case BlockColor.White:
					return MapColor.Quartz;

				case BlockColor.Orange:
					return MapColor.Orange;

				case BlockColor.Magenta:
					return MapColor.Magenta;

				case BlockColor.LightBlue:
					return MapColor.LightBlue;

				case BlockColor.Yellow:
					return MapColor.Yellow;

				case BlockColor.Lime:
					return MapColor.LightGreen;

				case BlockColor.Pink:
					return MapColor.Pink;

				case BlockColor.Gray:
					return MapColor.Gray;

				case BlockColor.Cyan:
					return MapColor.Cyan;

				case BlockColor.Purple:
					return MapColor.Purple;

				case BlockColor.Blue:
					return MapColor.Blue;

				case BlockColor.Brown:
					return MapColor.Brown;

				case BlockColor.Green:
					return MapColor.Green;

				case BlockColor.Red:
					return MapColor.Red;

				case BlockColor.Black:
					return MapColor.Black;

				default:
					return MapColor.TerracottaWhite;
			}
		}

		public static IMapColor ToMapColor(this ClayColor color)
		{
			switch (color)
			{
				case ClayColor.White:
					return MapColor.TerracottaWhite;

				case ClayColor.Orange:
					return MapColor.TerracottaOrange;

				case ClayColor.Magenta:
					return MapColor.TerracottaMagenta;

				case ClayColor.LightBlue:
					return MapColor.TerracottaLightBlue;

				case ClayColor.Yellow:
					return MapColor.TerracottaYellow;

				case ClayColor.Lime:
					return MapColor.TerracottaLightGreen;

				case ClayColor.Pink:
					return MapColor.TerracottaPink;

				case ClayColor.Gray:
					return MapColor.TerracottaGray;

				case ClayColor.Silver:
					return MapColor.TerracottaLightGray;

				case ClayColor.Cyan:
					return MapColor.TerracottaCyan;

				case ClayColor.Purple:
					return MapColor.TerracottaPurple;

				case ClayColor.Blue:
					return MapColor.TerracottaBlue;

				case ClayColor.Brown:
					return MapColor.TerracottaBrown;

				case ClayColor.Green:
					return MapColor.TerracottaGreen;

				case ClayColor.Red:
					return MapColor.TerracottaRed;

				case ClayColor.Black:
					return MapColor.TerracottaBlack;

				default:
					return MapColor.TerracottaWhite;
			}
		}

		public static bool IsAir(this Item item)
		{
			return item == null || item is ItemAir || item.Count <= 0;
		}

		/// <summary>
		/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
		/// </summary>
		/// <param name="position">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
		/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
		/// <returns>Transformed <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
		public static Vector3 Transform(this Vector3 position, Matrix matrix)
		{
			return Vector3.Transform(position, matrix);
			//Transform(ref position, ref matrix, out position);
			//return position;
		}

		/// <summary>
		/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
		/// </summary>
		/// <param name="position">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
		/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
		/// <param name="result">Transformed <see cref="T:Microsoft.Xna.Framework.Vector3" /> as an output parameter.</param>
		public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
		{
			result.X = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
			result.Y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
			result.Z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
		}

		public static RasterizerState Copy(this RasterizerState state)
		{
			return new RasterizerState
			{
				CullMode = state.CullMode,
				DepthBias = state.DepthBias,
				FillMode = state.FillMode,
				MultiSampleAntiAlias = state.MultiSampleAntiAlias,
				Name = state.Name,
				ScissorTestEnable = state.ScissorTestEnable,
				SlopeScaleDepthBias = state.SlopeScaleDepthBias,
				Tag = state.Tag,
			};
		}

		public static string SplitPascalCase(this string input)
		{
			var result = new StringBuilder();

			foreach (var ch in input)
			{
				if (char.IsUpper(ch) && result.Length > 0)
				{
					result.Append(' ');
				}

				result.Append(ch);
			}

			return result.ToString();
		}

		public static byte[] ReadAllBytes(this Stream reader)
		{
			const int bufferSize = 4096;

			using (var ms = new MemoryStream())
			{
				byte[] buffer = new byte[bufferSize];
				int count;

				while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
					ms.Write(buffer, 0, count);

				return ms.ToArray();
			}
		}


		public static BoundingBox OffsetBy(this BoundingBox box, Vector3 offset)
		{
			box.Min += offset;
			box.Max += offset;

			return box;
		}

		public static Vector3 Floor(this Vector3 toFloor)
		{
			return new Vector3(
				(float)Math.Floor(toFloor.X), (float)Math.Floor(toFloor.Y), (float)Math.Floor(toFloor.Z));
		}

		public static BlockFace GetBlockFace(this Vector3 vector)
		{
			if (vector == Vector3.Up)
				return BlockFace.Up;

			if (vector == Vector3.Down)
				return BlockFace.Down;

			if (vector == Vector3.Forward)
				return BlockFace.North;

			if (vector == Vector3.Right)
				return BlockFace.East;

			if (vector == Vector3.Backward)
				return BlockFace.South;

			if (vector == Vector3.Left)
				return BlockFace.West;

			return BlockFace.None;
		}

		public static BlockFace GetBlockFace(this BlockCoordinates coordinates)
		{
			if (coordinates == BlockCoordinates.Up)
				return BlockFace.Up;

			if (coordinates == BlockCoordinates.Down)
				return BlockFace.Down;

			if (coordinates == BlockCoordinates.North)
				return BlockFace.North;

			if (coordinates == BlockCoordinates.East)
				return BlockFace.East;

			if (coordinates == BlockCoordinates.South)
				return BlockFace.South;

			if (coordinates == BlockCoordinates.West)
				return BlockFace.West;

			return BlockFace.None;
		}

		public static void Fill<TType>(this TType[] data, TType value)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = value;
			}
		}

		public static Guid GuidFromBits(long least, long most)
		{
			byte[] uuidMostSignificantBytes = BitConverter.GetBytes(most);
			byte[] uuidLeastSignificantBytes = BitConverter.GetBytes(least);

			byte[] guidBytes = new byte[16]
			{
				uuidMostSignificantBytes[4], uuidMostSignificantBytes[5], uuidMostSignificantBytes[6],
				uuidMostSignificantBytes[7], uuidMostSignificantBytes[2], uuidMostSignificantBytes[3],
				uuidMostSignificantBytes[0], uuidMostSignificantBytes[1], uuidLeastSignificantBytes[7],
				uuidLeastSignificantBytes[6], uuidLeastSignificantBytes[5], uuidLeastSignificantBytes[4],
				uuidLeastSignificantBytes[3], uuidLeastSignificantBytes[2], uuidLeastSignificantBytes[1],
				uuidLeastSignificantBytes[0]
			};

			return new Guid(guidBytes);
		}

		public static bool IsBitSet(this byte b, int pos)
		{
			return (b & (1 << pos)) != 0;
		}

		public static byte SetBit(this byte b, int bit, bool value)
		{
			var mask = (byte)(1 << bit);

			if (value)
			{
				b |= mask;
			}
			else
			{
				b = (byte)(b & ~mask);
			}

			return b;
		}
	}
}