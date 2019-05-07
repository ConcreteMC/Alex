using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui
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

	public class GuiScaledResolution
	{
		public event EventHandler<UiScaleEventArgs> ScaleChanged;

		public double ScaledWidthD  { get; private set; }
		public double ScaledHeightD { get; private set; }

		public int ScaledWidth  { get; private set; }
		public int ScaledHeight { get; private set; }

		public int ScaleFactor { get; private set; }

		public Matrix TransformMatrix { get; private set; } = Matrix.Identity;
		public Matrix InverseTransformMatrix { get; private set; } = Matrix.Identity;

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

		public GuiScaledResolution(Game game)
		{
			Graphics = game.GraphicsDevice;

			Graphics.DeviceReset          += (sender, args) => Update();
			game.Window.ClientSizeChanged += (sender, args) => Update();
			game.Activated                += (sender, args) => Update();

			//TargetWidth = 480;
			//TargetHeight = 320;

			//_targetWidth = Viewport.TitleSafeArea.Width;
			//_targetHeight = Viewport.TitleSafeArea.Height;
			//Update();
		}

		public void Update()
		{
			var viewportWidth  = Graphics.Viewport.Width;
			var viewportHeight = Graphics.Viewport.Height;

			CalculateScale(viewportWidth, viewportHeight, 0, TargetWidth, TargetHeight, out var scaleFactor, out var scaledWidthD, out var scaledHeightD);

			var scaledWidth  = MathHelpers.IntCeil(scaledWidthD);
			var scaledHeight = MathHelpers.IntCeil(scaledHeightD);

			if (scaledWidth != ScaledWidth || scaledHeight != ScaledHeight || ScaleFactor != scaleFactor)
			{

				ScaleFactor  = scaleFactor;
				ScaledWidthD = scaledWidthD;
				ScaledHeightD = scaledHeightD;

				ScaledWidth  = scaledWidth;
				ScaledHeight = scaledHeight;
				
				var scaleX = (float)(viewportWidth / ScaledWidthD);
				var scaleY = (float)(viewportHeight / ScaledHeightD);


				TransformMatrix = Matrix.CreateScale(scaleX, scaleY, 1f);
				InverseTransformMatrix = Matrix.Invert(TransformMatrix);

				ScaleChanged?.Invoke(this, new UiScaleEventArgs(ScaledWidth, ScaledHeight, ScaleFactor));
			}
		}

		private void CalculateScale(int viewportWidth, int viewportHeight, int guiScale, int targetWidth, int targetHeight, out int scaleFactor, out double scaledWidthD, out double scaledHeightD)
		{
			var isUnicode      = true;
			scaleFactor    = 1;

			if (guiScale == 0)
			{
				guiScale = 1000;
			}

			while (scaleFactor < guiScale && viewportWidth / (scaleFactor + 1.0d) >= targetWidth && viewportHeight / (scaleFactor + 1.0d) >= targetHeight)
			{
				++scaleFactor;
			}

			if (isUnicode && scaleFactor % 2 != 0 && scaleFactor != 1)
			{
				--scaleFactor;
			}

			scaledWidthD  = (double) viewportWidth  / (double) scaleFactor;
			scaledHeightD = (double) viewportHeight / (double) scaleFactor;
		}
	}
}