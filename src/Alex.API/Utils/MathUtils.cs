using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
	public static class MathUtils
	{
		public static float ConstrainAngle(float targetAngle, float centreAngle, float maximumDifference)
		{
			return centreAngle + Clamp(NormDeg(targetAngle - centreAngle), -maximumDifference, maximumDifference);
		}
		public static float normalAbsoluteAngleDegrees(float angle)
		{
			return (angle %= 360f) >= 0 ? angle : (angle + 360f);
		}

		// normalizes a double degrees angle to between +180 and -180
		public static float NormDeg(float a)
		{
			a %= 360f;
			if (a >= 180f)
			{
				a -= 360f;
			}
			if (a < -180)
			{
				a += 360f;
			}
			return a;
		}

		public static double NormDeg(double a)
		{
			a %= 360f;
			if (a >= 180f)
			{
				a -= 360f;
			}
			if (a < -180)
			{
				a += 360f;
			}
			return a;
		}

		// numeric double clamp
		public static float Clamp(float value, float min, float max)
		{
			return (value < min ? min : (value > max ? max : value));
		}

		public static float ToRadians(float deg)
		{
			return (float)(Math.PI* deg) / 180F;
		}

		public static float RadianToDegree(float angle)
		{
			return (float)(angle * (180.0f / Math.PI));
		}

		public static float AngleToDegree(sbyte angle)
		{
			return (angle / 256f) * 360f;
		}

		public static float AngleToNotchianDegree(sbyte angle)
		{
			return (float) (angle * 360) / 256.0F;
			return AngleToDegree(angle);
		}

		public static sbyte DegreeToAngle(float angle)
		{
			return (sbyte)(((angle % 360) / 360) * 256);
		}

		public static float FromFixedPoint(int f)
		{
			return ((float) f) / (32.0f * 128.0f);
		}

		public static uint Hash(uint value)
		{
			unchecked
			{
				value ^= value >> 16;
				value *= 0x85ebca6b;
				value ^= value >> 13;
				value *= 0xc2b2ae35;
				value ^= value >> 16;
			}

			return value;
		}
		
		private static float HueToRgb(float v1, float v2, float vH)
		{
			vH += (vH < 0) ? 1 : 0;
			vH -= (vH > 1) ? 1 : 0;
			float ret = v1;

			if ((6 * vH) < 1)
			{
				ret = (v1 + (v2 - v1) * 6 * vH);
			}

			else if ((2 * vH) < 1)
			{
				ret = (v2);
			}

			else if ((3 * vH) < 2)
			{
				ret = (v1 + (v2 - v1) * ((2f / 3f) - vH) * 6f);
			}

			return MathF.Clamp(ret, 0f, 1f);
		}

		public static Color HslToRGB(float h, float s, float l)
		{
			var c = new Color();

			if (s == 0)
			{
				c.R = (byte)(l * 255f);
				c.G = (byte)(l * 255f);
				c.B = (byte)(l * 255f);
			}
			else
			{
				float v2 = (l + s) - (s * l);
				if (l < 0.5f)
				{
					v2 = l * (1 + s);
				}
				float v1 = 2f * l - v2;

				c.R = (byte)(255f * HueToRgb(v1, v2, h + (1f / 3f)));
				c.G = (byte)(255f * HueToRgb(v1, v2, h));
				c.B = (byte)(255f * HueToRgb(v1, v2, h - (1f / 3f)));
			}

			return c;
		}

		private static int[] MULTIPLY_DE_BRUIJN_BIT_POSITION = new int[] {0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9};

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
