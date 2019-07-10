using System;

namespace Alex.GuiDebugger.Common.Services
{
	public interface IGuiDebuggerService
    {
        Guid? TryGetElementUnderCursor();

        void HighlightGuiElement(Guid id);
		void DisableHighlight();

		GuiElementInfo[] GetAllGuiElementInfos();

		GuiElementPropertyInfo[] GetElementPropertyInfos(Guid id);

		bool SetElementPropertyValue(Guid id, string propertyName, string propertyValue);
        void EnableUIDebugging();
        bool IsUIDebuggingEnabled();
    }
}
