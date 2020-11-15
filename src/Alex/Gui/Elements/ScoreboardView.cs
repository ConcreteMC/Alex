using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class ScoreboardElement : GuiContainer
	{
		private GuiTextElement Left { get; }
		private GuiContainer Right { get; }
		
		public ScoreboardElement(string left, uint value)
		{
			//Orientation = Orientation.Horizontal;
			//ChildAnchor = Alignment.FillCenter;

			Left = new GuiTextElement()
			{
				Text = left,
				Anchor = Alignment.TopLeft,
			//	Margin = new Thickness(0, 0, 2, 0),
				//ParentElement = this
			};
			
			Right = new GuiContainer()
			{
				Padding = new Thickness(2, 0, 0, 0),
				Anchor = Alignment.TopRight
			};
			
			Right.AddChild(new GuiTextElement()
			{
				Anchor = Alignment.TopRight,
				Text = $"  {value.ToString()}",
				//ParentElement = this
			});
			
			AddChild(Left);
			AddChild(Right);
		}

		/*	protected override void GetPreferredSize(out Size size, out Size minSize, out Size maxSize)
		{
			base.GetPreferredSize(out size, out minSize, out maxSize);
			
			size = new Size(Left.Width + Right.Width, Math.Max(Left.Height, Right.Height));
		}*/
	}

	public class ScoreboardObjective : GuiStackContainer
	{
		private ConcurrentDictionary<string, ScoreboardEntry> Entries      { get; }
		public  string                                      Name         { get; set; }
		public  string                                      DisplayName  { get; set; }
		public  int                                         SortOrder    { get; set; }
		public  string                                      CriteriaName { get; set; }
		public ScoreboardObjective(string name, string displayName, int sortOrder, string criteriaName)
		{
			Entries = new ConcurrentDictionary<string, ScoreboardEntry>();
			Name = name;
			DisplayName = displayName;
			SortOrder = sortOrder;
			CriteriaName = criteriaName;
			ChildAnchor = Alignment.Fill;
		}

		public void AddOrUpdate(string id, ScoreboardEntry entry)
		{
			if (Entries.TryGetValue(id, out var value))
			{
				value.Score = entry.Score;
				value.DisplayName = entry.DisplayName;
			}

			if (Entries.TryAdd(id, entry))
			{
				
			}

			Rebuild();
			/*Entries.AddOrUpdate(id, entry, (oldId, oldValue) => entry);
			
			Rebuild();*/
		}

		public void Remove(string id)
		{
			if (Entries.TryRemove(id, out var old))
			{
				Rebuild();
			}
		}

		private void Rebuild()
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
			
			ClearChildren();

			GuiContainer container = new GuiContainer();
			//container.BackgroundOverlay = new Color(Color.Black, );
			container.AddChild(new GuiTextElement(DisplayName)
			{
				Anchor = Alignment.CenterX
			});
			AddChild(container);
			
			foreach (var entry in entries)
			{
				AddChild(entry.Value);
			}
		}
	}

	public class ScoreboardEntry : GuiContainer
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
				_score = value;
				RightText.Text = value.ToString();
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

		private GuiTextElement Left      { get; }
		private GuiContainer   Right     { get; }
		private GuiTextElement RightText { get; }
		public ScoreboardEntry(string entryId, uint score, string displayName = "")
		{
			EntryId = entryId;

			Left = new GuiTextElement()
			{
				//Text = displayName,
				Anchor = Alignment.TopLeft,
				//	Margin = new Thickness(0, 0, 2, 0),
				//ParentElement = this
			};
			
			Right = new GuiContainer()
			{
				Padding = new Thickness(2, 0, 0, 0),
				Anchor = Alignment.TopRight
			};
			
			Right.AddChild(RightText = new GuiTextElement()
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
	
	public class ScoreboardView : GuiStackContainer
	{
		//private ConcurrentDictionary<string, EntryData> Rows { get; set; } = new ConcurrentDictionary<string, EntryData>();
		private ConcurrentDictionary<string, ScoreboardObjective> Objectives { get; set; } = new ConcurrentDictionary<string, ScoreboardObjective>();
		public ScoreboardView() : base()
		{
			BackgroundOverlay = new Color(Color.Black, 0.5f);
			ChildAnchor = Alignment.Fill;
		}

		public void RemoveObjective(string name)
		{
			if (Objectives.TryRemove(name, out var objective))
			{
				RemoveChild(objective);
			}
		}
		
		public void AddObjective(ScoreboardObjective objective)
		{
			if (Objectives.TryAdd(objective.Name, objective))
			{
				AddChild(objective);
			}
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
		
		public void AddString(string text)
		{
			GuiContainer container = new GuiContainer();
			container.AddChild(new GuiTextElement(text)
			{
				Anchor = Alignment.CenterX
			});
			AddChild(container);
		}

		public void Remove(string id)
		{
			/*if (Rows.TryRemove(id, out var old))
			{
				RemoveChild(old.Container);
			}*/
		}

		public void AddRow(string id, string key, uint value)
		{
			/*if (Rows.TryRemove(id, out var old))
			{
				RemoveChild(old.Container);
			}*/
			
			/*GuiTextElement keyElement, valueElement;
			keyElement = new GuiTextElement()
			{
				 Text = key,
				 Anchor = Alignment.TopLeft
			};
			
			valueElement = new GuiTextElement()
			{
				 Text = value.ToString(),
				 Anchor = Alignment.TopRight
			};
			
			var container = new GuiContainer();
			GuiMultiStackContainer stackContainer = new GuiMultiStackContainer()
			{
				Orientation = Orientation.Horizontal,
				Anchor = Alignment.Fill,
				ChildAnchor = Alignment.FillCenter
			};
			stackContainer.AddRow(
				r =>
				{
					r.ChildAnchor = Alignment.Fill;
					
					var c = new GuiContainer();
					c.AddChild(keyElement);
					
					r.AddChild(c);
				});
			stackContainer.AddRow(
				r =>
				{
					r.ChildAnchor = Alignment.Fill;
					
					var c = new GuiContainer();
					c.AddChild(valueElement);
					
					r.AddChild(c);
				});
			
			container.AddChild(stackContainer);*/

			/*
			var container = new ScoreboardElement(key, value);

			//AddChild(container);
			if (Rows.TryAdd(id, new EntryData() {Container = container, Score = value}))
			{
				var rows = Rows.ToArray();
				
				foreach (var child in rows)
				{
					RemoveChild(child.Value.Container);
				}
				
				foreach (var row in rows.OrderBy(x => x.Value.Score))
				{
					AddChild(row.Value.Container);
				}
			}*/
			/*
			var row = AddRow(keyElement = new GuiTextElement()
				{
					Text = key,
					Anchor = Alignment.MiddleLeft
				},
				valueElement = new GuiTextElement()
				{
					Text = value,
					Anchor = Alignment.MiddleRight
				});
*/
			//	row.Anchor = Alignment.TopFill;
			//keyElement.Anchor = Alignment.TopLeft;
			//	valueElement.Anchor = Alignment.TopRight;
			//	var keyElementMargin = keyElement.Margin;
			//	keyElementMargin.Left = 0;

			//	keyElement.Margin = keyElementMargin;

			//	var valueElementMargin = valueElement.Margin;
			//		valueElementMargin.Right = 0;

			//		valueElement.Margin = valueElementMargin;
			//keyElement.Anchor = Alignment.MinX;
			//valueElement.Anchor = Alignment.MaxX;

		}

		private class EntryData
		{
			public ScoreboardElement Container { get; set; }
			public uint Score { get; set; }
		}
	}
}
