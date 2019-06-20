using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.GuiDebugger.Common.Services
{
	public interface IGuiDebuggerService
	{

		void HighlightGuiElement(Guid id);
		void DisableHighlight();

		GuiElementInfo[] GetAllGuiElementInfos();

		GuiElementPropertyInfo[] GetElementPropertyInfos(Guid id);

		bool SetElementPropertyValue(Guid id, string propertyName, string propertyValue);

	}
}
