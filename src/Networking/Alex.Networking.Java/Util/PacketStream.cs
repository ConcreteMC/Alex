using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.Interfaces;
using Alex.Networking.Java.Models;
using fNbt;

namespace Alex.Networking.Java.Util
{
	public class PacketStream : Stream, IAsyncMinecraftStream
	{
		protected Stream BaseStream;

		private bool IsDataAvailable(Stream stream)
		{
			if (stream is NetworkStream ns)
			{
				return ns.DataAvailable;
			}
			else if (stream is CipherStream cs)
			{
				return IsDataAvailable(cs.Stream);
			}

			return stream.Position < stream.Length;
		}

		public bool DataAvailable
		{
			get
			{
				return IsDataAvailable(BaseStream);
			}
		}

		protected CancellationTokenSource CancellationToken { get; }

		protected PacketStream(byte[] data) : this(new MemoryStream(data), System.Threading.CancellationToken.None) { }

		public PacketStream(Stream baseStream, CancellationToken cancellationToken)
		{
			BaseStream = baseStream;
			CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		}

		/// <inheritdoc />
		public override bool CanRead => BaseStream.CanRead;

		/// <inheritdoc />
		public override bool CanSeek => BaseStream.CanSeek;

		/// <inheritdoc />
		public override bool CanWrite => BaseStream.CanWrite;

		/// <inheritdoc />
		public override long Length => BaseStream.Length;

		/// <inheritdoc />
		public override long Position
		{
			get
			{
				return BaseStream.Position;
			}
			set
			{
				BaseStream.Position = value;
			}
		}

		/// <inheritdoc />
		public override int Read(byte[] buffer, int offset, int count)
		{
			return BaseStream.Read(buffer, offset, count);
		}

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin)
		{
			return BaseStream.Seek(offset, origin);
		}

		/// <inheritdoc />
		public override void SetLength(long value)
		{
			BaseStream.SetLength(value);
		}

		/// <inheritdoc />
		public override void Flush()
		{
			BaseStream.Flush();
		}

