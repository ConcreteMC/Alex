using Alex.API.Utils;

namespace Alex.API.Events.World
{
    public class ChatMessagePublishEvent : ChatMessageEvent
    {
        public ChatMessagePublishEvent(ChatObject chatObject) : base(chatObject)
        {
        }
    }
}