using System;
using System.Collections.Concurrent;
using System.Linq;
using NLog;
using RocketUI;

namespace Alex.Gui.Elements.Scoreboard
{
	public class ScoreboardObjective : StackContainer
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ScoreboardObjective));
		private ConcurrentDictionary<string, ScoreboardEntry> Entries      { get; }
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
		public ScoreboardObjective(string name, string displayName, int sortOrder, string criteriaName)
		{
			_displayNameElement = new TextElement(displayName) {Anchor = Alignment.CenterX};
			
			Entries = new ConcurrentDictionary<string, ScoreboardEntry>(StringComparer.InvariantCulture);
			Name = name;
			DisplayName = displayName;
			SortOrder = sortOrder;
			CriteriaName = criteriaName;
			ChildAnchor = Alignment.Fill;
			
			_container = new Container();
			_container.AddChild(_displayNameElement);
			
			AddChild(_container);

			_spacer = new StackMenuSpacer();
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
		}

		public void AddOrUpdate(string id, ScoreboardEntry entry)
		{
			bool rebuild = false;
			if (Entries.TryGetValue(id, out var value))
			{
				if (value.Score != entry.Score)
				{
					value.Score = entry.Score;
					rebuild = true;
				}

				value.DisplayName = entry.DisplayName;
			}
			else
			{
				if (Entries.TryAdd(id, entry))
				{
					OnEntryAdded?.Invoke(this, id);
				}
				rebuild = true;
			}

			if (rebuild)
				Rebuild();
			/*Entries.AddOrUpdate(id, entry, (oldId, oldValue) => entry);
			
			Rebuild();*/
		}

		public void Remove(string id)
		{
			if (Entries.TryRemove(id, out var old))
			{
				OnEntryRemoved?.Invoke(this, id);

				RemoveChild(old);
				Rebuild();
			}
			else
			{
				Log.Warn($"Could not find entry with id: {id}");
			}
		}

		public bool TryGetByScore(uint score, out ScoreboardEntry entry)
		{
			foreach (var e in Entries.Values.ToArray())
			{
				if (e.Score == score)
				{
					entry = e;

					return true;
				}
			}

			entry = null;
			return false;
		}
		
		public bool TryGet(string id, out ScoreboardEntry entry)
		{
			return Entries.TryGetValue(id, out entry);
		}

		private TextElement _displayNameElement;
		private Container   _container;
		private StackMenuSpacer _spacer;
		internal void Rebuild()
		{
			var                 entries = Entries.ToArray();

			if (SortOrder == 0) //Ascending
			{
				entries = entries.OrderBy(x => x.Value.Score).ToArray();
			}
			else if (SortOrder == 1)//Descending
			{
				entries = entries.OrderByDescending(x => x.Value.Score).ToArray();
			}
			
			//ClearChildren();

			//Container container = new Container();
			//container.BackgroundOverlay = new Color(Color.Black, );
			//_container.AddChild(_displayNameElement);
			//AddChild(_container);
			
			RemoveChild(_spacer);
			
			foreach (var entry in entries)
			{
				if (CriteriaName == "dummy")
					entry.Value.ShowScore = false;
				
				RemoveChild(entry.Value);
				AddChild(entry.Value);
			}
			
			AddChild(_spacer);
		}
	}
}