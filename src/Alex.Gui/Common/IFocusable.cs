using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Gui.Common
{
	public interface IFocusable
	{
		event EventHandler OnFocus;
		event EventHandler OnBlur;

		bool Focused { get; }

	}
}
