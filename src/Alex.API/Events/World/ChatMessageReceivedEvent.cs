using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Utils;

namespace Alex.API.Events.World
{
	public class ChatMessageReceivedEvent : ChatMessageEvent
	{
		public ChatMessageReceivedEvent(ChatObject chatObject) : base(chatObject)
		{
		}
	}
}
