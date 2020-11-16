using System.Collections.Concurrent;
using Alex.Networking.Java.Packets.Play;

namespace Alex.Utils
{
	public class TeamsManager
	{
		private ConcurrentDictionary<string, Team> _teams = new ConcurrentDictionary<string, Team>();
		
		public TeamsManager()
		{
			
		}

		public void AddOrUpdateTeam(string name, Team team)
		{
			if (!_teams.TryAdd(name, team))
			{
				_teams[name] = team;
			}
		}

		public void RemoveTeam(string name)
		{
			_teams.TryRemove(name, out _);
		}

		public bool TryGet(string name, out Team team)
		{
			return _teams.TryGetValue(name, out team);
		}
	}

	public class Team
	{
		public string                Name        { get; set; }
		public string                DisplayName { get; set; }
		public TeamsPacket.TeamColor Color       { get; set; }
		public string                TeamPrefix  { get; set; }
		public string                TeamSuffix  { get; set; }

		public Team(string name, string displayName, TeamsPacket.TeamColor color, string prefix, string suffix)
		{
			Name = name;
			DisplayName = displayName;
			Color = color;
			TeamPrefix = prefix;
			TeamSuffix = suffix;
		}
	}
}