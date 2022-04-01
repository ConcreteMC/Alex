using Alex.Interfaces;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class AcknowledgePlayerDiggingPacket : Packet<AcknowledgePlayerDiggingPacket>
	{
		public IVector3I Position { get; set; }
		public int Block { get; set; }
		public DigStatus Status { get; set; }
		public bool Successful { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			Position = stream.ReadBlockCoordinates();
			Block = stream.ReadVarInt();
			Status = (DigStatus)stream.ReadVarInt();
			Successful = stream.ReadBool();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public enum DigStatus
		{
			StartedDigging = 0,
			CancelledDigging = 1,
			FinishedDigging = 2
		}
	}
}