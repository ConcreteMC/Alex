using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class UpdateViewDistancePacket : Packet<UpdateViewDistancePacket>
	{
		public int ViewDistance { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			ViewDistance = stream.ReadVarInt();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}