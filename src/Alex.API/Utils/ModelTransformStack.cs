using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
	public interface ITransformation
	{
		Vector3 Translation { get; set; }
		Vector3 Rotation { get; set; }
		Vector3 Scale { get; set; }
	}
	
	public class ModelTransformStack
	{
		private Matrix Matrix { get; set; }
		private readonly Stack<TransformationState> _stack = new Stack<TransformationState>();
		
		private TransformationState State { get; set; }
		
		public ModelTransformStack()
		{
			//Push();
			Init();
		}

		public Matrix GetMatrix()
		{
			return Matrix;
		}
		
		private void Init()
		{
			Matrix = Matrix.Identity;
		}
		
		public void Push()
		{
			Matrix currentMatrix = Matrix;
			var stackValue = new TransformationState(currentMatrix);
			
			_stack.Push(stackValue);
			State = stackValue;
		}

		public void Pop()
		{
			if (_stack.Count == 0)
			{
				Init();

				return;
			}

			_stack.Pop();

			var peek = _stack.Peek();
			State = peek;
			Matrix = peek.Transformation;
		}

		public void ApplyTransformation(ITransformation transformation)
		{
			Translate(transformation.Translation);
			Rotate(transformation.Rotation);
			Scale(transformation.Scale);
		}
		
		public void Translate(Vector3 translation)
		{
			Translate(translation.X, translation.Y, translation.Z);
		}

		public void Translate(float x, float y, float z)
		{
			Matrix *= Matrix.CreateTranslation(
				ToRadians(x), ToRadians(y), ToRadians(z));
		}

		public void Rotate(Vector3 rotation)
		{
			Rotate(rotation.X, rotation.Y, rotation.Z);
		}
		
		public void Rotate(float x, float y, float z)
		{
			Matrix *= Matrix.CreateRotationY(ToRadians(y));
			Matrix *= Matrix.CreateRotationX(ToRadians(x));
			Matrix *= Matrix.CreateRotationZ(ToRadians(z));
			//Matrix *= Matrix.CreateFromQuaternion(GetQuaternion(ToRadians(x), ToRadians(y), ToRadians(z)));
		}

		public void Scale(Vector3 scale)
		{
			Scale(scale.X, scale.Y, scale.Z);
		}
		
		public void Scale(float x, float y, float z)
		{
			Matrix *= Matrix.CreateScale(x, y, z);
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

		private class TransformationState
		{
			public Matrix Transformation { get; }
			public TransformationState(Matrix matrix)
			{
				Transformation = matrix;
			}
		}
	}
}