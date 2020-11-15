using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class PlayerMovementPacket : Packet
	{
		public bool OnGround { get; set; }
		public PlayerMovementPacket()
		{
			PacketId = 0x15;
		}
		
		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			stream.WriteBool(OnGround);
		}
	}
}