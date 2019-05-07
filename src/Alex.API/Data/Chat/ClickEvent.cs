namespace Alex.API.Data.Chat
{
	public class ClickEvent
	{
		/**
		 * The type of action to perform on click
		 */
		public Action action;

		/**
		 * Depends on action
		 *
		 * @see Action
		 */
		public string value;

		public ClickEvent(Action a, string v)
		{
			action = a;
			value = v;
		}

		public ClickEvent()
		{
			
		}

		public enum Action
		{

			/**
			 * Open a url at the path given by
			 * {@link net.md_5.bungee.api.chat.ClickEvent#value}
			 */
			OpenUrl,
			/**
			 * Open a file at the path given by
			 * {@link net.md_5.bungee.api.chat.ClickEvent#value}
			 */
			OpenFile,
			/**
			 * Run the command given by
			 * {@link net.md_5.bungee.api.chat.ClickEvent#value}
			 */
			RunCommand,
			/**
			 * Inserts the string given by
			 * {@link net.md_5.bungee.api.chat.ClickEvent#value} into the players
			 * text box
			 */
			SuggestCommand,
			/**
			 * Change to the page number given by
			 * {@link net.md_5.bungee.api.chat.ClickEvent#value} in a book
			 */
			ChangePage
		}
	}
}
