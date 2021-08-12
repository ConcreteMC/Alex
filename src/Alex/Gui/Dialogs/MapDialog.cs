using System;
using Alex.Common.Gui.Elements;
using Alex.Gui.Elements.Map;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Input;

namespace Alex.Gui.Dialogs
{
	public class MapDialog : DialogBase
	{
		private MapRenderElement _mapRenderer;
		private AlexButton _zoomInBtn, _zoomOutBtn;
		private IMap _map;
		public MapDialog(IMap world)
		{
			_map = world;
			
			Anchor = Alignment.Fill;
			ContentContainer.Anchor = Alignment.Fill;
			
			_mapRenderer = new MapRenderElement(_map)
			{
				AutoSizeMode = AutoSizeMode.GrowOnly,
				Anchor = Alignment.Fill,
				Radius = 128,
				ZoomLevel = ZoomLevel.Maximum,
				ShowCompass = true
			};

			Container mapContainer = new Container()
			{
				Anchor = Alignment.Fill,
				Padding = new Thickness(5, 5, 5, 15)
			};
			mapContainer.AddChild(_mapRenderer);
			
			ContentContainer.AddChild(mapContainer);
			
			var leftContainer = new StackContainer
			{
				Orientation = Orientation.Vertical, 
				Anchor = Alignment.TopLeft,
				BackgroundOverlay = new Color(Color.Black, 0.3f),
				ChildAnchor = Alignment.TopLeft,
				Padding = new Thickness(2),
				Margin = new Thickness(2)
			};

			leftContainer.AddChild(
				new AutoUpdatingTextElement(
					() =>
					{
						if (_map?.Center == null) return string.Empty;
						return $"Coordinates: X={_map.Center.X:F2} Y={_map.Center.Y:F2} Z={_map.Center.Z:F2}";
					}));
			
			ContentContainer.AddChild(leftContainer);
			
			var rightContainer = new StackContainer
			{
				Orientation = Orientation.Vertical, 
				Anchor = Alignment.TopRight,
				BackgroundOverlay = new Color(Color.Black, 0.3f),
				ChildAnchor = Alignment.TopRight,
				Padding = new Thickness(2),
				Margin = new Thickness(2)
			};

			rightContainer.AddChild(new AutoUpdatingTextElement(() => $"Zoom Level: {_mapRenderer.ZoomLevel}"));
			
			ContentContainer.AddChild(rightContainer);
			
			var bottomContainer = new StackContainer
			{
				Orientation = Orientation.Horizontal, 
				Anchor = Alignment.BottomFill,
				ChildAnchor = Alignment.MiddleLeft,
				BackgroundOverlay = new Color(Color.Black, 0.3f)
			};

			bottomContainer.AddChild(new AlexButton("Close", () =>
			{
				GuiManager.HideDialog(this);
			}));
			
			bottomContainer.AddChild(_zoomInBtn = new AlexButton("Zoom In", ZoomIn));
			bottomContainer.AddChild(_zoomOutBtn = new AlexButton("Zoom Out", ZoomOut));
			
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

			UpdateZoomButtons();
		}

		/// <inheritdoc />
		public override void OnClose()
		{
			base.OnClose();
			Alex.Instance.Options.AlexOptions.VideoOptions.DisplayHud.Value = _displayHud;
		}

		private void UpdateZoomButtons()
		{
			if (_mapRenderer.ZoomLevel >= ZoomLevel.Minimum && _mapRenderer.ZoomLevel <= ZoomLevel.Maximum)
			{
				_zoomInBtn.Enabled = true;
				_zoomOutBtn.Enabled = true;
				
				return;
			}
			
			if (_mapRenderer.ZoomLevel >= ZoomLevel.Maximum)
			{
				_zoomInBtn.Enabled = true;
				_zoomOutBtn.Enabled = false;

				return;
			}
			
			if (_mapRenderer.ZoomLevel <= ZoomLevel.Minimum)
			{
				_zoomInBtn.Enabled = false;
				_zoomOutBtn.Enabled = true;
			}
		}

		private void ZoomIn()
		{
			_mapRenderer.ZoomLevel--;
			UpdateZoomButtons();
		}

		private void ZoomOut()
		{
			_mapRenderer.ZoomLevel++;
			UpdateZoomButtons();
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_mapRenderer?.Dispose();
		}
	}
}