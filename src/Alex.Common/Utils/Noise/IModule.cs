using System;

namespace Alex.Common.Utils.Noise
{
	/// <summary>
	/// Abstract interface for noise modules.
	///
	/// A <i>noise module</i> is an object that calculates and outputs a value
	/// given a N-dimensional input value.
	///
	/// Each type of noise module uses a specific method to calculate an
	/// output value.  Some of these methods include:
	///
	/// - Calculating a value using a coherent-noise function or some other
	///   mathematical function.
	/// - Mathematically changing the output value from another noise module
	///   in various ways.
	/// - Combining the output values from two noise modules in various ways.
	///
	/// </summary>
	public interface IModule
	{
	}

	/// <summary>
	/// Abstract interface for noise modules that calculates and outputs a value
	/// given a one-dimensional input value.
	/// </summary>
	public interface IModule1D : IModule
	{
		#region Interaction

		/// <summary>
		/// Generates an output value given the coordinates of the specified input value.
		/// </summary>
		/// <param name="x">The input coordinate on the x-axis.</param>
		/// <returns>The resulting output value.</returns>
		float GetValue(float x);

		#endregion
	}

	/// <summary>
	/// Abstract interface for noise modules that calculates and outputs a value
	/// given a two-dimensional input value.
	/// </summary>
	public interface IModule2D : IModule
	{
		#region Interaction

		/// <summary>
		/// Generates an output value given the coordinates of the specified input value.
		/// </summary>
		/// <param name="x">The input coordinate on the x-axis.</param>
		/// <param name="y">The input coordinate on the y-axis.</param>
		/// <returns>The resulting output value.</returns>
		float GetValue(float x, float y);

		#endregion
	}

	/// <summary>
	/// Abstract interface for noise modules that calculates and outputs a value
	/// given a three-dimensional input value.
	/// </summary>
	public interface IModule3D : IModule
	{
		#region Interaction

		/// <summary>
		/// Generates an output value given the coordinates of the specified input value.
		/// </summary>
		/// <param name="x">The input coordinate on the x-axis.</param>
		/// <param name="y">The input coordinate on the y-axis.</param>
		/// <param name="z">The input coordinate on the z-axis.</param>
		/// <returns>The resulting output value.</returns>
		float GetValue(float x, float y, float z);

		#endregion
	}

	/// <summary>
	/// Abstract interface for noise modules that calculates and outputs a value
	/// given a four-dimensional input value.
	/// </summary>
	public interface IModule4D : IModule
	{
		#region Interaction

		/// <summary>
		/// Generates an output value given the coordinates of the specified input value.
		/// </summary>
		/// <param name="x">The input coordinate on the x-axis.</param>
		/// <param name="y">The input coordinate on the y-axis.</param>
		/// <param name="z">The input coordinate on the z-axis.</param>
		/// <param name="z">The input coordinate on the t-axis.</param>
		/// <returns>The resulting output value.</returns>
		float GetValue(float x, float y, float z, float t);

		#endregion
	}

	public static class Libnoise
	{
		#region Constants

		/// <summary>
		/// Version
		/// </summary>
		public const string Version = "1.0.0 B";

		/// <summary>
		/// Pi
		/// </summary>
		public const float Pi = 3.1415926535897932385f;

		/// <summary>
		/// Square root of 2.
		/// </summary>
		public const float Sqrt2 = 1.4142135623730950488f;

		/// <summary>
		/// Square root of 3.
		/// </summary>
		public const float Sqrt3 = 1.7320508075688772935f;

		/// <summary>
		/// Square root of 5.
		/// </summary>
		public const float Sqrt5 = 2.2360679774997896964f;

		/// <summary>
		/// Converts an angle from degrees to radians.
		/// </summary>
		public const float Deg2Rad = Pi / 180.0f;

		/// <summary>
		/// Converts an angle from radians to degrees.
		/// </summary>
		public const float Rad2Deg = 1.0f / Deg2Rad;

