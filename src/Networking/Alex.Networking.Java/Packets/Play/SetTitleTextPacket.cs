using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class SetTitleTextPacket : Packet<SetTitleTextPacket>
	{
		public string Text { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			Text = stream.ReadChatObject();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}