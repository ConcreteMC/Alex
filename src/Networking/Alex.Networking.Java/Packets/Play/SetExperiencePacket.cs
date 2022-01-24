using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class SetExperiencePacket : Packet<SetExperiencePacket>
	{
		public float ExperienceBar { get; set; }
		public int Level { get; set; }
		public int TotalExperience { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			ExperienceBar = stream.ReadFloat();
			Level = stream.ReadVarInt();
			TotalExperience = stream.ReadVarInt();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}