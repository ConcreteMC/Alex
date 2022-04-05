using Alex.Interfaces;

namespace Alex.Networking.Java.Models
{
    public struct Vector3I : IVector3I
    {
        public Vector3I(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }
    }
}