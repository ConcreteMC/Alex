using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class GenericLoadingDialog : DialogBase, IProgressReceiver
	{
		private LoadingIndicator _loadingIndicator;
		private TextElement _textElement;

		public string Text
		{
			get
			{
				return _textElement.Text;
			}
			set
			{
				_textElement.Text = value;
			}
		}

		public GenericLoadingDialog()
		{
			Anchor = Alignment.Fill;
			BackgroundOverlay = Color.Black * 0.5f;

			var container = new StackContainer()
			{
				Anchor = Alignment.MiddleCenter, Orientation = Orientation.Vertical
			};

			container.AddChild(_textElement = new TextElement("Authenticating...") { Anchor = Alignment.MiddleCenter });

			container.AddChild(
				_loadingIndicator = new LoadingIndicator()
				{
					Anchor = Alignment.MiddleCenter,
					Width = 300,
					Height = 10,
					ForegroundColor = Color.Red,
					BackgroundColor = Color.Black,
					Margin = new Thickness(30, 30),
					Padding = new Thickness(0, 25, 0, 0)
				});

			AddChild(container);
		}

		public void UpdateProgress(double progress)
		{
			_loadingIndicator.DoPingPong = false;
			_loadingIndicator.Progress = progress;
		}

		/// <inheritdoc />
		public void UpdateProgress(int percentage, string statusMessage)
		{
			UpdateProgress(percentage, statusMessage, null);
		}

		/// <inheritdoc />
		public void UpdateProgress(int percentage, string statusMessage, string sub)
		{
			_loadingIndicator.DoPingPong = false;
			_loadingIndicator.Progress = percentage / 100d;
			Text = statusMessage;
		}
	}
}