		#endregion

		#region Misc

		/// <summary>
		/// Converts latitude/longitude coordinates on a unit sphere into 3D Cartesian coordinates. 
		/// </summary>
		/// <param name="lat">The latitude, in degrees. Must range from -90 to +90.</param>
		/// <param name="lon">The longitude, in degrees. Must range from -180 to +180.</param>
		/// <param name="x">By ref, this parameter contains the x coordinate.</param>
		/// <param name="y">By ref, this parameter contains the y coordinate.</param>
		/// <param name="z">By ref, this parameter contains the z coordinate.</param>
		public static void LatLonToXYZ(float lat, float lon, ref float x, ref float y, ref float z)
		{
			var r = (float)Math.Cos(Deg2Rad * lat);
			x = r * (float)Math.Cos(Deg2Rad * lon);
			y = (float)Math.Sin(Deg2Rad * lat);
			z = r * (float)Math.Sin(Deg2Rad * lon);
		}

		#endregion

		#region Interpolation methods

		/// <summary>
		/// Performs linear interpolation between two byte-values by a.
		///
		/// The amount value should range from 0.0 to 1.0.  If the amount value is
		/// 0.0, this function returns n0.  If the amount value is 1.0, this
		/// function returns n1.
		/// </summary>
		/// <param name="n0">The first value.</param>
		/// <param name="n1">The second value.</param>
		/// <param name="a">the amount to interpolate between the two values.</param>
		/// <returns>The interpolated value.</returns>
		public static byte Lerp(byte n0, byte n1, float a)
		{
			float c0 = n0 / 255.0f;
			float c1 = n1 / 255.0f;

			return (byte)((c0 + a * (c1 - c0)) * 255.0f);
		}

		/// <summary>
		/// Performs linear interpolation between two float-values by a.
		///
		/// The amount value should range from 0.0 to 1.0.  If the amount value is
		/// 0.0, this function returns n0.  If the amount value is 1.0, this
		/// function returns n1.
		/// </summary>
		/// <param name="n0">The first value.</param>
		/// <param name="n1">The second value.</param>
		/// <param name="a">The amount to interpolate between the two values.</param>
		/// <returns>The interpolated value.</returns>
		public static float Lerp(float n0, float n1, float a)
		{
			//return ((1.0 - a) * n0) + (a * n1);
			return n0 + a * (n1 - n0);
		}

		/// <summary>
		/// Performs cubic interpolation between two values bound between two other values.
		///
		/// The amount value should range from 0.0 to 1.0.  If the amount value is
		/// 0.0, this function returns n1.  If the amount value is 1.0, this
		/// function returns n2.
		/// </summary>
		/// <param name="n0">The value before the first value.</param>
		/// <param name="n1">The first value.</param>
		/// <param name="n2">The second value.</param>
		/// <param name="n3">The value after the second value.</param>
		/// <param name="a">The amount to interpolate between the two values.</param>
		/// <returns>The interpolated value.</returns>
		public static float Cerp(float n0, float n1, float n2, float n3, float a)
		{
			float p = (n3 - n2) - (n0 - n1);
			float q = (n0 - n1) - p;
			float r = n2 - n0;
			float s = n1;
			return p * a * a * a + q * a * a + r * a + s;
		}

		/// <summary>
		/// Maps a value onto a cubic S-curve.
		/// a should range from 0.0 to 1.0.
		/// The derivitive of a cubic S-curve is zero at a = 0.0 and a = 1.0
		/// </summary>
		/// <param name="a">The value to map onto a cubic S-curve.</param>
		/// <returns>The mapped value.</returns>
		public static float SCurve3(float a)
		{
			return (a * a * (3.0f - 2.0f * a));
		}

