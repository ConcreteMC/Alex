using System;
using System.IO;
using System.Linq;
using fNbt;
using fNbt.Tags;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Utils.Metadata;
using MiNET.Utils.Nbt;
using NLog;

namespace Alex.Net.Bedrock.Packets
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
		
		public static Nbt ReadNewNbt(Stream stream)
		{
			Nbt     nbt     = new Nbt();
			NbtFile nbtFile = new NbtFile();
			nbtFile.BigEndian = false;
			nbtFile.UseVarInt = true;
			nbtFile.AllowAlternativeRootTag = true;
			
			nbt.NbtFile = nbtFile;
			nbtFile.LoadFromStream(stream, NbtCompression.None);
			return nbt;
		}
		
		public static Nbt ReadLegacyNbt(Stream stream)
		{
			Nbt     nbt     = new Nbt();
			NbtFile nbtFile = new NbtFile();
			nbtFile.BigEndian = false;
			nbtFile.UseVarInt = true;
			nbtFile.AllowAlternativeRootTag = true;
			
			nbt.NbtFile = nbtFile;
			nbtFile.LoadFromStream(stream, NbtCompression.None);
			return nbt;
		}
		
		public static Item ReadItem2(this Packet packet, bool networkIds = true)
		{
			int id  = packet.ReadVarInt();

			if (id == 0)
			{
				return new ItemAir();
			}


			int   aux      = packet.ReadVarInt();
			short metadata = (short) (aux >> 8);
			if (metadata == short.MaxValue) metadata = -1;
			byte count = (byte) (aux & 0xff);
			
			//ItemFactory.Itemstates.FirstOrDefault(x => x.Id == )
			Item stack = ItemFactory.GetItem((short) id, metadata, count);

			var nbtLen = packet.ReadShort(); //_reader.ReadUInt16(); // NbtLen
			//Log.Info($"NBT: 0x{nbtLen:X}");
		
			if (nbtLen == -1)
			{
				var version = packet.ReadByte();

				if (version == 1)
				{
					stack.ExtraData = (NbtCompound) packet.ReadNbt().NbtFile.RootTag;
				}
				else
				{
					throw new NotSupportedException($"Invalid NBT data version: {version}");
				}
			}
			else if (nbtLen > 0)
			{
				//packet.ReadNbt();
				var nbtData = packet.ReadBytes(nbtLen);

				//	using (MemoryStream ms = new MemoryStream(nbtData))
				//{
				//		stack.ExtraData = (NbtCompound) ReadLegacyNbt(ms).NbtFile.RootTag;
				//stack.ExtraData = Packet.ReadLegacyNbtCompound(ms);
				//	}
			}

			var canPlace = packet.ReadVarInt();
			for (int i = 0; i < canPlace; i++)
			{
				packet.ReadString();
			}
			
			var canBreak = packet.ReadVarInt();
			for (int i = 0; i < canBreak; i++)
			{
				packet.ReadString();
			}

			if (id == 513) // shield
			{
				packet.ReadSignedVarLong(); // something about tick, crap code
			}

			return stack;
		}
		
		internal static bool ReadTag(this NbtTag tag, NbtBinaryReader readStream)
		{
			while (true)
			{
				NbtTag     nbtTag = null;
				NbtTagType nbtTagType;// = readStream.ReadTagType();
				do
				{
					nbtTagType = readStream.ReadTagType();
					
					switch (nbtTagType)
					{
						case NbtTagType.End:
							break;
						case NbtTagType.Byte:
							nbtTag = (NbtTag) new NbtByte();
							break;
						case NbtTagType.Short:
							nbtTag = (NbtTag) new NbtShort();
							break;
						case NbtTagType.Int:
							nbtTag = (NbtTag) new NbtInt();
							break;
						case NbtTagType.Long:
							nbtTag = (NbtTag) new NbtLong();
							break;
						case NbtTagType.Float:
							nbtTag = (NbtTag) new NbtFloat();
							break;
						case NbtTagType.Double:
							nbtTag = (NbtTag) new NbtDouble();
							break;
						case NbtTagType.ByteArray:
							nbtTag = (NbtTag) new NbtByteArray();
							break;
						case NbtTagType.String:
							nbtTag = (NbtTag) new NbtString();
							break;
						case NbtTagType.List:
							nbtTag = (NbtTag) new NbtList();
							break;
						case NbtTagType.Compound:
							nbtTag = (NbtTag) new NbtCompound();
							break;
						case NbtTagType.IntArray:
							nbtTag = (NbtTag) new NbtIntArray();
							break;
						case NbtTagType.LongArray:
							nbtTag = (NbtTag) new NbtLongArray();
							break;
						case NbtTagType.Unknown:
							break;
						default:
							throw new FormatException("Unsupported tag type found in NBT_Compound: " + (object) nbtTagType);
					}

					if (nbtTag != null)
					{
						nbtTag.Name = readStream.ReadString();
					}

					//nbtTag.Parent = (NbtTag) this;
					//nbtTag.Name = readStream.ReadString();
				}
				while (nbtTagType != NbtTagType.End && nbtTagType != NbtTagType.Unknown);

				switch (tag.TagType)
				{
					case NbtTagType.Compound:
						NbtCompound compound = (NbtCompound) tag;
						compound.Add(nbtTag);
						break;
					case NbtTagType.List:
						NbtList list = (NbtList) tag;
						list.Add(nbtTag);
						break;
				}
			}
		}
		
		internal static NbtTag ReadUnknownTag(NbtBinaryReader readStream)
		{
			NbtTagType nbtTagType = (NbtTagType) readStream.ReadByte(); //readStream.ReadTagType();
			if (nbtTagType == NbtTagType.Unknown)
				return new NbtCompound("");
			
			NbtTag     nbtTag;
			switch (nbtTagType)
			{
				case NbtTagType.End:
					throw new EndOfStreamException();
				case NbtTagType.List:
					nbtTag = (NbtTag) new NbtList();
					
					break;
				case NbtTagType.Compound:
					nbtTag = (NbtTag) new NbtCompound();
					break;
				default:
					throw new Exception("Unsupported tag type found in NBT_Tag: " + (object) nbtTagType);
			}
			nbtTag.Name = readStream.ReadString();
			if (nbtTag.ReadTag(readStream))
				return nbtTag;
			throw new Exception("Given NBT stream does not start with a proper TAG");
		}
		
		public static Nbt ReadNbt(Stream stream, bool useVarInt = false)
		{
			if (stream.ReadByte() == 255)
				return null;

			stream.Position -= 1;
			
			Nbt     nbt     = new Nbt();
			NbtFile nbtFile = new NbtFile();
			nbtFile.BigEndian = false;
			nbtFile.UseVarInt = useVarInt;
			
			nbtFile.LoadFromStream(stream, NbtCompression.None);
			
			/*var reader = new NbtBinaryReader(stream, false) {
				Selector = null,
				UseVarInt = useVarInt
			};

			nbtFile.RootTag = ReadUnknownTag(reader);*/
			
			nbt.NbtFile = nbtFile;
			
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

				for (int i = 0; i < count; i++)
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
		
		public static ItemStacks ReadItemStacksAlternate(this Packet packet, bool networkIds)
		{
			ItemStacks itemStacks = new ItemStacks();
			uint       num        = packet.ReadUnsignedVarInt();
			for (int index = 0; (long) index < (long) num; index++)
				itemStacks.Add(packet.AlternativeReadItem(networkIds));
			return itemStacks;
		}

		public static Item AlternativeReadItem(this Packet packet, bool readNetworkId)
		{
			Item stack;
			int  networkId = -1;
			if (readNetworkId)
			{
				networkId = packet.ReadSignedVarInt();
				
				if (networkId == 0)
				{
					return new ItemAir();
				}
			}

			return packet.ReadItem();
			
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
			stack = ItemFactory.GetItem((short) id, metadata, count);
			stack.UniqueId = networkId;
			
			ushort dataMarker = packet.ReadUshort(); // NbtLen

			if (dataMarker == 0xffff)
			{
				var version = packet.ReadByte();

				switch (version)
				{
					case 1:
						var nbt = packet.ReadNbt();

						if (nbt != null)
						{
							stack.ExtraData = (NbtCompound) nbt.NbtFile.RootTag;
						}

						break;
				}
			}
			else if (dataMarker > 0)
			{
				var nbtData = packet.ReadBytes(dataMarker);
			//	var stream = (Stream)ReflectionHelper.GetPrivateFieldValue<MemoryStreamReader>(typeof(Packet), packet, "_reader");
				using (MemoryStream ms = new MemoryStream(nbtData))
				{
					stack.ExtraData = ReadNbtCompound(ms);
					/*var nbt = ReadNbt(ms);

					if (nbt != null)
					{
						stack.ExtraData = (NbtCompound) nbt.NbtFile.RootTag;
					}*/
					
					//stack.ExtraData = ReadNbt(ms);
				}
			}

			var canPlace = packet.ReadVarInt();
			for (int i = 0; i < canPlace; ++i)
			{
				packet.ReadString();
			}
			
			var canBreak = packet.ReadVarInt();
			for (int i = 0; i < canBreak; ++i)
			{
				packet.ReadString();
			}

			if (id == 513) // shield
			{
				packet.ReadVarLong(); // something about tick, crap code
			}

			return stack;
		}

		public static NbtCompound ReadNbtCompound(Stream stream)
		{
			NbtFile file = new NbtFile();
			file.BigEndian = false;
			file.UseVarInt = false;

			file.LoadFromStream(stream, NbtCompression.None);

			return (NbtCompound) file.RootTag;
		}
		
		public static void AlternativeWriteItem(this Packet packet, Item item)
		{
			if (item.UniqueId != -1)
			{
				packet.WriteSignedVarInt(item.UniqueId);
			}
			
			packet.Write(item);
		}
	}
}