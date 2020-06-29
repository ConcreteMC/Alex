using Alex.Networking.Java.Util;
using Microsoft.Xna.Framework;

namespace Alex.Networking.Java.Packets.Play
{
	public class SpawnPositionPacket : Packet<SpawnPositionPacket>
	{
		public Vector3 SpawnPosition { get; set; }
		
		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			SpawnPosition = stream.ReadPosition();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}