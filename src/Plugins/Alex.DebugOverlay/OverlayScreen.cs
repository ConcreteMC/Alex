using System;
using Alex.Utils.Collections;
using RocketUI;

namespace Alex.DebugOverlay
{
	public class OverlayScreen : Screen
	{
		private Alex _alex;
		private TextElement _fpsElement;
		private GuiGraph _graph;
		public OverlayScreen(Alex alex)
		{
			_alex = alex;
			_alex.OnEndDraw += OnEndDraw;

			SizeToWindow = true;
			AddChild(_fpsElement = new TextElement("00 FPS", true)
			{
				Anchor = Alignment.TopLeft,
				TextAlignment = TextAlignment.Left
			});
			//AddChild(_graph = new GuiGraph()
			//{
			//	Anchor = Alignment.TopRight
			//});
			//_graph.AutoSizeMode = AutoSizeMode.None;
			//_graph.Width = 210;
			//_graph.Height = 180;
		}

		private ulong _frameCount = 0;
		private void OnEndDraw(object sender, EventArgs e)
		{
			var frameTime = _alex.FpsMonitor.LastFrameTime;
			//_graph.Add(_frameCount++, frameTime);
			
			//_history.Push(new Record(_frameCount++, frameTime));
			_fpsElement.Text = $"{_alex.FpsMonitor.Value:00} FPS\nFrametime: {_alex.FpsMonitor.AverageFrameTime:F2}ms avg";
			//_streamWriter.WriteLine($"{_frameCounter++},{frameTime}");
		}
		
		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			_alex.OnEndDraw -= OnEndDraw;
			base.Dispose(disposing);
		}
	}
}