		public override async Task<int> ReadAsync(byte[] buffer,
			int offset,
			int count,
			CancellationToken cancellationToken)
		{
			try
			{
				var read = await BaseStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);

				return read;
			}
			catch (Exception)
			{
				return 0;
			} //TODO better handling of this
		}

		public virtual async Task<int> ReadAsync(byte[] buffer, CancellationToken cancellationToken = default)
		{
			try
			{
				var read = await this.BaseStream.ReadAsync(buffer, cancellationToken);

				return read;
			}
			catch (Exception)
			{
				return 0;
			} //TODO better handling of this
		}

		public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			await BaseStream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.BaseStream.Write(buffer, offset, count);
		}

		public async Task<byte[]> ReadAsync(int length)
		{
			Memory<byte> buffer = new Memory<byte>(new byte[length]);
			int read = 0;

			do
			{
				int received = await ReadAsync(buffer.Slice(read));

				if (received > 0)
				{
					read += received;
				}
			} while (read < length && !CancellationToken.IsCancellationRequested);

			return buffer.ToArray();
		}

		/// <inheritdoc />
		public async Task WriteAsync(byte[] data)
		{
			await this.BaseStream.WriteAsync(data);
		}

		private int ToInt(byte[] data)
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
		}

		public async Task WriteByteAsync(byte value)
		{
			await WriteAsync(new byte[] { value });
		}

		public sbyte ReadSignedByte() => (sbyte)this.ReadUnsignedByte();

		public async Task<sbyte> ReadByteAsync() => (sbyte)await this.ReadUnsignedByteAsync();

		public byte ReadUnsignedByte()
		{
			Span<byte> buffer = stackalloc byte[1];
			BaseStream.Read(buffer);

			return buffer[0];
		}

		public async Task<byte> ReadUnsignedByteAsync()
		{
			var buffer = new byte[1];
			await this.ReadAsync(buffer);

			return buffer[0];
		}

		/// <inheritdoc />
		public async Task<int> ReadIntAsync()
		{
			return ToInt(await ReadAsync(4));
		}

		/// <inheritdoc />
		public async Task<float> ReadFloatAsync()
		{
			return EndianUtils.NetworkToHostOrder(BitConverter.ToSingle(await ReadAsync(4), 0));
		}

		/// <inheritdoc />
		public async Task<bool> ReadBoolAsync()
		{
			return (await ReadUnsignedByteAsync()) == 1;
		}

		/// <inheritdoc />
		public async Task<double> ReadDoubleAsync()
		{
			return EndianUtils.NetworkToHostOrder(await ReadAsync(8));
		}

		/// <inheritdoc />
		public async Task<int> ReadVarIntAsync()
		{
			int numRead = 0;
			int result = 0;
			byte read;

			do
			{
				read = await ReadUnsignedByteAsync();

				int value = (read & 0x7f);
				result |= (value << (7 * numRead));

				numRead++;

				if (numRead > 5)
				{
					throw new Exception("VarInt is too big");
				}
			} while ((read & 0x80) != 0);

			return result;
		}

		/// <inheritdoc />
		public async Task<long> ReadVarLongAsync()
		{
			int numRead = 0;
			long result = 0;
			byte read;

			do
			{
				read = await ReadUnsignedByteAsync();
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

		/// <inheritdoc />
		public async Task<short> ReadShortAsync()
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(await ReadAsync(2), 0));
		}

		/// <inheritdoc />
		public async Task<ushort> ReadUShortAsync()
		{
			return EndianUtils.NetworkToHostOrder(BitConverter.ToUInt16(await ReadAsync(2), 0));
		}

		/// <inheritdoc />
		public async Task<string> ReadStringAsync()
		{
			return Encoding.UTF8.GetString(await ReadAsync(await ReadVarIntAsync()));
		}

		/// <inheritdoc />
		public async Task<long> ReadLongAsync()
		{
			return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(await ReadAsync(8), 0));
		}

		/// <inheritdoc />
		public async Task<ulong> ReadULongAsync()
		{
			return EndianUtils.NetworkToHostOrder(BitConverter.ToUInt64(await ReadAsync(8), 0));
		}

		/// <inheritdoc />
		public async Task<IVector3> ReadPositionAsync()
		{
			var val = await ReadULongAsync();
			var x = Convert.ToSingle(val >> 38);
			var y = Convert.ToSingle(val & 0xFFF);
			var z = Convert.ToSingle(val << 26 >> 38);

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

			return new NetworkVector3(x, y, z);
		}

		/// <inheritdoc />
		public async Task<IVector3I> ReadBlockCoordinatesAsync()
		{
			ulong value = await ReadULongAsync();

			long x = (long)(value >> 38);
			long y = (long)(value & 0xFFF);
			long z = (long)(value << 26 >> 38);

			if (x >= Math.Pow(2, 25))
				x -= (long)Math.Pow(2, 26);

			if (y >= Math.Pow(2, 11))
				y -= (long)Math.Pow(2, 12);

			if (z >= Math.Pow(2, 25))
				z -= (long)Math.Pow(2, 26);

			return new Vector3I((int)x, (int)y, (int)z);
			
			/*
			var val = await ReadLongAsync();
			var x = (val >> 38);
			var y = (val & 0xFFF);

			//if (y > 2048)
			//	y = -(0xFFF - y);

			var z = ((val << 38 >> 38) >> 12); //Convert.ToSingle((val << 38 >> 38) >> 12);

			if (x >= (2 ^ 25))
			{
				x -= 2 ^ 26;
			}

			//if (y >= (2^11))
			//{
			//    y -= 2^12;
			//}

			if (z >= (2 ^ 25))
			{
				z -= 2 ^ 26;
			}

			return new Vector3I((int)x, (int)y, (int)z);*/
		}

		/// <inheritdoc />
		public async Task<SlotData> ReadSlotAsync()
		{
			bool present = await ReadBoolAsync();

			if (!present) return null;

			int id = await ReadVarIntAsync();
			byte count = 0;
			NbtCompound nbt = null;

			count = await ReadUnsignedByteAsync();
			nbt = await ReadNbtCompoundAsync();


			SlotData slot = new SlotData();
			slot.Count = count;
			slot.Nbt = nbt;
			slot.ItemID = id;
			//	slot.ItemDamage = damage;

			return slot;
		}

		/// <inheritdoc />
		public async Task WriteSlotAsync(SlotData slot)
		{
			await WriteBoolAsync(slot != null && slot.ItemID != -1);

			if (slot == null)
				return;

			await WriteVarIntAsync(slot.ItemID);
			await WriteByteAsync(slot.Count);
			await WriteNbtCompoundAsync(slot.Nbt);
		}

		/// <inheritdoc />
		public async Task WritePositionAsync(NetworkVector3 position)
		{
			var x = Convert.ToInt64(position.X);
			var y = Convert.ToInt64(position.Y);
			var z = Convert.ToInt64(position.Z);
			long toSend = ((x & 0x3FFFFFF) << 38) | ((z & 0x3FFFFFF) << 12) | (y & 0xFFF);
			await WriteLongAsync(toSend);
		}

		/// <inheritdoc />
		public async Task WritePositionAsync(IVector3I pos)
		{
			await WritePositionAsync(new NetworkVector3(pos.X, pos.Y, pos.Z));
		}

		/// <inheritdoc />
		public async Task<int> WriteRawVarInt32Async(uint value)
		{
			int written = 0;

			while ((value & -128) != 0)
			{
				await WriteByteAsync((byte)((value & 0x7F) | 0x80));
				value >>= 7;
			}

			await WriteByteAsync((byte)value);
			written++;

			return written;
		}

		/// <inheritdoc />
		public async Task<int> WriteVarIntAsync(int value)
		{
			return await WriteRawVarInt32Async((uint)value);
		}

		/// <inheritdoc />
		public async Task<int> WriteVarLongAsync(long value)
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

				await WriteByteAsync(temp);
				write++;
			} while (value != 0);

			return write;
		}

		/// <inheritdoc />
		public async Task WriteIntAsync(int data)
		{
			await WriteAsync(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data)));
		}

		/// <inheritdoc />
		public async Task WriteStringAsync(string data)
		{
			var stringData = Encoding.UTF8.GetBytes(data);
			await WriteVarIntAsync(stringData.Length);
			await WriteAsync(stringData);
		}

		/// <inheritdoc />
		public async Task WriteShortAsync(short data)
		{
			await WriteAsync(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data)));
		}

		/// <inheritdoc />
		public async Task WriteUShortAsync(ushort data)
		{
			await WriteAsync(BitConverter.GetBytes(data));
		}

		/// <inheritdoc />
		public async Task WriteBoolAsync(bool data)
		{
			await WriteByteAsync((byte)(data ? 1 : 0));
		}

		/// <inheritdoc />
		public async Task WriteDoubleAsync(double data)
		{
			await WriteAsync(EndianUtils.HostToNetworkOrder(data));
		}

		/// <inheritdoc />
		public async Task WriteFloatAsync(float data)
		{
			await WriteAsync(EndianUtils.HostToNetworkOrder(data));
		}

		/// <inheritdoc />
		public async Task WriteLongAsync(long data)
		{
			await WriteAsync(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data)));
		}

		/// <inheritdoc />
		public async Task WriteULongAsync(ulong data)
		{
			await WriteAsync(EndianUtils.HostToNetworkOrderLong(data));
		}

		/// <inheritdoc />
		public async Task WriteUuidAsync(Guid uuid)
		{
			var guid = uuid.ToByteArray();
			var long1 = new byte[8];
			var long2 = new byte[8];
			Array.Copy(guid, 0, long1, 0, 8);
			Array.Copy(guid, 8, long2, 0, 8);
			await WriteAsync(long1);
			await WriteAsync(long2);
		}

		/// <inheritdoc />
		public async Task<Guid> ReadUuidAsync()
		{
			var long1 = await ReadAsync(8);
			var long2 = await ReadAsync(8);
			var concat = long1.Concat(long2).ToArray();

			return new Guid(concat);
			//return new MiNET.Utils.UUID(long1.Concat(long2).ToArray());
		}

		/// <inheritdoc />
		public async Task<NbtCompound> ReadNbtCompoundAsync()
		{
			//return Task.Run(
			//	 (Func<NbtCompound>)(() =>
			//	{
			NbtTagType t = (NbtTagType)(await ReadUnsignedByteAsync());

			if (t != NbtTagType.Compound) return null;
			Position--;

			NbtFile file = new NbtFile() { BigEndian = true, UseVarInt = false };

			file.LoadFromStream(this, NbtCompression.None);

			return (NbtCompound)file.RootTag;
			//	}));
		}

		/// <inheritdoc />
		public Task WriteNbtCompoundAsync(NbtCompound compound)
		{
			return Task.Run(
				() =>
				{
					if (compound == null)
					{
						WriteByte(0);

						return;
					}

					NbtFile f = new NbtFile(compound) { BigEndian = true, UseVarInt = false };
					f.SaveToStream(this, NbtCompression.None);
				});
		}

		/// <inheritdoc />
		public async Task<string> ReadChatObjectAsync()
		{
			return await ReadStringAsync();

			/*
			if (ChatObject.TryParse(raw, out string result))
			{
				return result;
			}

			return raw; // new ChatObject(raw);*/
		}
	}
}