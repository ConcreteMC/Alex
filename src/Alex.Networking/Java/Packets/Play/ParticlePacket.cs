using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class ParticlePacket : Packet<ParticlePacket>
	{
		public int ParticleId;
		public bool LongDistance;
		public double X;
		public double Y;
		public double Z;
		public float OffsetX;
		public float OffsetY;
		public float OffsetZ;
		public float ParticleData;
		public int ParticleCount;

		public override void Decode(MinecraftStream stream)
		{
			ParticleId = stream.ReadInt();
			LongDistance = stream.ReadBool();
			X = stream.ReadDouble();
			Y = stream.ReadDouble();
			Z = stream.ReadDouble();
			OffsetX = stream.ReadFloat();
			OffsetY = stream.ReadFloat();
			OffsetZ = stream.ReadFloat();
			ParticleData = stream.ReadFloat();
			ParticleCount = stream.ReadInt();
			//TODO: Read data, varies per particle tho...
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}
	}
}
