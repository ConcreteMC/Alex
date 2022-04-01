using System;
using Alex.Interfaces;
using Alex.Networking.Java.Util;
using fNbt;

namespace Alex.Networking.Java.Packets.Play
{
	public class RespawnPacket : Packet<RespawnPacket>
	{
		public NbtCompound Dimension;
		public byte Difficulty;
		public GameMode Gamemode, PreviousGamemode;
		public string WorldName;
		public long HashedSeed;
		public bool IsDebug, IsFlat, CopyMetadata;

		public override void Decode(MinecraftStream stream)
		{
			Dimension = stream.ReadNbtCompound();
			WorldName = stream.ReadString();
			HashedSeed = stream.ReadLong();
			Gamemode = (GameMode)stream.ReadByte();
			PreviousGamemode = (GameMode)stream.ReadByte();
			IsDebug = stream.ReadBool();
			IsFlat = stream.ReadBool();
			CopyMetadata = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}
	}
}