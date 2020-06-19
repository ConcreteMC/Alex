using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using NLog;
using Org.BouncyCastle.Bcpg;

namespace Alex.API.Utils
{
    public static class VarInt
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint EncodeZigZag32(int n)
		{
			// Note:  the right-shift must be arithmetic
			return (uint) ((n << 1) ^ (n >> 31));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int DecodeZigZag32(uint n)
		{
			return (int) (n >> 1) ^ -(int) (n & 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong EncodeZigZag64(long n)
		{
			return (ulong) ((n << 1) ^ (n >> 63));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long DecodeZigZag64(ulong n)
		{
			return (long) (n >> 1) ^ -(long) (n & 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint ReadRawVarInt32(SpanReader buf, int maxSize)
		{
			uint result = 0;
			int j = 0;
			int b0;

			do
			{
				b0 = buf.ReadByte(); // -1 if EOS
				if (b0 < 0) throw new EndOfStreamException("Not enough bytes for VarInt");

				result |= (uint) (b0 & 0x7f) << j++ * 7;

				if (j > maxSize)
				{
					throw new OverflowException("VarInt too big");
				}
			} while ((b0 & 0x80) == 0x80);

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong ReadRawVarInt64(SpanReader buf, int maxSize, bool printBytes = false)
		{
			List<byte> bytes = new List<byte>();

			ulong result = 0;
			int j = 0;
			int b0;

			do
			{
				b0 = buf.ReadByte(); // -1 if EOS
				bytes.Add((byte) b0);
				if (b0 < 0) throw new EndOfStreamException("Not enough bytes for VarInt");

				result |= (ulong) (b0 & 0x7f) << j++ * 7;

				if (j > maxSize)
				{
					throw new OverflowException("VarInt too big");
				}
			} while ((b0 & 0x80) == 0x80);

			//byte[] byteArray = bytes.ToArray();

			return result;
		}

	/*	[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void WriteRawVarInt32(SpanReader buf, uint value)
		{
			while ((value & -128) != 0)
			{
				buf.WriteByte((byte) ((value & 0x7F) | 0x80));
				value >>= 7;
			}

			buf.WriteByte((byte) value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void WriteRawVarInt64(SpanReader buf, ulong value)
		{
			while ((value & 0xFFFFFFFFFFFFFF80) != 0)
			{
				buf.WriteByte((byte) ((value & 0x7F) | 0x80));
				value >>= 7;
			}

			buf.WriteByte((byte) value);
		}
*/
		// Int

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt32(SpanReader stream, int value)
		{
			WriteRawVarInt32(stream, (uint) value);
		}*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ReadInt32(SpanReader stream)
		{
			return (int) ReadRawVarInt32(stream, 5);
		}

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteSInt32(SpanReader stream, int value)
		{
			WriteRawVarInt32(stream, EncodeZigZag32(value));
		}*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ReadSInt32(SpanReader stream)
		{
			return DecodeZigZag32(ReadRawVarInt32(stream, 5));
		}

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteUInt32(SpanReader stream, uint value)
		{
			WriteRawVarInt32(stream, value);
		}*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint ReadUInt32(SpanReader stream)
		{
			return ReadRawVarInt32(stream, 5);
		}

		// Long

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt64(SpanReader stream, long value)
		{
			WriteRawVarInt64(stream, (ulong) value);
		}*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ReadInt64(SpanReader stream, bool printBytes = false)
		{
			return (long) ReadRawVarInt64(stream, 10, printBytes);
		}

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteSInt64(SpanReader stream, long value)
		{
			WriteRawVarInt64(stream, EncodeZigZag64(value));
		}*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ReadSInt64(SpanReader stream)
		{
			return DecodeZigZag64(ReadRawVarInt64(stream, 10));
		}

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteUInt64(SpanReader stream, ulong value)
		{
			WriteRawVarInt64(stream, value);
		}*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong ReadUInt64(SpanReader stream)
		{
			return ReadRawVarInt64(stream, 10);
		}
	}
}