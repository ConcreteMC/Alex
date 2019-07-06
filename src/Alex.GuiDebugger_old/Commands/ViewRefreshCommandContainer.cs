using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Alex.GuiDebugger.Common.Services;
using Alex.GuiDebugger.Services;
using Catel.MVVM;

namespace Alex.GuiDebugger
{
	public class ViewRefreshCommandContainer : CommandContainerBase
	{
		private readonly IGuiDebugDataService _guiDebugDataService;

		public ViewRefreshCommandContainer(ICommandManager commandManager, IGuiDebugDataService guiDebugDataService) : base(Commands.View.Refresh, commandManager)
		{
			_guiDebugDataService = guiDebugDataService;
		}

		protected override async Task ExecuteAsync(object parameter)
		{
			_guiDebugDataService.RefreshElements();

		}
	}
}
