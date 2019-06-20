using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.GuiDebugger.Models
{
	public class PropertyGridItem
	{

		public string Name { get; set; }

		public bool IsEditable { get; set; }

		public Type ValueType { get; set; }

		public object Value { get; set; }
		public string ValueString
		{
			get => Value?.ToString();
		}

	}
}
