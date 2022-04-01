using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class PlayerListItemPacket : Packet<PlayerListItemPacket>
	{
		public PlayerListAction Action;

		public AddPlayerEntry[] AddPlayerEntries = new AddPlayerEntry[0];
		public RemovePlayerEntry[] RemovePlayerEntries = new RemovePlayerEntry[0];
		public UpdateDisplayNameEntry[] UpdateDisplayNameEntries = new UpdateDisplayNameEntry[0];
		public UpdateLatencyEntry[] UpdateLatencyEntries = new UpdateLatencyEntry[0];

		public PlayerListItemPacket()
		{
			PacketId = 0x2F;
		}

		public override void Decode(MinecraftStream stream)
		{
			Action = (PlayerListAction)stream.ReadVarInt();
			int count = stream.ReadVarInt();

			if (Action == PlayerListAction.AddPlayer)
			{
				ReadAddPlayerEntries(count, stream);

				return;
			}

			if (Action == PlayerListAction.UpdateLatency)
			{
				ReadUpdateLatencyEntries(count, stream);

				return;
			}

			if (Action == PlayerListAction.RemovePlayer)
			{
				RemovePlayerEntries = new RemovePlayerEntry[count];

				for (int i = 0; i < RemovePlayerEntries.Length; i++)
				{
					var entry = new RemovePlayerEntry();
					entry.UUID = stream.ReadUuid();
					RemovePlayerEntries[i] = entry;
				}
			}

			if (Action == PlayerListAction.UpdateDisplayName)
			{
				ReadUpdateDisplayNameEntries(count, stream);
			}
		}

		private void ReadUpdateLatencyEntries(int count, MinecraftStream stream)
		{
			UpdateLatencyEntries = new UpdateLatencyEntry[count];

			for (int i = 0; i < count; i++)
			{
				var entry = new UpdateLatencyEntry();
				entry.UUID = stream.ReadUuid();
				entry.Ping = stream.ReadVarInt();

				UpdateLatencyEntries[i] = entry;
			}
		}

		private void ReadUpdateDisplayNameEntries(int count, MinecraftStream stream)
		{
			UpdateDisplayNameEntries = new UpdateDisplayNameEntry[count];

			for (int i = 0; i < count; i++)
			{
				var entry = new UpdateDisplayNameEntry();
				entry.UUID = stream.ReadUuid();
				entry.HasDisplayName = stream.ReadBool();

				if (entry.HasDisplayName)
				{
					entry.DisplayName = stream.ReadString();
				}

				UpdateDisplayNameEntries[i] = entry;
			}
		}

		private void ReadAddPlayerEntries(int count, MinecraftStream stream)
		{
			AddPlayerEntries = new AddPlayerEntry[count];

			for (int i = 0; i < count; i++)
			{
				var newEntry = new AddPlayerEntry();
				newEntry.UUID = stream.ReadUuid();
				newEntry.Name = stream.ReadString();
				int propertyLength = stream.ReadVarInt();
				newEntry.Properties = new PlayerListProperty[propertyLength];

				for (int ii = 0; ii < propertyLength; ii++)
				{
					newEntry.Properties[ii] = new PlayerListProperty()
					{
						Name = stream.ReadString(), Value = stream.ReadString(), IsSigned = stream.ReadBool()
					};

					if (newEntry.Properties[ii].IsSigned)
					{
						newEntry.Properties[ii].Signature = stream.ReadString();
					}
				}

				newEntry.Gamemode = stream.ReadVarInt();
				newEntry.Ping = stream.ReadVarInt();
				newEntry.HasDisplayName = stream.ReadBool();

				if (newEntry.HasDisplayName)
				{
					newEntry.DisplayName = stream.ReadString();
				}

				AddPlayerEntries[i] = newEntry;
			}
		}

		private void WritePlayerEntries(MinecraftStream stream)
		{
			stream.WriteVarInt(AddPlayerEntries.Length);

			foreach (var playerEntry in AddPlayerEntries)
			{
				stream.WriteUuid(playerEntry.UUID);
				stream.WriteString(playerEntry.Name);
				stream.WriteVarInt(playerEntry.Properties.Length);

				foreach (var property in playerEntry.Properties)
				{
					stream.WriteString(property.Name);
					stream.WriteString(property.Value);
					stream.WriteBool(property.IsSigned);

					if (property.IsSigned)
					{
						stream.WriteString(property.Signature);
					}
				}

				stream.WriteVarInt(playerEntry.Gamemode);
				stream.WriteVarInt(playerEntry.Ping);
				stream.WriteBool(playerEntry.HasDisplayName);

				if (playerEntry.HasDisplayName)
				{
					stream.WriteString(playerEntry.DisplayName);
				}
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt((int)Action);

			switch (Action)
			{
				case PlayerListAction.AddPlayer:
					WritePlayerEntries(stream);

					break;

				case PlayerListAction.UpdateGamemode:
					//stream.WriteVarInt(Gamemode);
					break;

				case PlayerListAction.UpdateLatency:
					//	stream.WriteVarInt(Ping);
					break;

				case PlayerListAction.UpdateDisplayName:
					//	bool hdn = !string.IsNullOrEmpty(Displayname);
					//	stream.WriteBool(hdn);
					//	if (hdn)
					//	{
					//		stream.WriteString(Displayname);
					//	}
					break;

				case PlayerListAction.RemovePlayer:
					stream.WriteVarInt(RemovePlayerEntries.Length);

					foreach (var remove in RemovePlayerEntries)
					{
						stream.WriteUuid(remove.UUID);
					}

					break;
			}
		}

		public class PlayerEntry
		{
			public Guid UUID;
		}

		public class UpdateDisplayNameEntry : PlayerEntry
		{
			public bool HasDisplayName;
			public string DisplayName;
		}

		public class AddPlayerEntry : PlayerEntry
		{
			public string Name;
			public PlayerListProperty[] Properties;
			public int Gamemode;
			public int Ping;
			public bool HasDisplayName;
			public string DisplayName;
		}

		public class UpdateLatencyEntry : PlayerEntry
		{
			public int Ping;
		}

		public class RemovePlayerEntry : PlayerEntry
		{
			//public Guid UUID;
		}
	}

	public enum PlayerListAction : int
	{
		AddPlayer = 0,
		UpdateGamemode = 1,
		UpdateLatency = 2,
		UpdateDisplayName = 3,
		RemovePlayer = 4
	}

	public sealed class PlayerListProperty
	{
		public string Name;
		public string Value;
		public bool IsSigned;
		public string Signature;
	}
}