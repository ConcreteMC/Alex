using System;
using Alex.Interfaces;

namespace Alex.Networking.Java.Models
{
	public readonly struct NetworkColor : IColor
	{
		/// <inheritdoc />
		public byte R { get; }

		/// <inheritdoc />
		public byte G { get; }

		/// <inheritdoc />
		public byte B { get; }

		/// <inheritdoc />
		public byte A { get; }

		public NetworkColor(float r, float g, float b) : this((byte) (r * 255),(byte) (g * 255),(byte) (b * 255), 255)
		{
            
		}

		public NetworkColor(byte r, byte g, byte b) : this(r,g,b, 255)
		{
            
		}
        
		public NetworkColor(byte r, byte g, byte b, byte a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		/// <inheritdoc />
		public bool Equals(IColor other)
		{
			if (other == null)
				return false;

			if (other is NetworkColor v3)
				return Equals(v3);
            
			return R == other.R && G == other.G && B == other.B && A == other.A;
		}
        
		public bool Equals(NetworkColor other)
		{
			return R == other.R && G == other.G && B == other.B && A == other.A;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is NetworkColor other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(R, G, B, A);
		}
	}
}