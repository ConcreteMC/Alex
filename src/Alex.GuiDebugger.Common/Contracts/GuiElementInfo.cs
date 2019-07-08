using System;
using System.Runtime.Serialization;

namespace Alex.GuiDebugger.Common
{
	public class GuiElementInfo
	{
		public Guid Id { get; set; }
		
		public string ElementType { get; set; }
		
		public GuiElementInfo[] ChildElements { get; set; }

		public GuiElementInfo()
		{

		}

		public GuiElementInfo(Guid id, string elementType) : this()
		{
			Id = id;
			ElementType = elementType;
		}
	}
}
