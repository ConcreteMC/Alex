using System;
using System.Collections.Generic;
using Alex.Gui.Elements.Scoreboard;
using Alex.Networking.Java.Packets.Play;

namespace Alex.Utils
{
	public class Team
	{
		private string _teamPrefix;
		private string _teamSuffix;
		private string _displayName;
		public string                Name        { get; set; }

		public string DisplayName
		{
			get => _displayName;
			set
			{
				_displayName = value;
				UpdateScoreboard();
			}
		}

		public TeamsPacket.TeamColor Color       { get; set; }

		public string TeamPrefix
		{
			get => _teamPrefix;
			set
			{
				_teamPrefix = value;
				UpdateScoreboard();
			}
		}

		public string TeamSuffix
		{
			get => _teamSuffix;
			set
			{
				_teamSuffix = value;
				UpdateScoreboard();
			}
		}

		private void UpdateScoreboard()
		{
			var scoreboardEntry = ScoreboardEntry;

			if (scoreboardEntry != null && scoreboardEntry.TryGetTarget(out var target))
			{
				target.DisplayName = $"{_teamPrefix}{Name}{_teamSuffix}";
			}
		}
		
		public WeakReference<ScoreboardEntry> ScoreboardEntry { get; set; } = null;
		public List<string> Entities { get; } = new List<string>();
		
		public Team(string name, string displayName, TeamsPacket.TeamColor color, string prefix, string suffix)
		{
			Name = name;
			DisplayName = displayName;
			Color = color;
			TeamPrefix = prefix;
			TeamSuffix = suffix;
		}

		public void AddEntity(string identifier)
		{
			if (!Entities.Contains(identifier))
			{
				Entities.Add(identifier);
			}
		}

		public void RemoveEntity(string identifier)
		{
			if (Entities.Contains(identifier))
			{
				Entities.Remove(identifier);
			}
		}
	}
}