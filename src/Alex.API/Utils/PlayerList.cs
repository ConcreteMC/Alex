using System.Collections.Generic;
using MiNET.Worlds;

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
		public GameMode         Gamemode { get; set; } = GameMode.Survival;
		public int              Ping     { get; set; } = 0;

		public bool IsJavaPlayer { get; set; } = false;
		public PlayerListItem()
		{

		}

		public PlayerListItem(MiNET.Utils.UUID id, string username, GameMode gamemode, int ping, bool isJavaPlayer)
		{
			UUID = id;
			Username = username;
			Gamemode = gamemode;

			Ping = ping;
			IsJavaPlayer = isJavaPlayer;
		}
	}
}
