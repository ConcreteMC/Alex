using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class ScoreboardObjectivePacket : Packet<ScoreboardObjectivePacket>
	{
		public string ObjectiveName { get; set; }
		public ObjectiveMode Mode { get; set; }
		public string Value { get; set; }
		public ObjectiveType Type { get; set; }


		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			ObjectiveName = stream.ReadString();
			Mode = (ObjectiveMode)stream.ReadByte();

			if (Mode == ObjectiveMode.Create || Mode == ObjectiveMode.UpdateText)
			{
				Value = stream.ReadChatObject();
				Type = (ObjectiveType)stream.ReadByte();
			}
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public enum ObjectiveMode
		{
			Create = 0,
			Remove = 1,
			UpdateText = 2
		}

		public enum ObjectiveType
		{
			Integer = 0,
			Hearts = 1
		}
	}
}