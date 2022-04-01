using Alex.Interfaces;

namespace Alex.Networking.Java.Models
{
    public class Vector3I : IVector3I
    {
        public Vector3I(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
    }
}