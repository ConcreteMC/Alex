using System.Collections.Generic;
using Alex.Common.Utils.Vectors;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils
{
	public class MatrixStack
	{
		/// <summary>
		/// Retrieves the Top node matrix of the stack
		/// </summary>
		public Matrix Top = Matrix.Identity;

		public object Tag = null;
		private List<Matrix> mStack = new List<Matrix>();

		/// <summary>
		///
		/// </summary>
		public MatrixStack()
		{
			LoadIdentity();
		}

		/// <summary>
		/// Clears the stack and loads the Identity Matrix in the top of the stack
		/// </summary>
		public void LoadIdentity()
		{
			mStack.Clear();
			Top = Matrix.Identity;
		}

		/// <summary>
		/// Clears the Stack, and loads the matrix in the top of the stack
		/// </summary>
		/// <param name="pMat"></param>
		public void LoadMatrix(Matrix pMat)
		{
			mStack.Clear();
			Top = pMat;
		}

		/// <summary>
		/// Adds a new level to the stack, cloning the current TOP matrix of the stack
		/// </summary>
		public void Push()
		{
			mStack.Add(Top);
		}

		/// <summary>
		/// Removes the current TOP matrix of the stacks, returning back to the previous one
		/// </summary>
		public void Pop()
		{
			if (mStack.Count > 0)
			{
				Top = mStack[mStack.Count - 1];
				mStack.RemoveAt(mStack.Count - 1);
			}
		}

		/// <summary>
		/// This method right-multiplies the given matrix to the current matrix (transformation is about the current world origin).
		/// This method does not add an item to the stack, it replaces the current matrix with the product of the current matrix and the given matrix.
		/// </summary>
		/// <param name="pMat"></param>
		public void MultiplyMatrix(Matrix pMat)
		{
			Matrix.Multiply(ref Top, ref pMat, out Top);
		}

		/// <summary>
		/// This method left-multiplies the given matrix to the current matrix (transformation is about the local origin of the object).
		/// This method does not add an item to the stack, it replaces the current matrix with the product of the given matrix and the current matrix.
		/// </summary>
		/// <param name="pMat"></param>
		public void MultiplyMatrixLocal(Matrix pMat)
		{
			Matrix.Multiply(ref pMat, ref Top, out Top);
		}

		public void RotateDegrees(Vector3 rotation)
		{
			Matrix tmp = Matrix.CreateRotationX(rotation.X.ToRadians()) * Matrix.CreateRotationY(rotation.Y.ToRadians())
			                                                            * Matrix.CreateRotationZ(
				                                                            rotation.Z.ToRadians());

			Matrix.Multiply(ref Top, ref tmp, out Top);
		}

		public void RotateDegreesLocal(Vector3 rotation)
		{
			Matrix tmp = Matrix.CreateRotationX(rotation.X.ToRadians()) * Matrix.CreateRotationY(rotation.Y.ToRadians())
			                                                            * Matrix.CreateRotationZ(
				                                                            rotation.Z.ToRadians());

			Matrix.Multiply(ref tmp, ref Top, out Top);
		}

		/// <summary>
		/// Rotates (relative to world coordinate space) around an arbitrary axis.
		/// </summary>
		public void RotateAxis(Vector3 pAxis, float pAngle)
		{
			Matrix tmp;
			Matrix.CreateFromAxisAngle(ref pAxis, pAngle, out tmp);
			Matrix.Multiply(ref Top, ref tmp, out Top);
		}

		/// <summary>
		/// Rotates (relative to world coordinate space) around an arbitrary axis.
		/// </summary>
		public void RotateAxisLocal(Vector3 pAxis, float pAngle)
		{
			Matrix tmp;
			Matrix.CreateFromAxisAngle(ref pAxis, pAngle, out tmp);
			Matrix.Multiply(ref tmp, ref Top, out Top);
		}

		/// <summary>
		/// Rotates (relative to world coordinate space) the specified Euler Angles
		/// </summary>
		public void RotateYawPitchRoll(float pYaw, float pPitch, float pRoll)
		{
			Matrix tmp;
			Matrix.CreateFromYawPitchRoll(pYaw, pPitch, pRoll, out tmp);
			Matrix.Multiply(ref Top, ref tmp, out Top);
		}

		/// <summary>
		/// Rotates (relative to world coordinate space) the specified Euler Angles
		/// </summary>
		public void RotateYawPitchRollLocal(float pYaw, float pPitch, float pRoll)
		{
			Matrix tmp;
			Matrix.CreateFromYawPitchRoll(pYaw, pPitch, pRoll, out tmp);
			Matrix.Multiply(ref tmp, ref Top, out Top);
		}

		/// <summary>
		/// Scale the current matrix about the world coordinate origin
		/// </summary>
		public void Scale(Vector3 scale)
		{
			Matrix tmp;
			Matrix.CreateScale(scale.X, scale.Y, scale.Z, out tmp);
			Matrix.Multiply(ref Top, ref tmp, out Top);
		}

		/// <summary>
		/// Scale the current matrix about the world coordinate origin
		/// </summary>
		public void ScaleLocal(Vector3 scale)
		{
			Matrix tmp;
			Matrix.CreateScale(scale.X, scale.Y, scale.Z, out tmp);
			Matrix.Multiply(ref tmp, ref Top, out Top);
		}

		/// <summary>
		/// Determines the product of the current matrix and the computed translation matrix determined by the given factors (x, y, and z).
		/// </summary>
		public void Translate(Vector3 amount)
		{
			Matrix tmp;
			Matrix.CreateTranslation(amount.X, amount.Y, amount.Z, out tmp);
			Matrix.Multiply(ref Top, ref tmp, out Top);
		}

		/// <summary>
		/// Determines the product of the current matrix and the computed translation matrix determined by the given factors (x, y, and z).
		/// </summary>
		public void TranslateLocal(Vector3 amount)
		{
			Matrix tmp;
			Matrix.CreateTranslation(amount.X, amount.Y, amount.Z, out tmp);
			Matrix.Multiply(ref tmp, ref Top, out Top);
		}
	}
}