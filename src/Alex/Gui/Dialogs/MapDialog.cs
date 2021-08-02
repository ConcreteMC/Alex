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
		private World _world;
		private MapRenderElement _mapRenderer;
		public MapDialog(World world)
		{
			_world = world;
			
			Anchor = Alignment.Fill;
			ContentContainer.Anchor = Alignment.Fill;
			ContentContainer.Padding = new Thickness(10, 10);
			
			_mapRenderer = new MapRenderElement(world)
			{
				AutoSizeMode = AutoSizeMode.GrowOnly,
				Anchor = Alignment.Fill
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
			_mapRenderer.Scale = Math.Clamp(_mapRenderer.Scale + 1, 1, 10);
		}

		private void ZoomOut()
		{
			_mapRenderer.Scale = Math.Clamp(_mapRenderer.Scale - 1, 1, 10);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_mapRenderer?.Dispose();
		}
	}
}