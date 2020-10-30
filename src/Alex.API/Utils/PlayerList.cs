using System.Collections.Generic;

namespace Alex.API.Utils
{
	public class PlayerList
	{
		public Dictionary<MiNET.Utils.UUID, PlayerListItem> Entries { get; }

		public PlayerList()
		{
			Entries = new Dictionary<MiNET.Utils.UUID, PlayerListItem>();
		}
	}

	public class PlayerListItem
	{
		public MiNET.Utils.UUID UUID     { get; set; }
		public string           Username { get; set; }
		public Gamemode         Gamemode { get; set; } = Gamemode.Survival;
		public int              Ping     { get; set; } = 0;

		public PlayerListItem()
		{

		}

		public PlayerListItem(MiNET.Utils.UUID id, string username, Gamemode gamemode, int ping)
		{
			UUID = id;
			Username = username;
			Gamemode = gamemode;

			Ping = ping;
		}
	}
}
