namespace Alex.Common.Utils;

public class StubbedProgressReceiver : IProgressReceiver
{
	public static StubbedProgressReceiver Instance { get; } = new StubbedProgressReceiver();

	/// <inheritdoc />
	public void UpdateProgress(int percentage, string statusMessage)
	{
		
	}

	/// <inheritdoc />
	public void UpdateProgress(int percentage, string statusMessage, string sub)
	{
		
	}
}