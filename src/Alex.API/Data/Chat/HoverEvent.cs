namespace Alex.API.Data.Chat
{
	public class HoverEvent
	{
		public Action action;
		public BaseComponent[] value;

		public enum Action
		{

			ShowText,
			ShowAchievement,
			ShowItem,
			ShowEntity
		}
	}
}
