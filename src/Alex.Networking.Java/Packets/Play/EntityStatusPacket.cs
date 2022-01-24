using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityStatusPacket : Packet<EntityStatusPacket>
	{
		public EntityStatusPacket()
		{
			PacketId = 0x1C;
		}

		public int EntityId;
		public byte EntityStatus;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadInt();
			EntityStatus = (byte)stream.ReadByte();
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}
	}
}