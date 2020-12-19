using Microsoft.Xna.Framework;
using Valve.VR;

namespace SharpVR
{
    public static class SharpVrExtensions
    {
        public static Matrix ToMg(this HmdMatrix34_t mat)
        {
            var m = new Matrix(
                mat.m0, mat.m4, mat.m8, 0.0f,
                mat.m1, mat.m5, mat.m9, 0.0f,
                mat.m2, mat.m6, mat.m10, 0.0f,
                mat.m3, mat.m7, mat.m11, 1.0f);

            return Matrix.Invert(m);
            //return m;
        }

        public static Matrix ToMg(this HmdMatrix44_t mat)
        {
            var m = new Matrix(
                mat.m0, mat.m4, mat.m8, mat.m12,
                mat.m1, mat.m5, mat.m9, mat.m13,
                mat.m2, mat.m6, mat.m10, mat.m14,
                mat.m3, mat.m7, mat.m11, mat.m15);

            return m;
        }

        public static Vector3 ToMg(this HmdVector3_t vec)
        {
            var m = new Vector3(vec.v0, vec.v1, vec.v2);
            return m;
        }
    }
}
