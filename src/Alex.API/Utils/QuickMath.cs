using System;

namespace Alex.API.Utils
{
	public static class QuickMath
	{
		public static readonly double HalfPi = Math.PI / 2;
		public static readonly double Tau = Math.PI * 2;

		/**
		 * @return The floor of d
		 */
		public static long Floor(double d)
		{
			long i = (long)d;
			return d < i ? i - 1 : i;
		}

		/**
		 * @return The ceil of d
		 */
		public static long Ceil(double d)
		{
			long i = (long)d;
			return d > i ? i + 1 : i;
		}

		/**
		 * Get the next power of two.
		 *
		 * @return The next power of two
		 */
		public static int NextPow2(int x)
		{
			x--;
			x |= x >> 1;
			x |= x >> 2;
			x |= x >> 4;
			x |= x >> 8;
			x |= x >> 16;
			return x + 1;
		}

		/**
		 * @return the 2-logarithm of x, rounded down.
		 */
		public static int Log2(int x)
		{
			int v = 0;
			while ((x >>= 1) != 0)
			{
				v += 1;
			}
			return v;
		}

		/**
		 * @return The sign of x
		 */
		public static int Signum(double x)
		{
			return x < 0 ? -1 : 1;
		}

		/**
		 * @return The sign of x
		 */
		public static int Signum(float x)
		{
			return x < 0 ? -1 : 1;
		}

		/**
		 * Convert radians to degrees
		 *
		 * @param rad Radians
		 * @return Degrees
		 */
		public static double RadToDeg(double rad)
		{
			return 180 * (rad / Math.PI);
		}

		/**
		 * @return Value modulo mod
		 */
		public static double Modulo(double value, double mod)
		{
			return ((value % mod) + mod) % mod;
		}

		/**
		 * Convert degrees to radians
		 *
		 * @param deg Degrees
		 * @return Radians
		 */
		public static double DegToRad(double deg)
		{
			return (deg * Math.PI) / 180;
		}

		/**
		 * @return value clamped to min and max
		 */
		public static double Clamp(double value, double min, double max)
		{
			return value < min ? min : value > max ? max : value;
		}

		/**
		 * NB not NaN-correct
		 *
		 * @return maximum value of a and b
		 */
		public static double Max(double a, double b)
		{
			return (a > b) ? a : b;
		}

		/**
		 * NB not NaN-correct
		 *
		 * @return maximum value of a and b
		 */
		public static float Max(float a, float b)
		{
			return (a > b) ? a : b;
		}

		/**
		 * NB disregards NaN. Don't use if a or b can be NaN
		 *
		 * @return minimum value of a and b
		 */
		public static double Min(double a, double b)
		{
			return (a < b) ? a : b;
		}

		/**
		 * NB disregards NaN. Don't use if a or b can be NaN
		 *
		 * @return minimum value of a and b
		 */
		public static float Min(float a, float b)
		{
			return (a < b) ? a : b;
		}

		/**
		 * NB disregards +-0
		 *
		 * @return absolute value of x
		 */
		public static float Abs(float x)
		{
			return (x < 0f) ? -x : x;
		}

		/**
		 * NB disregards +-0
		 *
		 * @return absolute value of x
		 */
		public static double Abs(double x)
		{
			return (x < 0.0) ? -x : x;
		}
	}
}
