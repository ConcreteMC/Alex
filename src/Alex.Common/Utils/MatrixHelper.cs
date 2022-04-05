using Alex.Common.Utils.Vectors;
using Alex.Interfaces;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils
{
	public class MatrixHelper
	{
		public static Quaternion FromRotationDegrees(float x, float y, float z)
		{
			return Quaternion.CreateFromRotationMatrix(
				Matrix.CreateRotationX(x.ToRadians()) * Matrix.CreateRotationY(y.ToRadians())
				                                      * Matrix.CreateRotationZ(z.ToRadians()));
		}

		public static Quaternion FromRotationDegrees(Vector3 input)
		{
			return FromRotationDegrees(input.X, input.Y, input.Z);
		}

		public static Matrix CreateRotationDegrees(Vector3 input)
		{
			return Matrix.CreateRotationX(input.X.ToRadians()) * Matrix.CreateRotationY(input.Y.ToRadians())
			                                                   * Matrix.CreateRotationZ(input.Z.ToRadians());
		}
		
		public static Matrix CreateRotationDegrees(IVector3 input)
		{
			return Matrix.CreateRotationX(input.X.ToRadians()) * Matrix.CreateRotationY(input.Y.ToRadians())
			                                                   * Matrix.CreateRotationZ(input.Z.ToRadians());
		}

		public static Matrix CreateRotation(Vector3 input)
		{
			return Matrix.CreateRotationX(input.X) * Matrix.CreateRotationY(input.Y) * Matrix.CreateRotationZ(input.Z);
		}
	}
}