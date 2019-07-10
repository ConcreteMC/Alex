using System;

namespace Alex.API.Utils
{
	public static class MathF
	{
		public static float Smoothstep(float edge0, float edge1, float x)
		{
			// Scale, bias and saturate x to 0..1 range
			x = Clamp((x - edge0) / (edge1 - edge0), 0.0F, 1.0F);
			// Evaluate polynomial
			return x * x * (3 - 2 * x);
		}

		public static float Clamp(float x, float lowerlimit, float upperlimit)
		{
			if (x < lowerlimit)
				x = lowerlimit;
			if (x > upperlimit)
				x = upperlimit;
			return x;
		}

		public static float Acos(float d)
		{
			return (float)Math.Acos(d);
		}

		public static float Asin(float d)
		{
			return (float)Math.Asin(d);
		}

		public static float Atan(float d)
		{
			return (float)Math.Atan(d);
		}

		public static float Atan2(float y, float x)
		{
			return (float)Math.Atan2(y, x);
		}

		public static float Ceiling(float a)
		{
			return (float)Math.Ceiling(a);
		}

		public static float Cos(float d)
		{
			return (float)Math.Cos(d);
		}

		public static float Cosh(float value)
		{
			return (float)Math.Cosh(value);
		}

		public static float Floor(float d)
		{
			return (float)Math.Floor(d);
		}

		public static float Sin(float a)
		{
			return (float)Math.Sin(a);
		}

		public static float Tan(float a)
		{
			return (float)Math.Tan(a);
		}

		public static float Sinh(float value)
		{
			return (float)Math.Sinh(value);
		}

		public static float Tanh(float value)
		{
			return (float)Math.Tanh(value);
		}

		public static float Round(float a)
		{
			return (float)Math.Round(a);
		}

		public static float Truncate(float d)
		{
			return (float)Math.Truncate(d);
		}

		public static float Sqrt(float d)
		{
			return (float)Math.Sqrt(d);
		}

		public static float Log(float d)
		{
			return (float)Math.Log(d);
		}

		public static float Log10(float d)
		{
			return (float)Math.Log10(d);
		}

		public static float Exp(float d)
		{
			return (float)Math.Exp(d);
		}

		public static float Pow(float x, float y)
		{
			return (float)Math.Pow(x, y);
		}

		public static float IEEERemainder(float x, float y)
		{
			return (float)Math.IEEERemainder(x, y);
		}

		public static float Abs(float value)
		{
			return (float)Math.Abs(value);
		}

		public static float Max(float val1, float val2)
		{
			return (float)Math.Max(val1, val2);
		}

		public static float Min(float val1, float val2)
		{
			return (float)Math.Min(val1, val2);
		}

		public static float Log(float a, float newBase)
		{
			return (float)Math.Log(a, newBase);
		}

	}
}
