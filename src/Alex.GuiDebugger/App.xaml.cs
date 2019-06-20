using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using EasyPipes;

namespace Alex.GuiDebugger
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		public static IGuiDebuggerService GuiDebuggerService { get; private set; }

		private Client _server;

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			_server = new Client(GuiDebuggerConstants.NamedPipeName);
			GuiDebuggerService = _server.GetServiceProxy<IGuiDebuggerService>();

			base.OnStartup(e);
		}
	}
}
