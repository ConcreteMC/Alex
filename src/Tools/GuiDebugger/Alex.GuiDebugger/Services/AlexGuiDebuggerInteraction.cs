using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using Alex.GuiDebugger.Models;
using EasyPipes;
using JKang.IpcServiceFramework;

namespace Alex.GuiDebugger.Services
{
    public class AlexGuiDebuggerInteraction
    {
		#region Singleton

		private static AlexGuiDebuggerInteraction _instance;
		public static  AlexGuiDebuggerInteraction Instance
		{
			get
			{
				if (_instance == null)
					_instance = new AlexGuiDebuggerInteraction();
				return _instance;
			}
		}

		#endregion

		private IpcServiceClient<IGuiDebuggerService> _ipcServiceClient;
		
		public AlexGuiDebuggerInteraction()
		{
			_ipcServiceClient = new IpcServiceClientBuilder<IGuiDebuggerService>()
				.UseNamedPipe(GuiDebuggerConstants.NamedPipeName)
				.Build();
		}


		public async Task<ICollection<ElementTreeItem>> GetElementTreeItems()
		{
			var items = await _ipcServiceClient.InvokeAsync(x => x.GetAllGuiElementInfos());
			return items.Select(ConvertItem).ToList();
		}

		private ElementTreeItem ConvertItem(GuiElementInfo guiElementInfo)
		{
			return new ElementTreeItem(guiElementInfo.Id, guiElementInfo.ElementType)
			{
				Children = guiElementInfo.ChildElements?.Any() ?? false ? guiElementInfo.ChildElements.Select(ConvertItem).ToArray() : null
			};
		}
	}
}
