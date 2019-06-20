using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Alex.GuiDebugger.Common
{
	[DataContract]
	public class GuiElementInfo
	{
		[DataMember]
		public Guid Id { get; set; }
		
		[DataMember]
		public string ElementType { get; set; }
		
		[DataMember]
		public GuiElementInfo[] ChildElements { get; set; }

	}
}
