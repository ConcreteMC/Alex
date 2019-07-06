using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using InputGesture = Catel.Windows.Input.InputGesture;

namespace Alex.GuiDebugger
{
	public static class Commands
	{

		public static class View
		{
			public const           string       Refresh             = "View.Refresh";
			public static readonly InputGesture RefreshInputGesture = new InputGesture(Key.F5);

        }

	}
}
