using System;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
    public static class Vector3Extension
    {
        public static Vector3I ToBlockCoords(this Vector3 vector)
        {
            return new Vector3I((int)Math.Round(vector.X), (int)Math.Round(vector.Y), (int)Math.Round(vector.Z));
        }
    }
}