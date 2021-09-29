using Alex.Common.Utils;
using RocketUI;

namespace Alex.Gui.Elements.Scoreboard
{
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
				TextColor = TextColor.Red.ForegroundColor
				//ParentElement = this
			});
			
			Score = score;
			DisplayName = displayName;
			
			AddChild(Left);
			AddChild(Right);
		}
	}
}