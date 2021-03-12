using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Gui;
using Alex.API.Gui.Elements;

using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using RocketUI;
using NLog;

namespace Alex.Gui.Elements
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
			
			foreach (var entry in entries)
			{
				if (CriteriaName == "dummy")
					entry.Value.ShowScore = false;
				
				RemoveChild(entry.Value);
				AddChild(entry.Value);
			}
		}
	}

	public class ScoreboardEntry : Container
	{
		private string   _entryId;
		private uint   _score;
		private string _displayName;

		public string EntryId
		{
			get => _entryId;
			set => _entryId = value;
		}

		public uint Score
		{
			get => _score;
			set
			{
				var originalScore = _score;
				_score = value;
				RightText.Text = value.ToString();

				if (ParentElement is ScoreboardObjective obj && _score != originalScore)
				{
					obj.Rebuild();
				}
			}
		}

		public string DisplayName
		{
			get => _displayName;
			set
			{
				_displayName = value;
				Left.Text = $"{value}  ";
			}
		}

		private bool _showScore = true;
		public bool ShowScore
		{
			get => _showScore;
			set
			{
				_showScore = value;

				RightText.IsVisible = _showScore;
			}
		}

		private TextElement Left      { get; }
		private Container   Right     { get; }
		private TextElement RightText { get; }
		public ScoreboardEntry(string entryId, uint score, string displayName = "")
		{
			Padding = Thickness.One;
			EntryId = entryId;

			Left = new TextElement()
			{
				//Text = displayName,
				Anchor = Alignment.TopLeft,
				//	Margin = new Thickness(0, 0, 2, 0),
				//ParentElement = this
			};
			
			Right = new Container()
			{
				Padding = new Thickness(2, 0, 0, 0),
				Anchor = Alignment.TopRight
			};
			
			Right.AddChild(RightText = new TextElement()
			{
				Anchor = Alignment.TopRight,
				Text = score.ToString(),
				//ParentElement = this
			});
			
			Score = score;
			DisplayName = displayName;
			
			AddChild(Left);
			AddChild(Right);
		}
	}
	
	public class ScoreboardView : StackContainer
	{
		//private ConcurrentDictionary<string, EntryData> Rows { get; set; } = new ConcurrentDictionary<string, EntryData>();
		private ConcurrentDictionary<string, ScoreboardObjective> Objectives { get; set; } = new ConcurrentDictionary<string, ScoreboardObjective>();

		private ConcurrentDictionary<string, string> EntityObjectives { get; set; } =
			new ConcurrentDictionary<string, string>();

		public ScoreboardView() : base()
		{
			BackgroundOverlay = new Color(Color.Black, 0.5f);
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
	}
}
