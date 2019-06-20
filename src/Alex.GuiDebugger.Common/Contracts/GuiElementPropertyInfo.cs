using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Alex.GuiDebugger.Common
{

	[ProtoContract]
	public class GuiElementPropertyInfo
	{

		[ProtoMember(1)]
		public string Name { get; set; }

		[ProtoMember(2)]
		public string StringValue { get; set; }
	}
}
