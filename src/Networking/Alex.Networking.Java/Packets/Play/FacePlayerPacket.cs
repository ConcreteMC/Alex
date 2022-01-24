using Alex.Networking.Java.Util;
using Microsoft.Xna.Framework;

namespace Alex.Networking.Java.Packets.Play
{
	public class FacePlayerPacket : Packet<FacePlayerPacket>
	{
		public Vector3 Target { get; set; } = Vector3.Zero;
		public bool LookAtEyes { get; set; } = false;
		public bool IsEntity { get; set; } = false;
		public int EntityId { get; set; } = 0;
		public bool AimWithHead { get; set; } = false;

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			AimWithHead = stream.ReadVarInt() == 1;
			var x = (float)stream.ReadDouble();
			var y = (float)stream.ReadDouble();
			var z = (float)stream.ReadDouble();
			Target = new Vector3(x, y, z);
			IsEntity = stream.ReadBool();

			if (IsEntity)
			{
				EntityId = stream.ReadVarInt();
				LookAtEyes = stream.ReadVarInt() == 1;
			}
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}