namespace Alex.API.Data
{
    public interface IChatReceiver
    {
	    void Receive(string message);
    }

	public interface IChatProvider
	{
		void Send(string message);
	}
}
