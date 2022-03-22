using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Data;
using Alex.Common.Gui.Elements;
using Alex.Common.Utils.Vectors;
using Alex.Gui.Elements.Map;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;
using RocketUI.Input;

namespace Alex.Gui.Dialogs
{
	public class MapDialog : DialogBase
	{
		private MapRenderElement _mapRenderer;
		private AlexButton _zoomInBtn, _zoomOutBtn, _undoButton, _redoButton;
		private IMap _map;
		private MapMarker _selectedMarker = MapMarker.None;

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
				ShowCompass = false,
				AutoZoomLevel = true,
				Rotation = 0f
			};

			_mapRenderer.OnClick += OnMapClicked;

			Container mapContainer = new Container()
			{
				Anchor = Alignment.Fill, Padding = new Thickness(5, 5, 5, 15), Name = "MapContainer"
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
				Margin = new Thickness(2),
				Name = "LeftDebugContainer"
			};

			leftContainer.AddChild(
				new AutoUpdatingTextElement(
					() =>
					{
						if (_map?.Center == null) return string.Empty;

						return $"Coordinates: X={_map.Center.X:F2} Y={_map.Center.Y:F2} Z={_map.Center.Z:F2}";
					}));

			leftContainer.AddChild(
				new AutoUpdatingTextElement(
					() =>
					{
						if (_mapRenderer == null) return string.Empty;

						var cwp = _mapRenderer?.CursorWorldPosition ?? Vector3.Zero;

						return $"Cursor: X={cwp.X:F2} Y={cwp.Y:F2} Z={cwp.Z:F2}";
					}));

			leftContainer.AddChild(new AutoUpdatingTextElement(() => $"Zoom Level: {_mapRenderer.ZoomLevel}"));
			leftContainer.AddChild(new AutoUpdatingTextElement(() => $"Marker: {_selectedMarker}"));

			ContentContainer.AddChild(leftContainer);

			var rightContainer = new StackContainer
			{
				Orientation = Orientation.Vertical,
				Anchor = Alignment.MiddleRight,
				BackgroundOverlay = new Color(Color.Black, 0.3f),
				ChildAnchor = Alignment.TopRight,
				Padding = new Thickness(2),
				Margin = new Thickness(2),
				Name = "Right Side Dock"
			};

			foreach (var marker in Enum.GetValues<MapMarker>())
			{
				if (marker == MapMarker.None) continue;
				var textureResource = marker.ToTexture()?.TextureResource;

				if (!textureResource.HasValue)
					continue;

				var button = new AlexButton(
					" ", () =>
					{
						if (_selectedMarker == marker)
						{
							_selectedMarker = MapMarker.None;

							return;
						}

						_selectedMarker = marker;
					});

				button.IsModern = true;
				button.CanFocus = true;
				button.CanHighlight = true;
				button.Padding = new Thickness(2);

				button.AddChild(
					new Image(textureResource.Value) { Anchor = Alignment.MiddleCenter, Margin = new Thickness(2), });

				rightContainer.AddChild(button);
			}

			mapContainer.AddChild(rightContainer);

			var bottomContainer = new StackContainer
			{
				Orientation = Orientation.Horizontal,
				Anchor = Alignment.BottomFill,
				ChildAnchor = Alignment.MiddleLeft,
				BackgroundOverlay = new Color(Color.Black, 0.3f),
				Name = "Bottom Menu"
			};

			bottomContainer.AddChild(new AlexButton("Close", () => { GuiManager.HideDialog(this); }));
			bottomContainer.AddChild(new AlexButton("Reset", () => { _mapRenderer.Reset(); }));
			bottomContainer.AddChild(_zoomInBtn = new AlexButton("Zoom In", ZoomIn));
			bottomContainer.AddChild(_zoomOutBtn = new AlexButton("Zoom Out", ZoomOut));

			bottomContainer.AddChild(_undoButton = new AlexButton("Undo", UndoAction) { Enabled = false });
			bottomContainer.AddChild(_redoButton = new AlexButton("Redo", RedoAction) { Enabled = false });
			ContentContainer.AddChild(bottomContainer);
		}

		private LinkedList<MapIconAction> _undoActions = new LinkedList<MapIconAction>();
		private LinkedList<MapIconAction> _redoActions = new LinkedList<MapIconAction>();

		private void UpdateActionButtons()
		{
			_undoButton.Enabled = _undoActions.Count > 0;
			_redoButton.Enabled = _redoActions.Count > 0;
		}

		private void RedoAction()
		{
			var last = _redoActions.Last;

			if (last != null)
			{
				_redoActions.RemoveLast();

				var icon = last.Value;
				icon.Apply(_map);
			}

			UpdateActionButtons();
		}

		private void UndoAction()
		{
			var last = _undoActions.Last;

			if (last != null)
			{
				_undoActions.RemoveLast();

				var icon = last.Value;
				icon.Apply(_map);
			}

			UpdateActionButtons();
		}

		private void OnMapClicked(object sender, MapClickedEventArgs e)
		{
			if (e.EventType == ClickEventType.DoubleClick && e.MouseButton == MouseButton.Left)
			{
				if (_selectedMarker != MapMarker.None)
				{
					var mapIcon = new UserMapIcon(_selectedMarker) { Position = e.WorldPosition, Label = "Waypoint" };

					_map.Add(mapIcon);
					_undoActions.AddLast(new AddIconAction(mapIcon));

					_selectedMarker = MapMarker.None;
				}
				else
				{
					var icon = _map.GetMarkers(new ChunkCoordinates(e.WorldPosition), 1).Where(x => x is UserMapIcon)
					   .OrderBy(x => Vector3.Distance(x.Position, e.WorldPosition)).FirstOrDefault();

					if (icon != null)
					{
						_map.Remove(icon);
						_undoActions.AddLast(new RemoveIconAction(icon));
					}
				}
			}
			else if (e.MouseButton == MouseButton.Right)
			{
				_selectedMarker = MapMarker.None;
			}

			UpdateActionButtons();
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
			_mapRenderer.ZoomLevel++;
			UpdateZoomButtons();
		}

		private void ZoomOut()
		{
			_mapRenderer.ZoomLevel--;
			UpdateZoomButtons();
		}

		private MouseState _mouseState = new MouseState();

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			var state = Mouse.GetState();

			if (state != _mouseState)
			{
				var scrollWheelDifference = state.ScrollWheelValue - _mouseState.ScrollWheelValue;

				if (scrollWheelDifference > 0)
				{
					ZoomIn();
				}
				else if (scrollWheelDifference < 0)
				{
					ZoomOut();
				}
			}

			_mouseState = state;
		}

		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_mapRenderer?.Dispose();
		}

		public class MapIconAction
		{
			public MapIcon Icon { get; }

			public MapIconAction(MapIcon icon)
			{
				Icon = icon;
			}

			public virtual void Apply(IMap map) { }
		}

		public class RemoveIconAction : MapIconAction
		{
			/// <inheritdoc />
			public RemoveIconAction(MapIcon icon) : base(icon) { }

			/// <inheritdoc />
			public override void Apply(IMap map)
			{
				base.Apply(map);
				map.Add(Icon);
			}
		}

		public class AddIconAction : MapIconAction
		{
			/// <inheritdoc />
			public AddIconAction(MapIcon icon) : base(icon) { }

			/// <inheritdoc />
			public override void Apply(IMap map)
			{
				base.Apply(map);
				map.Remove(Icon);
			}
		}
	}
}