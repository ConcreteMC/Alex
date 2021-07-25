using System;
using System.IO;
using Alex.Gui;
using Alex.Plugins;
using Microsoft.VisualBasic.CompilerServices;
using RocketUI;

namespace Alex.DebugOverlay
{
	public class DebugOverlayPlugin : Plugin
	{
		private Alex _alex;
		private GuiManager _guiManager;
		private GuiRenderer _guiRenderer;
		public DebugOverlayPlugin(Alex alex)
		{
			_alex = alex;
		}

		private const string Filename = "report.csv";
		private StreamWriter _streamWriter;
		/// <inheritdoc />
		public override void Enabled()
		{
			_guiRenderer = new GuiRenderer();
			_guiRenderer.Init(_alex.GraphicsDevice, _alex.Services);
			_guiRenderer.Font = _alex.GuiRenderer.Font;
			
			_guiManager = new GuiManager(_alex, _alex.Services, _alex.InputManager, _guiRenderer);
			_alex.Components.Add(_guiManager);
			_guiManager.DrawOrder = _alex.GuiManager.DrawOrder + 1;
			//_streamWriter = File.AppendText(Filename);
			//_streamWriter.WriteLine("frame,frametime");
			_guiManager.AddScreen(new OverlayScreen(_alex));
			_alex.OnBeginDraw += OnBeginDraw;
		}

		private ulong _frameCounter = 0;
		private void OnBeginDraw(object sender, EventArgs e)
		{
			var frameTime = _alex.FpsMonitor.LastFrameTime;
			
			//_streamWriter.WriteLine($"{_frameCounter++},{frameTime}");
		}

		/// <inheritdoc />
		public override void Disabled()
		{
			_alex.OnBeginDraw -= OnBeginDraw;
		//	_streamWriter.Dispose();
		}
	}
}