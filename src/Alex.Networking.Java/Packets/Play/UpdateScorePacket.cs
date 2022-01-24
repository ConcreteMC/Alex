using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class UpdateScorePacket : Packet<UpdateScorePacket>
	{
		public string EntityName { get; set; }
		public UpdateScoreAction Action { get; set; }
		public string ObjectiveName { get; set; }
		public int Value { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			EntityName = stream.ReadString();
			Action = (UpdateScoreAction)stream.ReadByte();
			ObjectiveName = stream.ReadString();

			if (Action == UpdateScoreAction.CreateOrUpdate)
				Value = stream.ReadVarInt();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public enum UpdateScoreAction
		{
			CreateOrUpdate = 0,
			Remove = 1
		}
	}
}