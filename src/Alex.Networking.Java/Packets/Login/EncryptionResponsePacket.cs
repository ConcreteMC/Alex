using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Login
{
	public class EncryptionResponsePacket : Packet<EncryptionResponsePacket>
	{
		public byte[] SharedSecret { get; set; }
		public byte[] VerifyToken { get; set; }

		public EncryptionResponsePacket()
		{
			PacketId = 0x01;
		}

		public override void Decode(MinecraftStream stream)
		{
			int sharedSecretLength = stream.ReadVarInt();
			SharedSecret = stream.Read(sharedSecretLength);
			int verifyTokenLength = stream.ReadVarInt();
			VerifyToken = stream.Read(verifyTokenLength);
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(SharedSecret.Length);
			stream.Write(SharedSecret);
			stream.WriteVarInt(VerifyToken.Length);
			stream.Write(VerifyToken);
		}
	}
}