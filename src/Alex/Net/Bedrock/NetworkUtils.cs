using System;
using System.IO;
using System.Text;
using Alex.API.Utils;
using fNbt;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using NLog;

namespace Alex.Net.Bedrock
{
	public class MetadataNbt : MetadataEntry
	{
		public Nbt Nbt { get; set; }
		public MetadataNbt(Nbt nbtCompound)
		{
			Nbt = nbtCompound;
		}
		
		public MetadataNbt(){}
		
		/// <inheritdoc />
		public override void FromStream(BinaryReader reader)
		{
			Nbt = NetworkUtils.ReadNbt(reader.BaseStream, true);
		}

		/// <inheritdoc />
		public override void WriteTo(BinaryWriter stream)
		{
			Nbt.NbtFile.UseVarInt = true;
			Nbt.NbtFile.SaveToStream(stream.BaseStream, NbtCompression.None);
		}

		/// <inheritdoc />
		public override byte Identifier => 5;

		/// <inheritdoc />
		public override string FriendlyName => "NBT";
	}
	public static class NetworkUtils
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(NetworkUtils));

		public static Nbt ReadNbt(Stream stream, bool useVarInt = false)
		{
			Nbt     nbt     = new Nbt();
			NbtFile nbtFile = new NbtFile();
			nbtFile.BigEndian = false;
			nbtFile.UseVarInt = useVarInt;
			nbt.NbtFile = nbtFile;
			nbtFile.LoadFromStream(stream, NbtCompression.None);
			return nbt;
		}

		public static MetadataDictionary ReadMetadataDictionaryAlternate(this Packet packet)
		{
			MetadataDictionary metadata = new MetadataDictionary(); 
			uint count = 0;
			int index;
			uint type;
			try
			{
				//var stream = (Stream)ReflectionHelper.GetPrivateFieldValue<MemoryStreamReader>(typeof(Packet), packet, "_reader");
				count = packet.ReadUnsignedVarInt(); //VarInt.ReadInt32(stream);

				for (int i = 0; i < count; ++i)
				{
					index = (int) packet.ReadUnsignedVarInt(); //VarInt.ReadInt32(stream);
					type  = packet.ReadUnsignedVarInt(); //VarInt.ReadInt32(stream);

					switch (type)
					{
						case 0:
							metadata[index] = new MetadataByte(packet.ReadByte());

							break;

						case 1:
							metadata[index] = new MetadataShort(packet.ReadShort());

							break;

						case 2:
							metadata[index] = new MetadataInt(packet.ReadVarInt());

							break;

						case 3:
							metadata[index] = new MetadataFloat(packet.ReadFloat());

							break;

						case 4:
							metadata[index] = new MetadataString(packet.ReadString());

							break;

						case 5:
							metadata[index] = new MetadataNbt(packet.ReadNbt());

							break;

						case 6:
							metadata[index] = new MetadataIntCoordinates(
								packet.ReadVarInt(), packet.ReadVarInt(), packet.ReadVarInt());

							break;

						case 7:
							metadata[index] = new MetadataLong(packet.ReadVarLong());

							break;

						case 8:
							metadata[index] = new MetadataVector3(packet.ReadVector3());

							break;
						default:
							Log.Warn($"Unknown metadata type: {type} at index {index}");
							break;
					}
				}


				return metadata;
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Incomplete metadata: {ex.ToString()}");
				return metadata;
			}
		}
		
		public static ItemStacks ReadItemStacksAlternate(this Packet packet)
		{
			ItemStacks itemStacks = new ItemStacks();
			uint       num        = packet.ReadUnsignedVarInt();
			for (int index = 0; (long) index < (long) num; ++index)
				itemStacks.Add(packet.AlternativeReadItem());
			return itemStacks;
		}

		public static Item AlternativeReadItem(this Packet packet)
		{
			int id = packet.ReadSignedVarInt();

			if (id == 0)
			{
				return new ItemAir();
			}

			int   tmp      = packet.ReadSignedVarInt();
			short metadata = (short) (tmp >> 8);

			if (metadata == short.MaxValue)
				metadata = 0;

			byte count = (byte) (tmp & 0xff);
			Item stack = ItemFactory.GetItem((short) id, metadata, count);

			ushort nbtLen = packet.ReadUshort(); // NbtLen

			if (nbtLen == 0xffff)
			{
				if (packet.ReadByte() != 1)
				{
					Log.Warn($"Invalid NBT while reading item data...");
				}
				
				stack.ExtraData = (NbtCompound) packet.ReadNbt().NbtFile.RootTag;
			}
			else if (nbtLen != 0)
			{
				var stream = (Stream)ReflectionHelper.GetPrivateFieldValue<MemoryStreamReader>(typeof(Packet), packet, "_reader");
				stack.ExtraData = (NbtCompound) ReadNbt(stream).NbtFile.RootTag;
			}

			for (int i = 0, canPlace = packet.ReadVarInt(); i < canPlace; ++i)
			{
				packet.ReadString();
			}

			for (int i = 0, canBreak = packet.ReadVarInt(); i < canBreak; ++i)
			{
				packet.ReadString();
			}

			if (id == 513) // shield
			{
				packet.ReadVarLong(); // something about tick, crap code
			}

			return stack;
		}
	}
}