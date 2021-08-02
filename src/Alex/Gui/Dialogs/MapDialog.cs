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
			ContentContainer.Padding = new Thickness(10, 10);
			
			_mapRenderer = new MapRenderElement(world)
			{
				AutoSizeMode = AutoSizeMode.GrowOnly,
				Anchor = Alignment.Fill,
				Radius = 128,
				ZoomLevel = ZoomLevel.Maximum
			};
			
			ContentContainer.AddChild(_mapRenderer);
			
			var leftStack = new StackContainer
			{
				Orientation = Orientation.Horizontal, 
				Anchor = Alignment.BottomLeft,
				ChildAnchor = Alignment.FillCenter
			};

			leftStack.AddChild(new AlexButton("Exit", () =>
			{
				GuiManager.HideDialog(this);
			}));
			
			leftStack.AddChild(new AlexButton("Zoom In", ZoomIn));
			leftStack.AddChild(new AlexButton("Zoom Out", ZoomOut));
			
			ContentContainer.AddChild(leftStack);
			
			var rightStack = new StackContainer
			{
				Orientation = Orientation.Vertical, 
				Anchor = Alignment.TopRight
			};
			
			rightStack.AddChild(new AutoUpdatingTextElement(() => $"Zoom: {_mapRenderer.ZoomLevel}", true)
			{
				TextAlignment = TextAlignment.Right
			});
			
			ContentContainer.AddChild(rightStack);
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