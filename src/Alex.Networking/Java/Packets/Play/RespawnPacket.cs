using System;
using Alex.API.Utils;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class RespawnPacket : Packet<RespawnPacket>
	{
		public int Dimension;
		public byte Difficulty;
		public Gamemode Gamemode;
		public string LevelType;

		public override void Decode(MinecraftStream stream)
		{
			Dimension = stream.ReadInt();
			Difficulty = (byte) stream.ReadByte();
			Gamemode = (Gamemode) stream.ReadByte();
			LevelType = stream.ReadString();
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}
	}
}
