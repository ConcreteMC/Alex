using Alex.Interfaces;
using Alex.Networking.Java.Models;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class SoundEffectPacket : Packet<SoundEffectPacket>
	{
		public int SoundId { get; set; }
		public SoundCategory Category { get; set; }
		public IVector3 Position { get; set; }
		public float Volume { get; set; }
		public float Pitch { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			SoundId = stream.ReadVarInt();
			Category = (SoundCategory)stream.ReadVarInt();
			var x = stream.ReadInt();
			var y = stream.ReadInt();
			var z = stream.ReadInt();

			Position = new NetworkVector3(x / 8f, y / 8f, z / 8f);

			Volume = stream.ReadFloat();
			Pitch = stream.ReadFloat();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public enum SoundCategory
		{
			Master = 0,
			Music = 1,
			Record = 2,
			Weather = 3,
			Block = 4,
			Hostile = 5,
			Neutral = 6,
			Player = 7,
			Ambient = 8,
			Voice = 9
		}
	}
}