		/// <summary>
		/// Maps a value onto a quintic S-curve.
		/// a should range from 0.0 to 1.0.
		/// The first derivitive of a quintic S-curve is zero at a = 0.0 and a = 1.0.
		/// The second derivitive of a quintic S-curve is zero at a = 0.0 and a = 1.0.
		/// </summary>
		/// <param name="a">The value to map onto a quintic S-curve.</param>
		/// <returns>The mapped value.</returns>
		public static float SCurve5(float a)
		{
			return a * a * a * (a * (a * 6.0f - 15.0f) + 10.0f);

			/* original libnoise code
			double a3 = a * a * a;
			double a4 = a3 * a;
			double a5 = a4 * a;
			return (6.0 * a5) - (15.0 * a4) + (10.0 * a3);
			*/
		}

		#endregion

		#region Variables utility

		/// <summary>
		/// Clamps a value onto a clamping range.
		///
		/// This function does not modify any parameters.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="lowerBound">The lower bound of the clamping range</param>
		/// <param name="upperBound">The upper bound of the clamping range</param>
		/// <returns>		
		/// - value if value lies between lowerBound and upperBound.
		/// - lowerBound if value is less than lowerBound.
		/// - upperBound if value is greater than upperBound.
		/// </returns>
		public static int Clamp(int value, int lowerBound, int upperBound)
		{
			if (value < lowerBound)
				return lowerBound;
			if (value > upperBound)
				return upperBound;
			return value;
		}

		public static float Clamp(float value, float lowerBound, float upperBound)
		{
			if (value < lowerBound)
				return lowerBound;
			if (value > upperBound)
				return upperBound;
			return value;
		}

		public static double Clamp(double value, double lowerBound, double upperBound)
		{
			if (value < lowerBound)
				return lowerBound;
			if (value > upperBound)
				return upperBound;
			return value;
		}

		public static int Clamp01(int value)
		{
			return Clamp(value, 0, 1);
		}

		public static float Clamp01(float value)
		{
			return Clamp(value, 0, 1);
		}

		public static double Clamp01(double value)
		{
			return Clamp(value, 0, 1);
		}

		/// <summary>
		/// Swaps two values.
		/// 
		/// The values within the the two variables are swapped.
		/// </summary>
		/// <param name="a">A variable containing the first value.</param>
		/// <param name="b">A variable containing the second value.</param>
		public static void SwapValues<T>(ref T a, ref T b)
		{
			T c = a;
			a = b;
			b = c;
		}

		public static void SwapValues(ref double a, ref double b)
		{
			SwapValues<double>(ref a, ref b);
		}

		public static void SwapValues(ref int a, ref int b)
		{
			SwapValues<int>(ref a, ref b);
		}

		public static void SwapValues(ref float a, ref float b)
		{
			SwapValues<float>(ref a, ref b);
		}

		/// <summary>
		/// Modifies a floating-point value so that it can be stored in a
		/// int 32 bits variable.
		///
		/// In libnoise, the noise-generating algorithms are all integer-based;
		/// they use variables of type int 32 bits.  Before calling a noise
		/// function, pass the x, y, and z coordinates to this function to
		/// ensure that these coordinates can be cast to a int 32 bits value.
		///
		/// Although you could do a straight cast from double to int 32 bits, the
		/// resulting value may differ between platforms.  By using this function,
		/// you ensure that the resulting value is identical between platforms.
		/// </summary>
		/// <param name="value">A floating-point number.</param>
		/// <returns>The modified floating-point number.</returns>
		public static double ToInt32Range(double value)
		{
			if (value >= 1073741824.0)
				return (2.0 * Math.IEEERemainder(value, 1073741824.0)) - 1073741824.0;
			if (value <= -1073741824.0)
				return (2.0 * Math.IEEERemainder(value, 1073741824.0)) + 1073741824.0;
			return value;
		}

