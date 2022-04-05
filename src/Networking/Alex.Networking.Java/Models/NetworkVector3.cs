using System;
using Alex.Interfaces;

namespace Alex.Networking.Java.Models
{
	public struct NetworkVector3 : IVector3
	{
		public static readonly NetworkVector3 Zero = new NetworkVector3(0, 0, 0);
		public NetworkVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public bool Equals(NetworkVector3 other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <inheritdoc />
		public bool Equals(IVector3 other)
		{
			if (other == null)
				return false;

			if (other is NetworkVector3 v3)
				return Equals(v3);
            
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;

			return Equals((NetworkVector3) obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y, Z);
		}
	}
}