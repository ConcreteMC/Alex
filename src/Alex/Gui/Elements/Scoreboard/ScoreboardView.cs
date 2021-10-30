using System;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements.Scoreboard
{
	public class ScoreboardView : StackContainer
	{
		//private ConcurrentDictionary<string, EntryData> Rows { get; set; } = new ConcurrentDictionary<string, EntryData>();
		private ConcurrentDictionary<string, ScoreboardObjective> Objectives { get; set; } = new ConcurrentDictionary<string, ScoreboardObjective>(StringComparer.OrdinalIgnoreCase);

		private ConcurrentDictionary<string, string> EntityObjectives { get; set; } =
			new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public ScoreboardView() : base()
		{
			ChildAnchor = Alignment.Fill;
		}

		public void RemoveObjective(string name)
		{
			if (Objectives.TryRemove(name, out var objective))
			{
				objective.OnEntryAdded -= OnEntryAdded;
				objective.OnEntryRemoved -= OnEntryRemoved;
				RemoveChild(objective);
			}
		}
		
		public void AddObjective(ScoreboardObjective objective)
		{
			if (Objectives.TryAdd(objective.Name, objective))
			{
				objective.OnEntryAdded += OnEntryAdded;
				objective.OnEntryRemoved += OnEntryRemoved;
				AddChild(objective);
			}
		}

		private void OnEntryRemoved(object sender, string id)
		{
			if (sender is ScoreboardObjective objective)
			{
				EntityObjectives.TryRemove(id, out _);
			}
		}

		private void OnEntryAdded(object sender, string id)
		{
			if (sender is ScoreboardObjective objective)
			{
				EntityObjectives.AddOrUpdate(id, s => objective.Name, (s, s1) => objective.Name);
			}
		}

		public bool TryGetEntityScoreboard(string id, out ScoreboardObjective objective)
		{
			if (EntityObjectives.TryGetValue(id, out string obj) && Objectives.TryGetValue(obj, out objective))
			{
				return true;
			}

			objective = null;
			return false;
		}
		
		public bool TryGetObjective(string name, out ScoreboardObjective objective)
		{
			return Objectives.TryGetValue(name, out objective);
		}

		public void Clear()
		{
			//Rows.Clear();
			Objectives.Clear();
			foreach (var child in ChildElements)
			{
				RemoveChild(child);
			}
		}

		private double _elapsed = 0d;
		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			_elapsed += Alex.DeltaTimeSpan.TotalMilliseconds;

			if (_elapsed >= (1000d / 5d))
			{
				_elapsed = 0d;
				Tick();
			}
		}

		public void Tick()
		{
			foreach (var objective in Objectives.Values)
			{
				if (objective.HasChanges)
					objective.Rebuild();
			}
		}
	}
}
