using System;
using System.Drawing;
using Alex.Common.Utils;
using Alex.Gui.Elements.Web;
using Microsoft.Xna.Framework;
using RocketUI;
using Color = System.Drawing.Color;
using FontStyle = RocketUI.FontStyle;

namespace Alex.Gui.Dialogs
{
	public class BrowserDialog : DialogBase
	{
		private WebElement _webElement;
		private TextElement _titleElement;

		private StackContainer Header { get; }
		public Container Body { get; }
		public MultiStackContainer Footer { get; }

		private string _title;

		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				_titleElement.Text = value;
			}
		}

		protected Container Container => ContentContainer;

		public BrowserDialog(string title, string url)
		{
			Anchor = Alignment.Fill;
			Padding = new Thickness(5, 5);
			Background = Microsoft.Xna.Framework.Color.Black * 0.5f;
			const int width = 356;
			const int height = width;

			Container.Anchor = Alignment.FillCenter;
			//Container.MinWidth = width;
			//Container.Width = width;

			ContentContainer.Width = ContentContainer.MinWidth = width;
			ContentContainer.Height = ContentContainer.MinHeight = height;

			//SetFixedSize(width, height);

			//ContentContainer.AutoSizeMode = AutoSizeMode.None;

			Container.Anchor = Alignment.MiddleCenter;

			Container.AddChild(
				Footer = new MultiStackContainer(
					row =>
					{
						row.Anchor = Alignment.BottomFill;
						//row.Orientation = Orientation.Horizontal;
						row.ChildAnchor = Alignment.BottomFill;
						//row.Margin = new Thickness(3);
						//row.Width = width;
						//row.MaxWidth = width;
					})
				{
					Height = 24,
					Orientation = Orientation.Vertical,
					Anchor = Alignment.BottomFill,
					ChildAnchor = Alignment.BottomCenter,
					Background = Microsoft.Xna.Framework.Color.Black * 0.5f
				});

			Footer.AddRow(row => { });

			Container.AddChild(
				Body = new Container()
				{
					//Margin = new Thickness(0, Header.Height, 0, Footer.Height),
					//AutoSizeMode = AutoSizeMode.None,
					//Height = 100,
					//MaxHeight = 100,
					//Orientation = Orientation.Vertical,
					Anchor = Alignment.Fill,
					//ChildAnchor = Alignment.MiddleCenter,
					Background = Microsoft.Xna.Framework.Color.Black * 0.35f
					//HorizontalScrollMode = ScrollMode.Hidden
				});

			Body.AddChild(
				_webElement = new WebElement()
				{
					Source = new Uri(url), Anchor = Alignment.Fill, Homepage = url, Transparency = 0.1f
				});

			Container.AddChild(
				Header = new StackContainer()
				{
					Anchor = Alignment.TopFill,
					ChildAnchor = Alignment.BottomCenter,
					Height = 32,
					Padding = new Thickness(3),
					Background = Microsoft.Xna.Framework.Color.Black * 0.5f
				});

			Header.AddChild(
				_titleElement = new TextElement()
				{
					Text = Title,
					TextColor = (Microsoft.Xna.Framework.Color)TextColor.White,
					Scale = 2f,
					FontStyle = FontStyle.DropShadow,
					Anchor = Alignment.BottomCenter,
				});

			Body.Margin = new Thickness(0, Header.Height, 0, Footer.Height);

			Title = title;
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			_webElement.Focus();
		}

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			//Title = _webElement.Title;
		}
	}
}