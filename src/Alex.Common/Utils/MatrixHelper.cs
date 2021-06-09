using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils
{
	public class MatrixHelper
	{
		public static Matrix CreateRotationDegrees(Vector3 input)
		{
			return Matrix.CreateRotationX(input.X.ToRadians()) 
			       * Matrix.CreateRotationY(input.Y.ToRadians())
			       * Matrix.CreateRotationZ(input.Z.ToRadians());
		}
		
		public static Matrix CreateRotation(Vector3 input)
		{
			return Matrix.CreateRotationX(input.X) 
			       * Matrix.CreateRotationY(input.Y)
			       * Matrix.CreateRotationZ(input.Z);
		}
	}
}