using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gui.Elements.Scoreboard
{
	public class ScoreboardObjective : StackContainer
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ScoreboardObjective));
		private ConcurrentDictionary<string, ScoreboardEntry> Entries      { get; }
		private List<ScoreboardEntry> _removed { get; } = new List<ScoreboardEntry>();
		
		public  string                                      Name         { get; set; }

		public string DisplayName
		{
			get
			{
				return _displayNameElement.Text;
			}
			set
			{
				_displayNameElement.Text = value;
			}
		}

		public  int                                         SortOrder    { get; set; }
		public  string                                      CriteriaName { get; set; }

		public EventHandler<string> OnEntryAdded;
		public EventHandler<string> OnEntryRemoved;

		private int _changes = 0;
		public bool HasChanges => _changes > 0;
		public ScoreboardObjective(string name, string displayName, int sortOrder, string criteriaName)
		{
			_displayNameElement = new TextElement(displayName) {Anchor = Alignment.CenterX};
			
			Entries = new ConcurrentDictionary<string, ScoreboardEntry>(StringComparer.InvariantCulture);
			Name = name;
			DisplayName = displayName;
			SortOrder = sortOrder;
			CriteriaName = criteriaName;
			ChildAnchor = Alignment.Fill;
			
			var container = new Container();
			container.AddChild(_displayNameElement);
			
			AddChild(container);

			_spacer = new StackMenuSpacer();

			Margin = new Thickness(10);
			
			BackgroundOverlay = new Color(Color.Black, 0.5f);
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
		}

		public void AddOrUpdate(string id, ScoreboardEntry entry)
		{
			if (Entries.TryGetValue(id, out var value))
			{
				if (value.Score != entry.Score)
				{
					value.Score = entry.Score;
					
					Interlocked.Increment(ref _changes);
				}

				value.DisplayName = entry.DisplayName;
			}
			else
			{
				var firstRemoved = _removed.FirstOrDefault();
				if (firstRemoved != null)
				{
					_removed.Remove(firstRemoved);
					firstRemoved.Score = entry.Score;
					firstRemoved.DisplayName = entry.DisplayName;
					firstRemoved.EntryId = id;
					entry = firstRemoved;
				}
				
				Entries.TryAdd(id, entry);
				OnEntryAdded?.Invoke(this, id);

				Interlocked.Increment(ref _changes);
			}
		}

		public void Remove(string id)
		{
			if (Entries.TryRemove(id, out var old))
			{
				_removed.Add(old);
				
				OnEntryRemoved?.Invoke(this, id);

				Interlocked.Increment(ref _changes);
			}
			else
			{
				Log.Warn($"Could not find entry with id: {id}");
			}
		}
		
		public bool TryGet(string id, out ScoreboardEntry entry)
		{
			return Entries.TryGetValue(id, out entry);
		}

		private readonly TextElement _displayNameElement;
		private readonly StackMenuSpacer _spacer;
		internal void Rebuild()
		{
			if (Interlocked.Exchange(ref _changes, 0) <= 0)
				return;
			
			var                 entries = Entries.ToArray();
			
			long previousScore = 0;
			if (SortOrder == 0) //Ascending
			{
				entries = entries.OrderBy(x => x.Value.Score).ToArray();
			}
			else if (SortOrder == 1)//Descending
			{
				entries = entries.OrderByDescending(x => x.Value.Score).ToArray();
			}

			previousScore = entries[0].Value.Score;
			
			RemoveChild(_spacer);

			foreach (var child in ChildElements)
			{
				if (child is not ScoreboardEntry sbe)
					continue;
				
				RemoveChild(sbe);

				if (_removed.Contains(sbe))
					_removed.Remove(sbe);
			}

			bool showScores = false;
			long maxDifference = 0;

			for (var index = 0; index < entries.Length; index++)
			{
				var entry = entries[index];
				var difference = (long)Math.Abs(entry.Value.Score - previousScore);

				maxDifference = Math.Max(difference, maxDifference);

				previousScore = entry.Value.Score;

				AddChild(entry.Value);
			}

			if (maxDifference > 1 && CriteriaName != "dummy")
			{
				showScores = true;
				//Log.Info($"Max difference: {maxDifference}");
			}
			
			foreach (var element in entries)
			{
				element.Value.ShowScore = showScores;
			}
			
			AddChild(_spacer);
		}
	}
}