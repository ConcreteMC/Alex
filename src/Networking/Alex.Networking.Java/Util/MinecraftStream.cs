using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Alex.Interfaces;
using Alex.Networking.Java.Models;
using fNbt;

namespace Alex.Networking.Java.Util
{
	public class MinecraftStream : PacketStream, IMinecraftStream
	{
		public MinecraftStream(Stream baseStream, CancellationToken cancellationToken = default) : base(
			baseStream, cancellationToken) { }

		public MinecraftStream(CancellationToken cancellationToken = default) : this(
			new MemoryStream(), cancellationToken) { }

		public void Read(Span<byte> memory, int count)
		{
			BaseStream.Read(memory.Slice(0, count));
			//var data = BaseStream.ReadToSpan(count);
			//data.CopyTo(memory);
		}

		public void Write(in Memory<byte> buffer, int offset, in int bufferLength)
		{
			var bytes = buffer.Slice(offset, bufferLength).ToArray();

			BaseStream.Write(bytes, offset, bytes.Length);
		}

		#region Reader

		public byte[] Read(int length)
		{
			if (BaseStream is MemoryStream)
			{
				var dat = new byte[length];
				Read(dat, 0, length);

				return dat;
			}

			//SpinWait s = new SpinWait();
			int read = 0;

			var buffer = new byte[length];

			while (read < buffer.Length && !CancellationToken.IsCancellationRequested)
			{
				int oldRead = read;

				int r = this.Read(buffer, read, length - read);

				if (r == 0) //No data read?
				{
					break;
				}

				read += r;

				if (CancellationToken.IsCancellationRequested)
					throw new ObjectDisposedException("");
			}

			if (read < length)
				throw new EndOfStreamException();

			return buffer;
		}


