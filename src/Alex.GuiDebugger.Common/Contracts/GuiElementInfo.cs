using System;
using ProtoBuf;

namespace Alex.GuiDebugger.Common
{
	[ProtoContract]
	public class GuiElementInfo
	{
		[ProtoMember(1)]
		public string ElementType { get; set; }

		[ProtoMember(2)]
		public GuiElementPropertyInfo[] PropertyInfos { get; set; }
		
		[ProtoMember(3)]
		public GuiElementInfo[] ChildElements { get; set; }

	}
}
