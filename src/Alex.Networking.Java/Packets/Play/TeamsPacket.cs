using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class TeamsPacket : Packet<TeamsPacket>
	{
		public string TeamName { get; set; }
		public Mode PacketMode { get; set; }
		public TeamsMode Payload { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			TeamName = stream.ReadString();
			PacketMode = (Mode)stream.ReadByte();

			switch (PacketMode)
			{
				case Mode.CreateTeam:
					var a = new CreateTeam();
					a.Read(stream);
					Payload = a;

					break;

				case Mode.RemoveTeam:
					break;

				case Mode.UpdateTeam:
					var upd = new UpdateTeam();
					upd.Read(stream);
					Payload = upd;

					break;

				case Mode.AddPlayer:
					var add = new AddPlayers();
					add.Read(stream);
					Payload = add;

					break;

				case Mode.RemovePlayer:
					var remove = new RemovePlayers();
					remove.Read(stream);
					Payload = remove;

					break;
			}
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public enum Mode
		{
			CreateTeam = 0,
			RemoveTeam = 1,
			UpdateTeam = 2,
			AddPlayer = 3,
			RemovePlayer = 4
		}

		public class TeamsMode { }

		public class TeamInfo : TeamsMode
		{
			public string TeamDisplayName { get; set; }
			public byte Flags { get; set; }
			public string NameTagVisibility { get; set; }
			public string CollisionRule { get; set; }
			public TeamColor TeamColor { get; set; }
			public string TeamPrefix { get; set; }
			public string TeamSuffix { get; set; }

			public virtual void Read(MinecraftStream stream)
			{
				TeamDisplayName = stream.ReadChatObject();
				Flags = (byte)stream.ReadByte();
				NameTagVisibility = stream.ReadString();
				CollisionRule = stream.ReadString();
				TeamColor = (TeamsPacket.TeamColor)stream.ReadVarInt();
				TeamPrefix = stream.ReadChatObject();
				TeamSuffix = stream.ReadChatObject();
			}
		}

		public class UpdateTeam : TeamInfo { }

		public class CreateTeam : TeamInfo
		{
			public string[] Entities { get; set; }

			public override void Read(MinecraftStream stream)
			{
				base.Read(stream);

				int count = stream.ReadVarInt();
				Entities = new string[count];

				for (int i = 0; i < count; i++)
				{
					Entities[i] = stream.ReadString();
				}
			}
		}

		public class PlayersData : TeamsMode
		{
			public string[] Entities { get; set; }

			public void Read(MinecraftStream stream)
			{
				int count = stream.ReadVarInt();
				Entities = new string[count];

				for (int i = 0; i < count; i++)
				{
					Entities[i] = stream.ReadString();
				}
			}
		}

		public class AddPlayers : PlayersData { }

		public class RemovePlayers : PlayersData { }

		public enum TeamColor { }
	}
}