using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class SetTitleSubTextPacket : Packet<SetTitleSubTextPacket>
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
			stream.WriteString(Text);
		}
	}
}