		/// <summary>
		/// Unpack the given integer (int32) value to an array of 4 bytes in big endian format.
		/// If the length of the buffer is too smal, it wil be resized.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="buffer">The output buffer.</param>
		public static byte[] UnpackBigUint32(int value, ref byte[] buffer)
		{
			if (buffer.Length < 4)
				Array.Resize(ref buffer, 4);

			buffer[0] = (byte)(value >> 24);
			buffer[1] = (byte)(value >> 16);
			buffer[2] = (byte)(value >> 8);
			buffer[3] = (byte)(value);

			return buffer;
		}

		/// <summary>
		/// Unpack the given float to an array of 4 bytes in big endian format.
		/// If the length of the buffer is too smal, it wil be resized.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="buffer">The output buffer.</param>
		public static byte[] UnpackBigFloat(float value, ref byte[] buffer)
		{
			throw new NotImplementedException();
			/*
			if(buffer.Length < 4) {
				Array.Resize<byte>(ref buffer, 4);
			}
			
			buffer[0] = (byte)(value >> 24);
			buffer[1] = (byte)(value >> 16);
			buffer[2] = (byte)(value >> 8);
			buffer[3] = (byte)(value);
			
			return buffer;
			*/
		}

		/// <summary>
		/// Unpack the given short (int16) value to an array of 2 bytes in big endian format.
		/// If the length of the buffer is too smal, it wil be resized.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="buffer">The output buffer.</param>
		public static byte[] UnpackBigUint16(short value, ref byte[] buffer)
		{
			if (buffer.Length < 2)
				Array.Resize(ref buffer, 2);

			buffer[0] = (byte)(value >> 8);
			buffer[1] = (byte)(value);

			return buffer;
		}

		/// <summary>
		/// Unpack the given short (int16) to an array of 2 bytes  in little endian format.
		/// If the length of the buffer is too smal, it wil be resized.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="buffer">The output buffer.</param>
		public static byte[] UnpackLittleUint16(short value, ref byte[] buffer)
		{
			if (buffer.Length < 2)
				Array.Resize(ref buffer, 2);

			buffer[0] = (byte)(value & 0x00ff);
			buffer[1] = (byte)((value & 0xff00) >> 8);

			return buffer;
		}

		/// <summary>
		/// Unpack the given integer (int32) to an array of 4 bytes  in little endian format.
		/// If the length of the buffer is too smal, it wil be resized.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="buffer">The output buffer.</param>
		public static byte[] UnpackLittleUint32(int value, ref byte[] buffer)
		{
			if (buffer.Length < 4)
				Array.Resize(ref buffer, 4);

			buffer[0] = (byte)(value & 0x00ff);
			buffer[1] = (byte)((value & 0xff00) >> 8);
			buffer[2] = (byte)((value & 0x00ff0000) >> 16);
			buffer[3] = (byte)((value & 0xff000000) >> 24);

			return buffer;
		}

		/// <summary>
		/// Unpack the given float (int32) to an array of 4 bytes  in little endian format.
		/// If the length of the buffer is too smal, it wil be resized.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="buffer">The output buffer.</param>
		public static byte[] UnpackLittleFloat(float value, ref byte[] buffer)
		{
			throw new NotImplementedException();

			/*
			if(buffer.Length < 4) {
				Array.Resize<byte>(ref buffer, 4);
			}
			buffer[0] = (byte)(value & 0x00ff);
			buffer[1] = (byte)((value & 0xff00) >> 8);
			buffer[2] = (byte)((value & 0x00ff0000) >> 16);
			buffer[3] = (byte)((value & 0xff000000) >> 24);
			
			return buffer;
*/
		}

		/// <summary>
		/// faster methid than using (int)Math.floor(x).
		/// </summary>
		/// <param name="x"></param>
		public static int FastFloor(double x)
		{
			return x >= 0 ? (int)x : (int)x - 1;
		}

		/// <summary>
		/// faster methid than using (int)Math.floor(x).
		/// </summary>
		/// <param name="x">The x.</param>
		public static int FastFloor(float x)
		{
			return x >= 0 ? (int)x : (int)x - 1;
		}

		#endregion
	}
}
