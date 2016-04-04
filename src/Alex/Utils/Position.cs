using System;
using System.Runtime.InteropServices;

namespace Alex.Utils
{
    /// <summary>
    ///     Struct representing a position AND orientation in 3D space.
    ///     Used to represent players' positions in Minecraft world.
    ///     Stores X/Y/Z as signed shorts, and R/L as unsigned bytes (8 bytes total).
    ///     Use Vector3I if you just need X/Y/Z coordinates without orientation.
    ///     Note that, as a struct, Positions are COPIED when assigned or passed as an argument.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Position : IEquatable<Position>
    {
        /// <summary> Position at (0,0,0) with R=0 and L=0. </summary>
        public static readonly Position Zero = new Position(0, 0, 0);

        public readonly short X, Y, Z;
        public readonly byte R, L;


        public Position(Position other, byte r, byte l)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
            R = r;
            L = l;
        }


        public Position(short x, short y, short z, byte r, byte l)
        {
            X = x;
            Y = y;
            Z = z;
            R = r;
            L = l;
        }


        public Position(int x, int y, int z)
        {
            X = (short) x;
            Y = (short) y;
            Z = (short) z;
            R = 0;
            L = 0;
        }


        internal bool FitsIntoMoveRotatePacket
        {
            get
            {
                return X >= sbyte.MinValue && X <= sbyte.MaxValue &&
                       Y >= sbyte.MinValue && Y <= sbyte.MaxValue &&
                       Z >= sbyte.MinValue && Z <= sbyte.MaxValue;
            }
        }


        public bool IsZero
        {
            get { return X == 0 && Y == 0 && Z == 0 && R == 0 && L == 0; }
        }


        // adjust for bugs in position-reporting in Minecraft client
        public Position GetFixed()
        {
            return new Position(X, Y, (short) (Z - 22), R, L);
        }


        public int DistanceSquaredTo(Position other)
        {
            return (X - other.X)*(X - other.X) + (Y - other.Y)*(Y - other.Y) +
                   (Z - other.Z)*(Z - other.Z);
        }

        #region Equality

        public static bool operator ==(Position a, Position b)
        {
            return a.Equals(b);
        }


        public static bool operator !=(Position a, Position b)
        {
            return !a.Equals(b);
        }


        public bool Equals(Position other)
        {
            return (X == other.X) && (Y == other.Y) && (Z == other.Z) && (R == other.R) && (L == other.L);
        }


        public override bool Equals(object obj)
        {
            return obj is Position && Equals((Position) obj);
        }


        public override int GetHashCode()
        {
            return (X + Y*short.MaxValue) ^ R + L*short.MaxValue + Z;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Position({0},{1},{2} @{3},{4})", X, Y, Z, R, L);
        }


        public static explicit operator Vector3I(Position a)
        {
            return new Vector3I(a.X, a.Y, a.Z);
        }


        public Vector3I ToVector3I()
        {
            return new Vector3I(X, Y, Z);
        }

        public Vector3I ToBlockCoords()
        {
            return new Vector3I((X - 16)/32, (Y - 16)/32, (Z - 16)/32);
        }
    }
}