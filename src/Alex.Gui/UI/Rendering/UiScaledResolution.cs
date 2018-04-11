using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.UI.Rendering
{
	public class UiScaleEventArgs : EventArgs
	{
		public int ScaledWidth  { get; }
		public int ScaledHeight { get; }
		public int ScaleFactor  { get; }

		public UiScaleEventArgs(int scaledWidth, int scaledHeight, int scaleFactor)
		{
			ScaledWidth  = scaledWidth;
			ScaledHeight = scaledHeight;
			ScaleFactor  = scaleFactor;
		}
	}

	public class UiScaledResolution
	{
		public event EventHandler<UiScaleEventArgs> ScaleChanged;

		public double ScaledWidthD  { get; private set; }
		public double ScaledHeightD { get; private set; }

		public int ScaledWidth  { get; private set; }
		public int ScaledHeight { get; private set; }

		public int ScaleFactor { get; private set; }

		public Matrix TransformMatrix { get; private set; } = Matrix.Identity;

		private int _targetWidth = 320;

		public int TargetWidth
		{
			get => _targetWidth;
			set
			{
				_targetWidth = value;
				Update();
			}
		}

		private int _targetHeight = 240;

		public int TargetHeight
		{
			get => _targetHeight;
			set
			{
				_targetHeight = value;
				Update();
			}
		}

		private int _guiScale = 1000;

		public int GuiScale
		{
			get => _guiScale;
			set
			{
				_guiScale = Math.Max(0, value);
				_guiScale = _guiScale == 0 ? 1000 : _guiScale;
				Update();
			}
		}

		private GraphicsDevice Graphics { get; }
		private Viewport       Viewport => Graphics.Viewport;

		public UiScaledResolution(Game game)
		{
			Graphics = game.GraphicsDevice;

			Graphics.DeviceReset          += (sender, args) => Update();
			game.Window.ClientSizeChanged += (sender, args) => Update();
			game.Activated                += (sender, args) => Update();

			//Update();
		}

		public void Update()
		{
			var viewportWidth  = Graphics.PresentationParameters.BackBufferWidth;
			var viewportHeight = Graphics.PresentationParameters.BackBufferHeight;

			var scaleFactor = 1;

			while (scaleFactor < GuiScale && viewportWidth / (scaleFactor + 1) >= TargetWidth &&
			       viewportHeight                          / (scaleFactor + 1) >= TargetHeight)
			{
				++scaleFactor;
			}


			ScaledWidthD  = (double) viewportWidth  / (double) scaleFactor;
			ScaledHeightD = (double) viewportHeight / (double) scaleFactor;
			var scaledWidth  = (int) Math.Ceiling(ScaledWidthD);
			var scaledHeight = (int) Math.Ceiling(ScaledHeightD);

			if (scaledWidth != ScaledWidth || scaledHeight != ScaledHeight || ScaleFactor != scaleFactor)
			{

				ScaleFactor  = scaleFactor;
				ScaledWidth  = scaledWidth;
				ScaledHeight = scaledHeight;
				
				var scaleX = viewportWidth / ScaledWidth;
				var scaleY = viewportHeight / ScaledHeight;

				var transformMatrix = Matrix.CreateScale(scaleX, scaleY, 1f);

				TransformMatrix = transformMatrix;

				ScaleChanged?.Invoke(this, new UiScaleEventArgs(ScaledWidth, ScaledHeight, ScaleFactor));
			}
		}
	}
}