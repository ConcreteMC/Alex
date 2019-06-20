using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Alex.GuiDebugger.Models
{
	public class GuiElementItem
	{
		public string Type { get; set; }

		public ObservableCollection<GuiElementItem> Children { get; set; }

		public GuiElementItem()
		{
			Children = new ObservableCollection<GuiElementItem>();
		}

	}
}
