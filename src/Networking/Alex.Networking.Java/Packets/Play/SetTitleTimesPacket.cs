using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class SetTitleTimesPacket : Packet<SetTitleTimesPacket>
	{
		public enum ActionEnum
		{
			SetTitle = 0,
			SetSubTitle = 1,
			SetActionBar = 2,
			SetTimesAndDisplay = 3,
			Hide = 4,
			Reset = 5
		}

		//public ActionEnum Action;
		//	public string     TitleText;
		//	public string     SubtitleText;
		//	public string     ActionBarText;
		public int FadeIn, Stay, FadeOut;

		public override void Decode(MinecraftStream stream)
		{
			FadeIn = stream.ReadInt();
			Stay = stream.ReadInt();
			FadeOut = stream.ReadInt();

			/*Action = (ActionEnum) stream.ReadVarInt();
			switch (Action)
			{
				case ActionEnum.SetTitle:
					TitleText = stream.ReadChatObject();
					break;
				case ActionEnum.SetSubTitle:
					SubtitleText = stream.ReadChatObject();
					break;
				case ActionEnum.SetActionBar:
					ActionBarText = stream.ReadChatObject();
					break;
				case ActionEnum.SetTimesAndDisplay:
					FadeIn = stream.ReadInt();
					Stay = stream.ReadInt();
					FadeOut = stream.ReadInt();
					break;
				case ActionEnum.Hide:

					break;
				case ActionEnum.Reset:

					break;
			}*/
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}
	}
}