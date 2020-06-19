using Alex.API.Data;
using Alex.API.Utils;

namespace Alex.API.Events.World
{
	public class ChatMessageReceivedEvent : ChatMessageEvent
	{
		public MessageType Type { get; }
		
		public ChatMessageReceivedEvent(ChatObject chatObject, MessageType type = MessageType.Chat) : base(chatObject)
		{
			Type = type;
		}

		public bool IsChat()
		{
			return Type == MessageType.Raw
			       || Type == MessageType.Chat
			       || Type == MessageType.Whisper
			       || Type == MessageType.Announcement
			       || Type == MessageType.System;
		}
	}
}
