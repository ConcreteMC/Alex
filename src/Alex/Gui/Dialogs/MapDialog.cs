using System;
using Alex.Common.Gui.Elements;
using Alex.Gui.Elements.Map;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Input;

namespace Alex.Gui.Dialogs
{
	public class MapDialog : DialogBase
	{
		private MapRenderElement _mapRenderer;
		public MapDialog(WorldMap world)
		{
			Anchor = Alignment.Fill;
			ContentContainer.Anchor = Alignment.Fill;
			
			_mapRenderer = new MapRenderElement(world)
			{
				AutoSizeMode = AutoSizeMode.GrowOnly,
				Anchor = Alignment.Fill,
				Radius = 128,
				ZoomLevel = ZoomLevel.Maximum,
				ShowCompass = false
			};
			
			var leftContainer = new StackContainer
			{
				Orientation = Orientation.Vertical, 
				Anchor = Alignment.TopLeft,
				BackgroundOverlay = new Color(Color.Black, 0.3f),
				ChildAnchor = Alignment.TopLeft,
				Padding = new Thickness(2)
			};

			leftContainer.AddChild(new AutoUpdatingTextElement(() => $"Coordinates: {world.CenterPosition.ToString()}"));
			
			ContentContainer.AddChild(leftContainer);
			
			var rightContainer = new StackContainer
			{
				Orientation = Orientation.Vertical, 
				Anchor = Alignment.TopRight,
				BackgroundOverlay = new Color(Color.Black, 0.3f),
				ChildAnchor = Alignment.TopRight,
				Padding = new Thickness(2)
			};

			rightContainer.AddChild(new AutoUpdatingTextElement(() => $"Zoom: {_mapRenderer.ZoomLevel}"));
			
			ContentContainer.AddChild(rightContainer);

			var middleContainer = new RocketElement()
			{
				//	Orientation = Orientation.Vertical,
				Anchor = Alignment.Fill,
				//	ChildAnchor = Alignment.Fill
			};
			//middleContainer.AddChild(_mapRenderer);
			
			ContentContainer.AddChild(_mapRenderer);
			
			var bottomContainer = new StackContainer
			{
				Orientation = Orientation.Horizontal, 
				Anchor = Alignment.BottomFill,
				ChildAnchor = Alignment.MiddleLeft,
				BackgroundOverlay = new Color(Color.Black, 0.3f)
			};

			bottomContainer.AddChild(new AlexButton("Exit", () =>
			{
				GuiManager.HideDialog(this);
			}));
			
			bottomContainer.AddChild(new AlexButton("Zoom In", ZoomIn));
			bottomContainer.AddChild(new AlexButton("Zoom Out", ZoomOut));
			
			ContentContainer.AddChild(bottomContainer);
		}

		private bool _displayHud;
		/// <inheritdoc />
		public override void OnShow()
		{
			base.OnShow();

			var option = Alex.Instance.Options.AlexOptions.VideoOptions.DisplayHud;
			_displayHud = option.Value;
			
			option.Value = false;
		}
		

		/// <inheritdoc />
		public override void OnClose()
		{
			base.OnClose();
			Alex.Instance.Options.AlexOptions.VideoOptions.DisplayHud.Value = _displayHud;
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
			
		}

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
		}

		private void ZoomIn()
		{
			_mapRenderer.ZoomLevel--;
		//	_mapRenderer.Scale = Math.Clamp(_mapRenderer.Scale + 1, 1, 10);
		}

		private void ZoomOut()
		{
			_mapRenderer.ZoomLevel++;
			//_mapRenderer.Scale = Math.Clamp(_mapRenderer.Scale - 1, 1, 10);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_mapRenderer?.Dispose();
		}
	}
}