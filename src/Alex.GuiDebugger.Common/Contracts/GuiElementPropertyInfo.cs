using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ProtoBuf;

namespace Alex.GuiDebugger.Common
{

	[DataContract]
	public class GuiElementPropertyInfo
	{
		
		[DataMember]
		public string Name { get; set; }
		
		[DataMember]
		public string StringValue { get; set; }
	}
}
