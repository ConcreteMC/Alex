using System;
using System.Buffers.Binary;
using System.IO;
using Alex.Items;
using fNbt;
using MiNET.Net;
using MiNET.UI;
using MiNET.Utils;

namespace Alex.Net.Bedrock.Packets
{
	public class CreativeContent : McpeCreativeContent
	{
		/// <inheritdoc />
		protected override void DecodePacket()
		{
			this.Id = (byte) this.ReadVarInt();

			this.input = ReadItemStacks2();
		}
		
		public ItemStacks ReadItemStacks2()
		{
			var metadata = new ItemStacks();

			var count = ReadUnsignedVarInt();

			for (int i = 0; i < count; i++)
			{
				//int uniqueId = 1;
				//if (!(this is McpeCraftingEvent))
				//{
					//uniqueId = ReadVarInt();
				//}
				var entryId = ReadVarInt();
				var item    = ReadItem2(this);
				item.UniqueId = entryId;
				metadata.Add(item);
			}

			return metadata;
		}
		
		public static MiNET.Items.Item ReadItem2(Packet packet)
		{
			int id = packet.ReadVarInt();
			if (id == 0)
			{
				return new MiNET.Items.ItemAir();
			}

			int   aux      = packet.ReadVarInt();
			short metadata = (short) (aux >> 8);
			byte  count    = (byte) (aux & 0xff);
			
			//if (metadata == short.MaxValue) metadata = -1;
			
			var stack = MiNET.Items.ItemFactory.GetItem((short) id, metadata, count);
			
			var nbtLen = packet.ReadShort(); // NbtLen
			if (nbtLen == -1)
			{
				var version = packet.ReadByte();

				if (version == 1)
				{
					stack.ExtraData = (NbtCompound) packet.ReadNbt().NbtFile.RootTag;
				}
				else
				{
					throw new Exception($"Unknown NBT version: {version}");
				}
			}
			else if (nbtLen > 0)
			{
				var nbtData = packet.ReadBytes(nbtLen);

				
					using (MemoryStream ms = new MemoryStream(nbtData))
					{
						stack.ExtraData = (NbtCompound) ReadLegacyNbtCompound(ms);
					}
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

			if (id == MiNET.Items.ItemFactory.GetItemIdByName("minecraft:shield")) // shield
			{
				packet.ReadSignedVarLong(); // something about tick, crap code
			}

			return stack;
		}
	}
}