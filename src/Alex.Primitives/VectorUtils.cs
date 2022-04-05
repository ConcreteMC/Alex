using System;

namespace Alex.Interfaces
{
	public static class VectorUtils
	{
		public static IVectorFactory VectorFactory => Primitives.Factory;
		
		/// <summary>Returns the distance between two vectors.</summary>
	    /// <param name="value1">The first vector.</param>
	    /// <param name="value2">The second vector.</param>
	    /// <returns>The distance between two vectors.</returns>
	    public static float Distance(IVector3 value1, IVector3 value2)
	    {
	      return MathF.Sqrt(DistanceSquared(value1, value2));
	    }

	    /*
	    /// <summary>Returns the distance between two vectors.</summary>
	    /// <param name="value1">The first vector.</param>
	    /// <param name="value2">The second vector.</param>
	    /// <param name="result">The distance between two vectors as an output parameter.</param>
	    public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
	    {
	      Vector3.DistanceSquared(ref value1, ref value2, out result);
	      result = MathF.Sqrt(result);
	    }*/

	    /// <summary>Returns the squared distance between two vectors.</summary>
	    /// <param name="value1">The first vector.</param>
	    /// <param name="value2">The second vector.</param>
	    /// <returns>The squared distance between two vectors.</returns>
	    public static float DistanceSquared(IVector3 value1, IVector3 value2) => (float) (((double) value1.X - (double) value2.X) * ((double) value1.X - (double) value2.X) + ((double) value1.Y - (double) value2.Y) * ((double) value1.Y - (double) value2.Y) + ((double) value1.Z - (double) value2.Z) * ((double) value1.Z - (double) value2.Z));

	    /*
	    /// <summary>Returns the squared distance between two vectors.</summary>
	    /// <param name="value1">The first vector.</param>
	    /// <param name="value2">The second vector.</param>
	    /// <param name="result">The squared distance between two vectors as an output parameter.</param>
	    public static void DistanceSquared(Vector3 value1, ref Vector3 value2, out float result) => result = (float) (((double) value1.X - (double) value2.X) * ((double) value1.X - (double) value2.X) + ((double) value1.Y - (double) value2.Y) * ((double) value1.Y - (double) value2.Y) + ((double) value1.Z - (double) value2.Z) * ((double) value1.Z - (double) value2.Z));
	    */

		
		public static IVector4 Add(IVector4 value1, IVector4 value2)
		{
			return VectorFactory.Vector4(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z, value1.W + value2.Z);
		}
		
		public static IVector3 Add(IVector3 value1, IVector3 value2)
		{
			return VectorFactory.Vector3(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);
		}
		
		public static IVector2 Add(IVector2 value1, IVector2 value2)
		{
			return VectorFactory.Vector2(value1.X + value2.X, value1.Y + value2.Y);
		}
		
		public static IVector4 Subtract(IVector4 value1, IVector4 value2)
		{
			return VectorFactory.Vector4(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z, value1.W - value2.Z);
		}
		
		public static IVector3 Subtract(IVector3 value1, IVector3 value2)
		{
			return VectorFactory.Vector3(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);
		}
		
		public static IVector2 Subtract(IVector2 value1, IVector2 value2)
		{
			return VectorFactory.Vector2(value1.X - value2.X, value1.Y - value2.Y);
		}

		public static IVector4 Multiply(IVector4 value1, IVector4 value2)
		{
			return VectorFactory.Vector4(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z, value1.W * value2.Z);
		}
		
		public static IVector4 Multiply(IVector4 value1, float value)
		{
			return VectorFactory.Vector4(value1.X * value, value1.Y * value, value1.Z * value, value1.W * value);
		}
		
		public static IVector3 Multiply(IVector3 value1, IVector3 value2)
		{
			return VectorFactory.Vector3(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z);
		}
		
		public static IVector3 Multiply(IVector3 value1, float value)
		{
			return VectorFactory.Vector3(value1.X * value, value1.Y * value, value1.Z * value);
		}
		
		public static IVector2 Multiply(IVector2 value1, IVector2 value2)
		{
			return VectorFactory.Vector2(value1.X * value2.X, value1.Y * value2.Y);
		}

		public static IVector2 Multiply(IVector2 value1, float value)
		{
			return VectorFactory.Vector2(value1.X * value, value1.Y * value);
		}
		
		public static IVector2 Lerp(IVector2 value1, IVector2 value2, float amount)
		{
			return VectorFactory.Vector2(Lerp(value1.X, value2.X, amount), Lerp(value1.Y, value2.Y, amount));
		}
		
		public static IVector3 Lerp(IVector3 value1, IVector3 value2, float amount)
		{
			return VectorFactory.Vector3(Lerp(value1.X, value2.X, amount), Lerp(value1.Y, value2.Y, amount), Lerp(value1.Z, value2.Z, amount));
		}
		
		public static IVector4 Lerp(IVector4 value1, IVector4 value2, float amount)
		{
			return VectorFactory.Vector4(Lerp(value1.X, value2.X, amount), Lerp(value1.Y, value2.Y, amount), Lerp(value1.Z, value2.Z, amount), Lerp(value1.W, value2.W, amount));
		}
		
		public static IVector3 LerpVector3Degrees(IVector3 start, IVector3 end, float amount)
		{
			if (amount >= 1f) return end;
			if (amount <= 0f) return start;

			return VectorFactory.Vector3(
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
		
		private static float Lerp(float start, float end, float amount)
		{
			if (amount >= 1f) return end;
			if (amount <= 0f) return start;

			return start + (end - start) * amount;
		}
	}
}