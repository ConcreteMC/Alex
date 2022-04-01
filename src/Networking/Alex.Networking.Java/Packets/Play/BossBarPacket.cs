using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class BossBarPacket : Packet<BossBarPacket>
	{
		public Guid Uuid;
		public BossBarAction Action;

		public string Title;
		public float Health;
		public BossBarColor Color;
		public BossBarDivisions Divisions;
		public byte Flags;

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			Uuid = stream.ReadUuid();
			Action = (BossBarAction)stream.ReadVarInt();

			switch (Action)
			{
				case BossBarAction.Add:
					Title = stream.ReadChatObject();
					Health = stream.ReadFloat();
					Color = (BossBarColor)stream.ReadVarInt();
					Divisions = (BossBarDivisions)stream.ReadVarInt();
					Flags = (byte)stream.ReadByte();

					break;

				case BossBarAction.Remove:
					break;

				case BossBarAction.UpdateHealth:
					Health = stream.ReadFloat();

					break;

				case BossBarAction.UpdateTitle:
					Title = stream.ReadChatObject();

					break;

				case BossBarAction.UpdateStyle:
					Color = (BossBarColor)stream.ReadVarInt();
					Divisions = (BossBarDivisions)stream.ReadVarInt();

					break;

				case BossBarAction.UpdateFlags:
					Flags = (byte)stream.ReadByte();

					break;
			}
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public enum BossBarAction
		{
			Add = 0,
			Remove = 1,
			UpdateHealth = 2,
			UpdateTitle = 3,
			UpdateStyle = 4,
			UpdateFlags = 5
		}

		public enum BossBarColor
		{
			Pink = 0,
			Blue = 1,
			Red = 2,
			Green = 3,
			Yellow = 4,
			Purple = 5,
			White = 6
		}

		public enum BossBarDivisions
		{
			None = 0,
			Six = 1,
			Ten = 2,
			Twelve = 3,
			Twenty = 4
		}
	}
}