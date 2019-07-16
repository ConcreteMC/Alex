using System.Collections.Generic;

namespace Alex.API.Utils
{
	public class PlayerList
	{
		public Dictionary<UUID, PlayerListItem> Entries { get; }

		public PlayerList()
		{
			Entries = new Dictionary<UUID, PlayerListItem>();
		}
	}

	public class PlayerListItem
	{
		public UUID UUID { get; set; }
		public string Username { get; set; }
		public Gamemode Gamemode { get; set; } = Gamemode.Survival;
		public int Ping { get; set; } = 0;

		public PlayerListItem()
		{

		}

		public PlayerListItem(UUID id, string username, Gamemode gamemode, int ping)
		{
			UUID = id;
			Username = username;
			Gamemode = gamemode;

			Ping = ping;
		}
	}
}
