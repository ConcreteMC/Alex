using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils.Vectors
{
	public readonly struct BlockCoordinates : IEquatable<BlockCoordinates>
	{
		public readonly int X;
		public readonly int Y;
		public readonly int Z;

		public BlockCoordinates(int value)
		{
			X = Y = Z = value;
		}

		public BlockCoordinates(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public BlockCoordinates(BlockCoordinates v)
		{
			X = v.X;
			Y = v.Y;
			Z = v.Z;
		}

		public BlockCoordinates(PlayerLocation location)
		{
			X = (int)Math.Floor(location.X);
			Y = (int)Math.Floor(location.Y);
			Z = (int)Math.Floor(location.Z);
		}


		/// <summary>
		/// Calculates the distance between two BlockCoordinates objects.
		/// </summary>
		public double DistanceTo(BlockCoordinates other)
		{
			return Math.Sqrt(Square(other.X - X) +
							 Square(other.Y - Y) +
							 Square(other.Z - Z));
		}

		/// <summary>
		/// Calculates the square of a num.
		/// </summary>
		private int Square(int num)
		{
			return num * num;
		}

		public BlockCoordinates Abs()
		{
			return new BlockCoordinates(
				Math.Abs(X),
				Math.Abs(Y),
				Math.Abs(Z)
				);
		}

		/// <summary>
		/// Finds the distance of this Coordinate3D from BlockCoordinates.Zero
		/// </summary>
		public double Distance
		{
			get { return DistanceTo(Zero); }
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates Min(BlockCoordinates value1, BlockCoordinates value2)
		{
			return new BlockCoordinates(
				Math.Min(value1.X, value2.X),
				Math.Min(value1.Y, value2.Y),
				Math.Min(value1.Z, value2.Z)
				);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates Max(BlockCoordinates value1, BlockCoordinates value2)
		{
			return new BlockCoordinates(
				Math.Max(value1.X, value2.X),
				Math.Max(value1.Y, value2.Y),
				Math.Max(value1.Z, value2.Z)
				);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(BlockCoordinates a, BlockCoordinates b)
		{
			return !a.Equals(b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(BlockCoordinates a, BlockCoordinates b)
		{
			return a.Equals(b);
		}

		public static BlockCoordinates operator +(BlockCoordinates a, BlockCoordinates b)
		{
			return new BlockCoordinates(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator -(BlockCoordinates a, BlockCoordinates b)
		{
			return new BlockCoordinates(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator -(BlockCoordinates a)
		{
			return new BlockCoordinates(-a.X, -a.Y, -a.Z);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator *(BlockCoordinates a, BlockCoordinates b)
		{
			return new BlockCoordinates(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator /(BlockCoordinates a, BlockCoordinates b)
		{
			return new BlockCoordinates(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator %(BlockCoordinates a, BlockCoordinates b)
		{
			return new BlockCoordinates(a.X % b.X, a.Y % b.Y, a.Z % b.Z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator +(BlockCoordinates a, int b)
		{
			return new BlockCoordinates(a.X + b, a.Y + b, a.Z + b);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator -(BlockCoordinates a, int b)
		{
			return new BlockCoordinates(a.X - b, a.Y - b, a.Z - b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator *(BlockCoordinates a, int b)
		{
			return new BlockCoordinates(a.X * b, a.Y * b, a.Z * b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator /(BlockCoordinates a, int b)
		{
			return new BlockCoordinates(a.X / b, a.Y / b, a.Z / b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator %(BlockCoordinates a, int b)
		{
			return new BlockCoordinates(a.X % b, a.Y % b, a.Z % b);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator +(int a, BlockCoordinates b)
		{
			return new BlockCoordinates(a + b.X, a + b.Y, a + b.Z);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator -(int a, BlockCoordinates b)
		{
			return new BlockCoordinates(a - b.X, a - b.Y, a - b.Z);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator *(int a, BlockCoordinates b)
		{
			return new BlockCoordinates(a * b.X, a * b.Y, a * b.Z);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator /(int a, BlockCoordinates b)
		{
			return new BlockCoordinates(a / b.X, a / b.Y, a / b.Z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BlockCoordinates operator %(int a, BlockCoordinates b)
		{
			return new BlockCoordinates(a % b.X, a % b.Y, a % b.Z);
		}

		public static explicit operator BlockCoordinates(ChunkCoordinates a)
		{
			return new BlockCoordinates(a.X, 0, a.Z);
		}

		public static implicit operator BlockCoordinates(Vector3 a)
		{
			//return new BlockCoordinates((int)(a.X), (int)(a.Y), (int)(a.Z));
			return new BlockCoordinates((int)Math.Floor(a.X), (int)Math.Floor(a.Y), (int)Math.Floor(a.Z));
		}

		public static explicit operator BlockCoordinates(PlayerLocation a)
		{
			return new BlockCoordinates((int)Math.Floor(a.X), (int)Math.Floor(a.Y), (int)Math.Floor(a.Z));
		}

		public static implicit operator Vector3(BlockCoordinates a)
		{
			return new Vector3(a.X, a.Y, a.Z);
		}

		public static readonly BlockCoordinates Zero = new BlockCoordinates(0);
		public static readonly BlockCoordinates One = new BlockCoordinates(1);

		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components 0, 1, 0
		/// </summary>
		public static readonly BlockCoordinates Up = new BlockCoordinates(0, 1, 0);
		
		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components 0, -1, 0
		/// </summary>
		public static readonly BlockCoordinates Down = new BlockCoordinates(0, -1, 0);
		
		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components -1, 0, 0
		/// </summary>
		public static readonly BlockCoordinates Left = new BlockCoordinates(-1, 0, 0);
		
		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components 1, 0, 0
		/// </summary>
		public static readonly BlockCoordinates Right = new BlockCoordinates(1, 0, 0);
		
		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components 0, 0, -1
		/// </summary>
		public static readonly BlockCoordinates Backwards = new BlockCoordinates(0, 0, -1);
		
		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components 0, 0, 1
		/// </summary>
		public static readonly BlockCoordinates Forwards = new BlockCoordinates(0, 0, 1);

		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components 1, 0, 0
		/// </summary>
		public static readonly BlockCoordinates East = new BlockCoordinates(1, 0, 0);
		
		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components -1, 0, 0
		/// </summary>
		public static readonly BlockCoordinates West = new BlockCoordinates(-1, 0, 0);
		
		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components 0, 0, -1
		/// </summary>
		public static readonly BlockCoordinates North = new BlockCoordinates(0, 0, -1);
		
		/// <summary>
		///		Returns a <see cref="BlockCoordinates"/> with components 0, 0, 1
		/// </summary>
		public static readonly BlockCoordinates South = new BlockCoordinates(0, 0, 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(BlockCoordinates other)
		{
			return X.Equals(other.X)&& Y.Equals(other.Y) & Z.Equals(other.Z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is BlockCoordinates coordinates && Equals(coordinates);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]

		public override int GetHashCode()
		{
			var yHash = Y.GetHashCode();
			var zHash = Z.GetHashCode();

			return X.GetHashCode() ^ (yHash << 4) ^ (yHash >> 28) ^ (zHash >> 4) ^ (zHash << 28);
		}

		public override string ToString()
		{
			return $"X={X}, Y={Y}, Z={Z}";
		}

		public BlockCoordinates BlockUp()
		{
			return this + Up;
		}

		public BlockCoordinates BlockDown()
		{
			return this + Down;
		}

		public BlockCoordinates BlockLeft()
		{
			return this + Left;
		}

		public BlockCoordinates BlockRight()
		{
			return this + Right;
		}

		public BlockCoordinates BlockForwards()
		{
			return this + Forwards;
		}

		public BlockCoordinates BlockBackwards()
		{
			return this + Backwards;
		}
		
		public BlockCoordinates BlockWest()
		{
			return this + West;
		}

		public BlockCoordinates BlockEast()
		{
			return this + East;
		}

		public BlockCoordinates BlockNorth()
		{
			return this + North;
		}

		public BlockCoordinates BlockSouth()
		{
			return this + South;
		}
	}
}
