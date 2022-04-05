using System;
using Alex.Interfaces;
using Microsoft.Xna.Framework;

namespace Alex.Common.Utils.Vectors
{
	public class PrimitiveFactory : IPrimitiveFactory
	{
		/// <inheritdoc />
		public IVector2 Vector2Zero { get; } = new Vector2Primitive(0, 0);

		/// <inheritdoc />
		public IVector2I Vector2IZero { get; }

		/// <inheritdoc />
		public IVector2 Vector2(float x, float y)
		{
			return new Vector2Primitive(x, y);
		}

		/// <inheritdoc />
		public IVector2I Vector2I(int x, int y)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public IVector3 Vector3Zero { get; } = new Vector3Primitive(0, 0, 0);

		/// <inheritdoc />
		public IVector3I Vector3IZero { get; } = BlockCoordinates.Zero;

		/// <inheritdoc />
		public IVector3 Vector3(float x, float y, float z)
		{
			return new Vector3Primitive(x, y, z);
		}

		/// <inheritdoc />
		public IVector3I Vector3I(int x, int y, int z)
		{
			return new BlockCoordinates(x, y, z);
		}

		/// <inheritdoc />
		public IVector4 Vector4Zero { get; }  = new Vector4Primitive(0, 0, 0, 0);

		/// <inheritdoc />
		public IVector4 Vector4(float x, float y, float z, float w)
		{
			return new Vector4Primitive(x, y, z, w);
		}

		/// <inheritdoc />
		public IVector4I Vector4I(int x, int y, int z, double w)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public IColor Color(byte r, byte g, byte b, byte a)
		{
			return new ColorPrimitive(r, g, b, a);
		}

		/// <inheritdoc />
		public IColor Color(byte r, byte g, byte b)
		{
			return new ColorPrimitive(r, g, b, 255);
		}

		/// <inheritdoc />
		public IColor Color(uint rgba)
		{
			return new ColorPrimitive(rgba);
		}

		/// <inheritdoc />
		public IColor Color(IVector4 vector)
		{
			return new ColorPrimitive(vector);
		}
		
		public IColor Color(IVector3 vector)
		{
			return new ColorPrimitive(vector);
		}
	}

	public struct ColorPrimitive : IColor, IEquatable<ColorPrimitive>
	{
		/// <inheritdoc />
		public byte R => (byte) (this._packedValue);

		/// <inheritdoc />
		public byte G => (byte) (this._packedValue >> 8);

		/// <inheritdoc />
		public byte B => (byte) (this._packedValue >> 16);

		/// <inheritdoc />
		public byte A => (byte) (this._packedValue >> 24);

		public ColorPrimitive(byte r, byte g, byte b, byte a)
		{
			this._packedValue = (uint) ((int) a << 24 | (int) b << 16 | (int) g << 8) | (uint) r;
		}

		private uint _packedValue = 0;
		public ColorPrimitive(uint rgba)
		{
			_packedValue = rgba;
		}

		public ColorPrimitive(IVector4 color) : this((byte) ((double) color.X * (double) byte.MaxValue), (byte) ((double) color.Y * (double) byte.MaxValue), (byte) ((double) color.Z * (double) byte.MaxValue), (byte) ((double) color.W * (double) byte.MaxValue))
		{
			
		}
		
		public ColorPrimitive(IVector3 color) : this((byte) ((double) color.X * (double) byte.MaxValue), (byte) ((double) color.Y * (double) byte.MaxValue), (byte) ((double) color.Z * (double) byte.MaxValue), 255)
		{
			
		}

		/// <inheritdoc />
		public bool Equals(IColor other)
		{
			if (other == null) return false;

			if (other is ColorPrimitive cp)
				return Equals(cp);

			return other.R == R && other.G == G && other.B == B && other.A == A;
		}

		/// <inheritdoc />
		public bool Equals(ColorPrimitive other)
		{
			return _packedValue == other._packedValue;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is ColorPrimitive other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return (int) _packedValue;
		}
	}
	
	public struct Vector3Primitive : IVector3
	{
		/// <inheritdoc />
		public float X { get; set; }

		/// <inheritdoc />
		public float Y { get; set; }

		/// <inheritdoc />
		public float Z { get; set; }

		public Vector3Primitive(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		
		public static implicit operator Vector3(Vector3Primitive primitive)
		{
			return new Vector3(primitive.X, primitive.Y,primitive.Z);
		}
		
		public static implicit operator Vector3Primitive(Vector3 primitive)
		{
			return new Vector3Primitive(primitive.X, primitive.Y,primitive.Z);
		}

		/// <inheritdoc />
		public bool Equals(IVector3 other)
		{
			if (other == null)
				return false;
			
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}
		
		public bool Equals(Vector3Primitive other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is Vector3Primitive other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y, Z);
		}
	}
	
	public struct Vector2Primitive : IVector2
	{
		/// <inheritdoc />
		public float X { get; set; }

		/// <inheritdoc />
		public float Y { get; set; }

		public Vector2Primitive(float x, float y)
		{
			X = x;
			Y = y;
		}
		
		public static implicit operator Vector2(Vector2Primitive primitive)
		{
			return new Vector2(primitive.X, primitive.Y);
		}
		
		public static implicit operator Vector2Primitive(Vector2 primitive)
		{
			return new Vector2Primitive(primitive.X, primitive.Y);
		}

		/// <inheritdoc />
		public bool Equals(IVector2 other)
		{
			if (other == null)
				return false;
			
			return X.Equals(other.X) && Y.Equals(other.Y);
		}
		
		public bool Equals(Vector2Primitive other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is Vector2Primitive other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y);
		}
	}

	public struct Vector4Primitive : IVector4
	{
		/// <inheritdoc />
		public float X { get; set; }

		/// <inheritdoc />
		public float Y { get; set; }

		/// <inheritdoc />
		public float Z { get; set; }
		
		public float W { get; set; }

		public Vector4Primitive(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
		
		public static implicit operator Vector4(Vector4Primitive primitive)
		{
			return new Vector4(primitive.X, primitive.Y,primitive.Z, primitive.W);
		}
		
		public static implicit operator Vector4Primitive(Vector4 primitive)
		{
			return new Vector4Primitive(primitive.X, primitive.Y,primitive.Z, primitive.W);
		}

		/// <inheritdoc />
		public bool Equals(IVector4 other)
		{
			if (other == null)
				return false;
			
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
		}
		
		public bool Equals(Vector4Primitive other)
		{
			return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is Vector4Primitive other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y, Z, W);
		}
	}
}