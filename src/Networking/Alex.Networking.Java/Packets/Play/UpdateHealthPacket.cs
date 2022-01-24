using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class UpdateHealthPacket : Packet<UpdateHealthPacket>
	{
		public float Health { get; set; }
		public int Food { get; set; }
		public float Saturation { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			Health = stream.ReadFloat();
			Food = stream.ReadVarInt();
			Saturation = stream.ReadFloat();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream) { }
	}
}