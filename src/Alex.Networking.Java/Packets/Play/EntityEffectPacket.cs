using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityEffectPacket : Packet<EntityEffectPacket>
	{
		public int EntityId { get; set; }
		public byte EffectId { get; set; }
		public byte Amplifier { get; set; }
		public int Duration { get; set; }
		public byte Flags { get; set; }

		public EntityEffectPacket() { }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			EffectId = (byte)stream.ReadByte();
			Amplifier = (byte)stream.ReadByte();
			Duration = stream.ReadVarInt();
			Flags = (byte)stream.ReadByte();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
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