using Alex.Networking.Java.Util;
using MiNET.Utils;

namespace Alex.Networking.Java.Packets.Play
{
	public class DisplayScoreboardPacket : Packet<DisplayScoreboardPacket>
	{
		public ScoreboardPosition Position { get; set; }
		public string Name { get; set; }
		
		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			Position = (ScoreboardPosition) stream.ReadByte();
			Name = stream.ReadString();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public enum ScoreboardPosition
		{
			List = 0,
			Sidebar = 1,
			BelowName = 2,
			TeamBlack = 3,
			TeamDarkBlue = 4,
			TeamDarkGreen = 5,
			TeamDarkAqua = 6,
			TeamDarkRed = 7,
			TeamDarkPurple = 8,
			TeamGold = 9,
			TeamGray = 10,
			TeamDarkGray = 11,
			TeamBlue = 12,
			TeamOther
		}
	}
}