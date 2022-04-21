using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityEffectPacket : Packet<EntityEffectPacket>
	{
		public int EntityId { get; set; }
		public int EffectId { get; set; }
		public byte Amplifier { get; set; }
		public int Duration { get; set; }
		public byte Flags { get; set; }

		public EntityEffectPacket() { }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			EffectId = stream.ReadVarInt();
			Amplifier = (byte)stream.ReadByte();
			Duration = stream.ReadVarInt();
			Flags = (byte)stream.ReadByte();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteVarInt(EffectId);
			stream.WriteByte(Amplifier);
			stream.WriteVarInt(Duration);
			stream.WriteByte(Flags);
		}

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			EntityId = 0;
			EffectId = 0;
			Amplifier = 0;
			Duration = 0;
			Flags = 0;
		}
	}
}