using System;
using System.Collections.Generic;
using System.Text;
using Alex.GuiDebugger.Models;

namespace Alex.GuiDebugger.Services
{
	public interface IGuiDebugDataService
	{

		GuiDebuggerData GuiDebuggerData { get; }

		void RefreshElements();

		void RefreshProperties(GuiDebuggerElementInfo elementInfo);

	}
}
