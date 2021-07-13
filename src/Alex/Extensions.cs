using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Minecraft.Terracotta;
using Alex.Common.Blocks;
using Alex.Entities.BlockEntities;
using Alex.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Alex
{
	public static class Extensions
	{
		static Extensions()
		{
			
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
		
		public static IMapColor ToMapColor(this BedColor color)
		{
			switch (color)
			{
				case BedColor.White:
					return MapColor.TerracottaWhite;

				case BedColor.Orange:
					return MapColor.TerracottaOrange;

				case BedColor.Magenta:
					return MapColor.Magenta;

				case BedColor.LightBlue:
					return MapColor.LightBlue;

				case BedColor.Yellow:
					return MapColor.Yellow;

				case BedColor.Lime:
					return MapColor.LightGreen;

				case BedColor.Pink:
					return MapColor.Pink;

				case BedColor.Gray:
					return MapColor.Gray;

				case BedColor.Silver:
					return MapColor.LightGray;

				case BedColor.Cyan:
					return MapColor.Cyan;

				case BedColor.Purple:
					return MapColor.Purple;

				case BedColor.Blue:
					return MapColor.Blue;

				case BedColor.Brown:
					return MapColor.Brown;

				case BedColor.Green:
					return MapColor.Green;

				case BedColor.Red:
					return MapColor.Red;

				case BedColor.Black:
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
			result.X =  position.X * matrix.M11 +  position.Y *  matrix.M21 +  position.Z *  matrix.M31 + matrix.M41;
			result.Y =  position.X * matrix.M12 +  position.Y *  matrix.M22 +  position.Z *  matrix.M32 + matrix.M42;
			result.Z =  position.X * matrix.M13 +  position.Y *  matrix.M23 +  position.Z *  matrix.M33 + matrix.M43;
		}

		public static RasterizerState Copy(this RasterizerState state)
		{
			return new RasterizerState()
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
	        return new Vector3((float)Math.Floor(toFloor.X), (float)Math.Floor(toFloor.Y), (float)Math.Floor(toFloor.Z));
	    }

		public static BlockFace GetBlockFace(this Vector3 vector)
		{
			BlockFace face = BlockFace.None;

			if (vector == Vector3.Up)
			{
				face = BlockFace.Up;
			}
			else if (vector == Vector3.Down)
			{
				face = BlockFace.Down;
			}
			else if (vector == Vector3.Backward)
			{
				face = BlockFace.South;
			}
			else if (vector == Vector3.Forward)
			{
				face = BlockFace.North;
			}
			else if (vector == Vector3.Left)
			{
				face = BlockFace.West;
			}
			else if (vector == Vector3.Right)
			{
				face = BlockFace.East;
			}

			return face;
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
			byte[] guidBytes = new byte[16] {
				uuidMostSignificantBytes[4],
				uuidMostSignificantBytes[5],
				uuidMostSignificantBytes[6],
				uuidMostSignificantBytes[7],
				uuidMostSignificantBytes[2],
				uuidMostSignificantBytes[3],
				uuidMostSignificantBytes[0],
				uuidMostSignificantBytes[1],
				uuidLeastSignificantBytes[7],
				uuidLeastSignificantBytes[6],
				uuidLeastSignificantBytes[5],
				uuidLeastSignificantBytes[4],
				uuidLeastSignificantBytes[3],
				uuidLeastSignificantBytes[2],
				uuidLeastSignificantBytes[1],
				uuidLeastSignificantBytes[0]
			};

			return new Guid(guidBytes);
		}

		public static bool IsBitSet(this byte b, int pos)
		{
			return (b & (1 << pos)) != 0;
		}
	}
}
