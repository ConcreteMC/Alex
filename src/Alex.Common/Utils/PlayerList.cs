using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MiNET.Worlds;

namespace Alex.Common.Utils
{
	public class PlayerList : IEnumerable<PlayerListItem>
	{
		public ConcurrentDictionary<Guid, PlayerListItem> Entries { get; }

		public PlayerList()
		{
			Entries = new ConcurrentDictionary<Guid, PlayerListItem>();
		}

		/// <inheritdoc />
		public IEnumerator<PlayerListItem> GetEnumerator()
		{
			foreach (var entry in Entries)
			{
				if (entry.Value?.Username != null)
					yield return entry.Value;
			}
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class PlayerListItem
	{
		public Guid UUID { get; set; }
		public string Username { get; set; }
		public GameMode Gamemode { get; set; } = GameMode.Survival;
		public int Ping { get; set; } = 0;

		public PlayerListItem() { }

		public PlayerListItem(Guid id, string username, GameMode gamemode, int ping)
		{
			UUID = id;
			Username = username;
			Gamemode = gamemode;

			Ping = ping;
		}
	}
}