using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Login
{
	public class EncryptionRequestPacket : Packet<EncryptionRequestPacket>
	{
		public string ServerId { get; set; }
		public byte[] PublicKey { get; set; }
		public byte[] VerifyToken { get; set; }

		public EncryptionRequestPacket()
		{
			PacketId = 0x02;
		}

		public override void Decode(MinecraftStream stream)
		{
			ServerId = stream.ReadString();
			int publicKeyLength = stream.ReadVarInt();
			PublicKey = stream.Read(publicKeyLength);
			int verifyTokenLength = stream.ReadVarInt();
			VerifyToken = stream.Read(verifyTokenLength);
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteString(ServerId);
			stream.WriteVarInt(PublicKey.Length);
			stream.Write(PublicKey);
			stream.WriteVarInt(VerifyToken.Length);
			stream.Write(VerifyToken);
		}
	}
}