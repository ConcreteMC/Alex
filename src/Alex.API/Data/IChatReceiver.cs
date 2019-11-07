using Alex.API.Utils;

namespace Alex.API.Data
{
    public interface IChatReceiver
    {
		void ReceivedTabComplete(int transactionId, int start, int length, TabCompleteMatch[] matches);
    }

	public interface IChatProvider
	{
		void Send(string message);
		void RequestTabComplete(string text, out int transactionId);
	}

	public class TabCompleteMatch
	{
		public string Match;
		public bool HasTooltip = false;
		public ChatObject Tooltip = null;
	}
}
