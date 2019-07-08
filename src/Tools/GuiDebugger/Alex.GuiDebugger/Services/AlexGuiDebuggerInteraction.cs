using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.GuiDebugger.Common;
using Alex.GuiDebugger.Common.Services;
using Alex.GuiDebugger.Models;
using EasyPipes;

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

		private Client _client;

		public IGuiDebuggerService GuiDebuggerService { get; }

		public AlexGuiDebuggerInteraction()
		{
			_client = new Client(GuiDebuggerConstants.NamedPipeName);
			GuiDebuggerService = _client.GetServiceProxy<IGuiDebuggerService>();
		}


		public ICollection<ElementTreeItem> GetElementTreeItems()
		{
			var items = GuiDebuggerService.GetAllGuiElementInfos();
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
