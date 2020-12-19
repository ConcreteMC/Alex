using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

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

		public Vector2 ElementScale { get; private set; } = Vector2.One;
		
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

		private int _guiScale = 2;
		private Size _viewportSize;

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

		public Size ViewportSize
		{
			get => _viewportSize;
			set
			{
				_viewportSize = value;
				Update();
			}
		}

		private GraphicsDevice Graphics { get; }
		private Viewport       Viewport => Graphics.Viewport;

		public GuiScaledResolution(Game game)
		{
			Graphics = game.GraphicsDevice;

			Graphics.DeviceReset          += (sender, args) => Update();
//			game.Window.ClientSizeChanged += (sender, args) => ViewportSize = new Size(Graphics.Viewport.Width,  Graphics.Viewport.Height);
//			game.Activated                += (sender, args) => Update();

			//TargetWidth = 480;
			//TargetHeight = 320;

			//_targetWidth = Viewport.TitleSafeArea.Width;
			//_targetHeight = Viewport.TitleSafeArea.Height;
			//Update();
		}

		public void Update()
		{
			var viewportWidth  = ViewportSize.Width;
			var viewportHeight = ViewportSize.Height;

			CalculateScale(viewportWidth, viewportHeight, GuiScale, TargetWidth, TargetHeight, out var scaleFactor, out var scaledWidthD, out var scaledHeightD);

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

				ElementScale = new Vector2(scaleX, scaleY);

				TransformMatrix = Matrix.CreateScale(scaleFactor);
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