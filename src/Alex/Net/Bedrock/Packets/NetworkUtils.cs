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

			nbt.NbtFile = nbtFile;
			
			return nbt;
		}
	}
}