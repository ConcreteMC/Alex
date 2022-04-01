using System.Collections.Concurrent;
using System.Linq;

namespace Alex.Utils
{
	public class TeamsManager
	{
		private ConcurrentDictionary<string, Team> _teams = new ConcurrentDictionary<string, Team>();

		public TeamsManager() { }

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

		public bool TryGetEntityTeam(string identifier, out Team team)
		{
			var result = _teams.Values.FirstOrDefault(x => x.Entities.Contains(identifier));

			if (result != null)
			{
				team = result;

				return true;
			}

			team = null;

			return false;
		}
	}
}