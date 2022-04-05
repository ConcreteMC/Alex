using System;
using Alex.Common;
using Alex.Common.Gui.Elements;
using Alex.Common.Gui.Graphics;
using Alex.Common.Utils;
using Alex.Common.World;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gui.Dialogs
{
	public class WorldLoadingDialog : DialogBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(WorldLoadingDialog));

		private readonly SimpleProgressBar _progressBar;
		private readonly TextElement _textDisplay;
		private readonly TextElement _subTextDisplay;
		private readonly TextElement _percentageDisplay;
		private readonly Button _cancelButton;

		public string Text
		{
			get { return _textDisplay?.Text ?? string.Empty; }
			set
			{
				_textDisplay.Text = value;
			}
		}

		public string SubText
		{
			get { return _subString; }
			set
			{
				_subString = value ?? string.Empty;
			}
		}

		private bool _connectingToServer = false;

		public bool ConnectingToServer
		{
			get
			{
				return _connectingToServer;
			}
			set
			{
				_connectingToServer = value;
				UpdateProgress(CurrentState, Percentage, SubText);
				_cancelButton.IsVisible = value;
			}
		}

		public string CancelText
		{
			get
			{
				return _cancelButton.Text;
			}
			set
			{
				_cancelButton.Text = value;
			}
		}

		public WorldLoadingDialog()
		{
			StackContainer progressBarContainer;

			BackgroundOverlay = new Color(Color.Black, 0.5f);

			Image logo;

			AddChild(
				logo = new Image(AlexGuiTextures.AlexLogo)
				{
					Margin = new Thickness(0, 25, 0, 0), Anchor = Alignment.TopCenter
				});

			AddChild(
				progressBarContainer = new StackContainer()
				{
					//Width  = 300,
					//Height = 35,
					//Margin = new Thickness(12),

					Anchor = Alignment.MiddleCenter,
					Background = Color.Transparent,
					BackgroundOverlay = Color.Transparent,
					Orientation = Orientation.Vertical
				});

			/*progressBarContainer.AddChild(_textDisplay = new TextElement()
			{
				Text      = Text,
				TextColor = (Color) TextColor.White,
				
				Anchor    = Alignment.TopCenter,
				HasShadow = false,
				Scale = 1.5f
			});*/

			MultiStackContainer element;

			progressBarContainer.AddChild(
				element = new MultiStackContainer()
				{
					Width = 300,
					//Height = 35,
					Margin = new Thickness(12),
					Orientation = Orientation.Vertical,
					ChildAnchor = Alignment.Fill
				});

			RocketControl rc = new RocketControl() { Anchor = Alignment.Fill, Margin = new Thickness(3, 0, 3, 0) };

			rc.AddChild(
				_textDisplay = new TextElement()
				{
					Text = Text, TextColor = (Color)TextColor.White.ToXna(), Anchor = Alignment.TopLeft, HasShadow = false
				});

			rc.AddChild(
				_percentageDisplay = new TextElement()
				{
					Text = Text, TextColor = (Color)TextColor.White.ToXna(), Anchor = Alignment.TopRight, HasShadow = false
				});

			element.AddRow(rc);

			var progressRow = element.AddRow(
				_progressBar = new SimpleProgressBar() { Height = 12, Anchor = Alignment.MiddleCenter, });

			progressRow.Margin = new Thickness(3);
			progressRow.ChildAnchor = Alignment.MiddleFill;

			element.AddRow(
				_subTextDisplay = new TextElement()
				{
					Text = Text,
					TextColor = (Color)TextColor.White.ToXna(),
					Anchor = Alignment.TopLeft,
					HasShadow = false,
					Margin = new Thickness(3, 0, 3, 0)
				});

			progressBarContainer.AddChild(
				_cancelButton = new AlexButton("Cancel", Cancel) { Anchor = Alignment.TopLeft });

			//HeaderTitle.TranslationKey = "menu.loadingLevel";

			UpdateProgress(LoadingState.ConnectingToServer, 10);
		}

		private void Cancel()
		{
			CancelAction?.Invoke();
		}

		public LoadingState CurrentState { get; private set; } = LoadingState.ConnectingToServer;
		public int Percentage { get; private set; } = 0;
		public Action CancelAction { get; set; } = null;

		private string _subString = null;

		public void UpdateProgress(LoadingState state, int percentage, string substring = null)
		{
			_subString = substring;

			switch (state)
			{
				case LoadingState.LoadingResources:
					_textDisplay.TranslationKey = "resourcepack.loading";

					break;

				case LoadingState.RetrievingResources:
					_textDisplay.TranslationKey = "resourcepack.downloading";

					break;

				case LoadingState.ConnectingToServer:
					_textDisplay.TranslationKey = "connect.connecting";

					break;

				case LoadingState.LoadingChunks:
					_textDisplay.TranslationKey =
						_connectingToServer ? "multiplayer.downloadingTerrain" : "menu.loadingLevel";

					break;

				case LoadingState.GeneratingVertices:
					_textDisplay.TranslationKey = "menu.generatingTerrain";

					break;

				case LoadingState.Spawning:
					_textDisplay.TranslationKey = "connect.joining";

					break;
			}

			UpdateProgress(percentage);
			CurrentState = state;
			Percentage = percentage;
			SubText = substring;
		}

		public void UpdateProgress(int value)
		{
			_progressBar.Value = value;
			_percentageDisplay.Text = $"{value}%";
		}

		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			ParentElement?.Draw(graphics, gameTime);
			base.OnDraw(graphics, gameTime);
		}

		/// <inheritdoc />
		protected override void OnUpdateLayout()
		{
			ParentElement?.InvalidateLayout();

			base.OnUpdateLayout();
		}

		private double _temp = 0f;
		private int _state = 0;

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			_temp += gameTime.ElapsedGameTime.TotalSeconds;

			if (_temp >= 0.75)
			{
				_temp -= 0.75;

				if (_state + 1 > 3)
				{
					_state = 1;
				}
				else
				{
					_state += 1;
				}

				_subTextDisplay.Text =
					$"{(string.IsNullOrWhiteSpace(_subString) ? "Please wait" : _subString)}{new string('.', _state)}";
			}
		}
	}
}