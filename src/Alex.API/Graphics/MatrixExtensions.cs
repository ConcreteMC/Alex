using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Graphics
{
    public static class MatrixExtensions
    {
	    public static Matrix Rotate(this Matrix matrix, float angle, Vector3 axis)
	    {
		    return Rotate(matrix, angle, axis, matrix);
	    }

	    /**
	     * Rotates the matrix around the given axis the specified angle
	     * @param angle the angle, in radians.
	     * @param axis The vector representing the rotation axis. Must be normalized.
	     * @param dest The matrix to put the result, or null if a new matrix is to be created
	     * @return The rotated matrix
	     */
	    public static Matrix Rotate(this Matrix matrix, float angle, Vector3 axis, Matrix dest)
	    {
		    return Rotate(angle, axis, matrix, dest);
	    }

        public static Matrix Rotate(float angle, Vector3 axis, Matrix src, Matrix dest)
	    {
		    if (dest == null)
			    dest = new Matrix();
		    float c = (float)Math.Cos(angle);
		    float s = (float)Math.Sin(angle);
		    float oneminusc = 1.0f - c;
		    float xy = axis.X * axis.Y;
		    float yz = axis.Y * axis.Z;
		    float xz = axis.X * axis.Z;
		    float xs = axis.X * s;
		    float ys = axis.Y * s;
		    float zs = axis.Z * s;

		    float f00 = axis.X * axis.X * oneminusc + c;
		    float f01 = xy * oneminusc + zs;
		    float f02 = xz * oneminusc - ys;
		    // n[3] not used
		    float f10 = xy * oneminusc - zs;
		    float f11 = axis.Y * axis.Y * oneminusc + c;
		    float f12 = yz * oneminusc + xs;
		    // n[7] not used
		    float f20 = xz * oneminusc + ys;
		    float f21 = yz * oneminusc - xs;
		    float f22 = axis.Z * axis.Z * oneminusc + c;

		    float t00 = src.M11 * f00 + src.M21 * f01 + src.M31 * f02;
		    float t01 = src.M12 * f00 + src.M22 * f01 + src.M31 * f02;
		    float t02 = src.M13 * f00 + src.M23 * f01 + src.M33 * f02;
		    float t03 = src.M14 * f00 + src.M24 * f01 + src.M34 * f02;

		    float t10 = src.M11 * f10 + src.M21 * f11 + src.M31 * f12;
		    float t11 = src.M12 * f10 + src.M21 * f11 + src.M32 * f12;
		    float t12 = src.M13 * f10 + src.M22 * f11 + src.M33 * f12;
		    float t13 = src.M14 * f10 + src.M23 * f11 + src.M34 * f12;

		    dest.M31 = src.M11 * f20 + src.M21 * f21 + src.M31 * f22;
		    dest.M32 = src.M12 * f20 + src.M22 * f21 + src.M32 * f22;
		    dest.M33 = src.M13 * f20 + src.M23 * f21 + src.M33 * f22;
		    dest.M34 = src.M14 * f20 + src.M24 * f21 + src.M34 * f22;
		    dest.M11 = t00;
		    dest.M12 = t01;
		    dest.M13 = t02;
		    dest.M14 = t03;
		    dest.M21 = t10;
		    dest.M22 = t11;
		    dest.M23 = t12;
		    dest.M24 = t13;
		    return dest;
        }
    }
}
