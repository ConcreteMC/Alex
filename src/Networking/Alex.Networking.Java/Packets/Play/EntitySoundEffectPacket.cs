using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntitySoundEffectPacket : Packet<EntitySoundEffectPacket>
	{
		public int SoundId { get; set; }
		public SoundEffectPacket.SoundCategory Category { get; set; }
		public int EntityId { get; set; }
		public float Volume { get; set; }
		public float Pitch { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			SoundId = stream.ReadVarInt();
			Category = (SoundEffectPacket.SoundCategory)stream.ReadVarInt();
			EntityId = stream.ReadVarInt();

			Volume = stream.ReadFloat();
			Pitch = stream.ReadFloat();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}