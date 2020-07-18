using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
	public class Transformation
	{
		public Vector3 Translation { get; set; }
		public Vector3 Rotation { get; set; }
		public Vector3 Scale { get; set; }
	}
	
	public class ModelTransformation
	{
		private Matrix _matrix;

		public Matrix Build()
		{
			return _matrix;
		}
		
		public ModelTransformation()
		{
			_matrix = Matrix.Identity;
		}

		public ModelTransformation(Matrix matrix)
		{
			_matrix = matrix;
		}

		public void Translate(Vector3 translation)
		{
			Translate(translation.X, translation.Y, translation.Z);
		}

		public void Translate(float x, float y, float z)
		{
			_matrix *= Matrix.CreateTranslation(
				ToRadians(x), ToRadians(y), ToRadians(z));
		}

		public void Rotate(Vector3 rotation)
		{
			Rotate(rotation.X, rotation.Y, rotation.Z);
		}
		
		public void Rotate(float x, float y, float z)
		{
			_matrix *= Matrix.CreateFromQuaternion(GetQuaternion(ToRadians(x), ToRadians(y), ToRadians(z)));
		}

		public void Scale(Vector3 scale)
		{
			Scale(scale.X, scale.Y, scale.Z);
		}
		
		public void Scale(float x, float y, float z)
		{
			_matrix *= Matrix.CreateScale(ToRadians(x), ToRadians(y), ToRadians(z));
		}

		private static Quaternion GetQuaternion(float x, float y, float z)
		{
			float f  = x * 0.017453292F;
			float f1 = y * 0.017453292F;
			float f2 = z * 0.017453292F;
			float f3 = MathF.Sin(0.5F * f);
			float f4 = MathF.Cos(0.5F * f);
			float f5 = MathF.Sin(0.5F * f1);
			float f6 = MathF.Cos(0.5F * f1);
			float f7 = MathF.Sin(0.5F * f2);
			float f8 = MathF.Cos(0.5F * f2);

			return new Quaternion(
				f3 * f6 * f8 + f4 * f5 * f7, f4 * f5 * f8 - f3 * f6 * f7, f3 * f5 * f8 + f4 * f6 * f7,
				f4 * f6 * f8 - f3 * f5 * f7);
		}
		
		static float ToRadians(float deg)
		{
			return (System.MathF.PI * deg) / 180F;
		}
	}
}