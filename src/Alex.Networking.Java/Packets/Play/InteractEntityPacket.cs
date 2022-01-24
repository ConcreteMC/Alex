using System.Numerics;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class InteractEntityPacket : Packet<InteractEntityPacket>
	{
		public InteractEntityPacket()
		{
			PacketId = 0x0D;
		}

		public int EntityId { get; set; }
		public int Type { get; set; }
		public Vector3 Target { get; set; }
		public int Hand { get; set; } = 0;
		public bool Sneaking { get; set; } = false;

		public override void Decode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteVarInt(Type);

			if (Type == 2)
			{
				stream.WriteFloat(Target.X);
				stream.WriteFloat(Target.Y);
				stream.WriteFloat(Target.Z);
			}

			if (Type == 2 || Type == 0)
			{
				stream.WriteVarInt(Hand);
			}

			stream.WriteBool(Sneaking);
		}
	}
}