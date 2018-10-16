using System;
using System.Collections.Generic;
using System.Text;

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