		public int ReadInt()
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(Read(4), 0));
		}

		public float ReadFloat()
		{
			return EndianUtils.NetworkToHostOrder(BitConverter.ToSingle(Read(4), 0));
		}

		public bool ReadBool()
		{
			return ReadUnsignedByte() == 1;
		}

		public double ReadDouble()
		{
			return EndianUtils.NetworkToHostOrder(Read(8));
		}

		public int ReadVarInt()
		{
			return ReadVarInt(out _);
		}

		public int ReadVarInt(out int bytesRead)
		{
			int numRead = 0;
			int result = 0;
			byte read;

			do
			{
				read = (byte)ReadUnsignedByte();
				;

				int value = (read & 0x7f);
				result |= (value << (7 * numRead));

				numRead++;

				if (numRead > 5)
				{
					throw new Exception("VarInt is too big");
				}
			} while ((read & 0x80) != 0);

			bytesRead = numRead;

			return result;
		}

		public long ReadVarLong()
		{
			int numRead = 0;
			long result = 0;
			byte read;

			do
			{
				read = (byte)ReadUnsignedByte();
				int value = (read & 0x7f);
				result |= (uint) (value << (7 * numRead));

				numRead++;

				if (numRead > 10)
				{
					throw new Exception("VarLong is too big");
				}
			} while ((read & 0x80) != 0);

			return result;
		}

		public short ReadShort()
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(Read(2), 0));
		}

		public ushort ReadUShort()
		{
			return EndianUtils.NetworkToHostOrder(BitConverter.ToUInt16(Read(2), 0));
		}

		public ushort[] ReadUShort(int count)
		{
			var us = new ushort[count];

			for (var i = 0; i < us.Length; i++)
			{
				var da = Read(2);
				var d = BitConverter.ToUInt16(da, 0);
				us[i] = d;
			}

			return EndianUtils.NetworkToHostOrder(us);
		}

		public ushort[] ReadUShortLocal(int count)
		{
			var us = new ushort[count];

			for (var i = 0; i < us.Length; i++)
			{
				var da = Read(2);
				var d = BitConverter.ToUInt16(da, 0);
				us[i] = d;
			}

			return us;
		}

		public short[] ReadShortLocal(int count)
		{
			var us = new short[count];

			for (var i = 0; i < us.Length; i++)
			{
				var da = Read(2);
				var d = BitConverter.ToInt16(da, 0);
				us[i] = d;
			}

			return us;
		}

		public string ReadString()
		{
			return Encoding.UTF8.GetString(Read(ReadVarInt()));
		}

		public long ReadLong()
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(Read(8), 0));
		}

		public ulong ReadULong()
		{
			return EndianUtils.NetworkToHostOrder(BitConverter.ToUInt64(Read(8), 0));
		}

		public IVector3 ReadPosition()
		{
			var val = ReadULong();
			var x = Convert.ToSingle(val >> 38);
			var y = Convert.ToSingle(val & 0xFFF);

			//if (y > 2048)
			//	y = -(0xFFF - y);

			var z = Convert.ToSingle(val << 26 >> 38); //Convert.ToSingle((val << 38 >> 38) >> 12);

			if (x >= (2 ^ 25))
			{
				x -= 2 ^ 26;
			}

			if (y >= (2 ^ 11))
			{
				y -= 2 ^ 12;
			}

			if (z >= (2 ^ 25))
			{
				z -= 2 ^ 26;
			}

			return Primitives.Factory.Vector3((int) x, (int) y, (int) z);
		}

		public IVector3I ReadBlockCoordinates()
		{
			ulong value = ReadULong();

			long x = (long)(value >> 38);
			long y = (long)(value & 0xFFF);
			long z = (long)(value << 26 >> 38);

			if (x >= Math.Pow(2, 25))
				x -= (long)Math.Pow(2, 26);

			if (y >= Math.Pow(2, 11))
				y -= (long)Math.Pow(2, 12);

			if (z >= Math.Pow(2, 25))
				z -= (long)Math.Pow(2, 26);

			return Primitives.Factory.Vector3I((int) x, (int) y, (int) z);
		}

		public SlotData ReadSlot()
		{
			bool present = ReadBool();

			if (!present) return null;

			int id = ReadVarInt();
			byte count = 0;
			NbtCompound nbt = null;

			count = (byte)ReadUnsignedByte();
			nbt = ReadNbtCompound();


			SlotData slot = new SlotData();
			slot.Count = count;
			slot.Nbt = nbt;
			slot.ItemID = id;
			//	slot.ItemDamage = damage;

			return slot;
		}

		public void WriteSlot(SlotData slot)
		{
			WriteBool(slot != null && slot.ItemID != -1);

			if (slot == null)
				return;

			WriteVarInt(slot.ItemID);
			WriteByte(slot.Count);
			WriteNbtCompound(slot.Nbt);
		}

		#endregion

		#region Writer

		public void Write(byte[] data)
		{
			this.Write(data, 0, data.Length);
		}

		public void WritePosition(IVector3 position)
		{
			var x = Convert.ToInt64(position.X);
			var y = Convert.ToInt64(position.Y);
			var z = Convert.ToInt64(position.Z);
			long toSend = ((x & 0x3FFFFFF) << 38) | ((z & 0x3FFFFFF) << 12) | (y & 0xFFF);
			WriteLong(toSend);
		}

		public void WritePosition(IVector3I pos)
		{
			WritePosition(new NetworkVector3(pos.X, pos.Y, pos.Z));
		}

		public int WriteRawVarInt32(uint value)
		{
			int written = 0;

			while ((value & -128) != 0)
			{
				WriteByte((byte)((value & 0x7F) | 0x80));
				value >>= 7;
			}

			WriteByte((byte)value);
			written++;

			return written;
		}

		public int WriteVarInt(int value)
		{
			return WriteRawVarInt32((uint)value);
		}

		public int WriteVarLong(long value)
		{
			int write = 0;

			do
			{
				byte temp = (byte)(value & 127);
				value >>= 7;

				if (value != 0)
				{
					temp |= 128;
				}

				WriteByte(temp);
				write++;
			} while (value != 0);

			return write;
		}

		public void WriteInt(int data)
		{
			Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data)));
		}

		public void WriteString(string data)
		{
			if (data == null)
			{
				WriteVarInt(0);

				return;
			}

			var stringData = Encoding.UTF8.GetBytes(data);
			WriteVarInt(stringData.Length);
			Write(stringData);
		}

		public void WriteShort(short data)
		{
			Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data)));
		}

		public void WriteUShort(ushort data)
		{
			Write(BitConverter.GetBytes(data));
		}

		public void WriteBool(bool data)
		{
			Write(BitConverter.GetBytes(data));
		}

		public void WriteDouble(double data)
		{
			Write(EndianUtils.HostToNetworkOrder(data));
		}

		public void WriteFloat(float data)
		{
			Write(EndianUtils.HostToNetworkOrder(data));
		}

		public void WriteLong(long data)
		{
			Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data)));
		}

		public void WriteULong(ulong data)
		{
			Write(EndianUtils.HostToNetworkOrderLong(data));
		}

		public void WriteUuid(Guid uuid)
		{
			var guid = uuid.ToByteArray();
			var long1 = new byte[8];
			var long2 = new byte[8];
			Array.Copy(guid, 0, long1, 0, 8);
			Array.Copy(guid, 8, long2, 0, 8);
			Write(long1);
			Write(long2);
		}

		public Guid ReadUuid()
		{
			var long1 = Read(8);
			var long2 = Read(8);

			return new Guid(long1.Concat(long2).ToArray());
		}

		#endregion

		private object _disposeLock = new object();
		private bool _disposed = false;

		protected MinecraftStream(byte[] data) : base(data) { }

		protected override void Dispose(bool disposing)
		{
			if (!Monitor.IsEntered(_disposeLock))
				return;

			try
			{
				if (disposing && !_disposed)
				{
					_disposed = true;

					if (!CancellationToken.IsCancellationRequested)
						CancellationToken.Cancel();
				}

				base.Dispose(disposing);
			}
			finally
			{
				Monitor.Exit(_disposeLock);
			}
		}

		public NbtCompound ReadNbtCompound()
		{
			NbtTagType t = (NbtTagType)ReadUnsignedByte();

			if (t != NbtTagType.Compound) return null;
			Position--;

			NbtFile file = new NbtFile() { BigEndian = true, UseVarInt = false };

			file.LoadFromStream(this, NbtCompression.None);

			return (NbtCompound)file.RootTag;
		}

		public void WriteNbtCompound(NbtCompound compound)
		{
			if (compound == null)
			{
				WriteByte(0);

				return;
			}

			NbtFile f = new NbtFile(compound) { BigEndian = true, UseVarInt = false };
			f.SaveToStream(this, NbtCompression.None);
			//WriteByte(0);
		}

		public string ReadChatObject()
		{
			var raw = ReadString();

			if (ChatObject.TryParse(raw, out string result))
			{
				return result;
			}

			return raw;
		}
	}
}