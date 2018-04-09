using Alex.API.Utils;

namespace Alex.API.Data
{
    public interface IChatReceiver
    {
	    void Receive(ChatObject message);
    }

	public interface IChatProvider
	{
		void Send(string message);
	}
}
