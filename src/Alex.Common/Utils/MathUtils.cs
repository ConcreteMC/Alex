using System;
using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils
{
	public static class MathUtils
	{
		public static Vector3 LerpVector3Safe(PlayerLocation start, PlayerLocation end, float amount)
		{
			if (amount >= 1f) return end;
			if (amount <= 0f) return start;

			return new Vector3(
				LerpSafe(start.X, end.X, amount), LerpSafe(start.Y, end.Y, amount), LerpSafe(start.Z, end.Z, amount));
		}
		
		public static Vector3 LerpVector3Safe(Vector3 start, Vector3 end, float amount)
		{
			if (amount >= 1f) return end;
			if (amount <= 0f) return start;

			return new Vector3(
				LerpSafe(start.X, end.X, amount), LerpSafe(start.Y, end.Y, amount), LerpSafe(start.Z, end.Z, amount));
		}

		public static float LerpSafe(float start, float end, float amount)
		{
			if (amount >= 1f) return end;
			if (amount <= 0f) return start;

			return start + (end - start) * amount;
		}

		public static Vector3 LerpVector3Degrees(Vector3 start, Vector3 end, float amount)
		{
			if (amount >= 1f) return end;
			if (amount <= 0f) return start;

			return new Vector3(
				LerpDegrees(start.X, end.X, amount), LerpDegrees(start.Y, end.Y, amount),
				LerpDegrees(start.Z, end.Z, amount));
		}

		public static float LerpDegrees(float start, float end, float amount)
		{
			if (amount >= 1f) return end;
			if (amount <= 0f) return start;
			
			float difference = MathF.Abs(end - start);

			if (difference < 0.001f)
				return end;
			
			if (difference > 180f)
			{
				// We need to add on to one of the values.
				if (end > start)
				{
					// We'll add it on to start...
					start += 360f;
				}
				else
				{
					// Add it on to end.
					end += 360f;
				}
			}

			// Interpolate it.
			float value = (start + ((end - start) * amount));

			// Wrap it..
			const float rangeZero = 360f;

			if (value >= 0f && value <= 360f)
				return value;

			return (value % rangeZero);
		}

		public static float ConstrainAngle(float targetAngle, float centreAngle, float maximumDifference)
		{
			return centreAngle + Clamp(targetAngle - centreAngle, -maximumDifference, maximumDifference);
		}

		// numeric double clamp
		public static float Clamp(float value, float min, float max)
		{
			return (value < min ? min : (value > max ? max : value));
		}

		public static float ToRadians(float deg)
		{
			return deg * (MathF.PI / 180f);
		}

		public static float RadianToDegree(float angle)
		{
			return angle * (180.0f / MathF.PI);
		}

		public static float AngleToDegree(sbyte angle)
		{
			return (angle / 256f) * 360f;
		}

		public static float AngleToNotchianDegree(sbyte angle)
		{
			return (float)(angle * 360) / 256.0F;
			return AngleToDegree(angle);
		}

		public static float FromFixedPoint(short f)
		{
			return f / (32.0f * 128.0f);
		}

		public static int HsvToRGB(float hue, float saturation, float value)
		{
			int i = (int)(hue * 6.0F) % 6;
			float f = hue * 6.0F - (float)i;
			float f1 = value * (1.0F - saturation);
			float f2 = value * (1.0F - f * saturation);
			float f3 = value * (1.0F - (1.0F - f) * saturation);
			float f4;
			float f5;
			float f6;

			switch (i)
			{
				case 0:
					f4 = value;
					f5 = f3;
					f6 = f1;

					break;

				case 1:
					f4 = f2;
					f5 = value;
					f6 = f1;

					break;

				case 2:
					f4 = f1;
					f5 = value;
					f6 = f3;

					break;

				case 3:
					f4 = f1;
					f5 = f2;
					f6 = value;

					break;

				case 4:
					f4 = f3;
					f5 = f1;
					f6 = value;

					break;

				case 5:
					f4 = value;
					f5 = f1;
					f6 = f2;

					break;

				default:
					throw new Exception(
						"Something went wrong when converting from HSV to RGB. Input was " + hue + ", " + saturation
						+ ", " + value);
			}

			int j = MathHelper.Clamp((int)(f4 * 255.0F), 0, 255);
			int k = MathHelper.Clamp((int)(f5 * 255.0F), 0, 255);
			int l = MathHelper.Clamp((int)(f6 * 255.0F), 0, 255);

			return j << 16 | k << 8 | l;
		}

		private static int[] MULTIPLY_DE_BRUIJN_BIT_POSITION = new int[]
		{
			0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6,
			11, 5, 10, 9
		};

		public static int SmallestEncompassingPowerOfTwo(int value)
		{
			int i = value - 1;
			i = i | i >> 1;
			i = i | i >> 2;
			i = i | i >> 4;
			i = i | i >> 8;
			i = i | i >> 16;

			return i + 1;
		}

		private static bool IsPowerOfTwo(int value)
		{
			return value != 0 && (value & value - 1) == 0;
		}

		public static int Log2DeBruijn(int value)
		{
			value = IsPowerOfTwo(value) ? value : SmallestEncompassingPowerOfTwo(value);

			return MULTIPLY_DE_BRUIJN_BIT_POSITION[(int)((long)value * 125613361L >> 27) & 31];
		}
	}
}