using Alex.API.Utils;

namespace Alex.API.Events.World
{
	public class ChatMessageEvent : Event
	{
		public ChatObject ChatObject { get; }

		protected ChatMessageEvent(ChatObject chatObject)
		{
			ChatObject = chatObject;
		}
	}
}