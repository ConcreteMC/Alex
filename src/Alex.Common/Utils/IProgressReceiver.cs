namespace Alex.Common.Utils;

public interface IProgressReceiver
{
	void UpdateProgress(int percentage, string statusMessage);
	void UpdateProgress(int percentage, string statusMessage, string sub);

	void UpdateProgress(int done, int total, string statusMessage) =>
		UpdateProgress((int) (((double) done / (double) total) * 100D), statusMessage);

	void UpdateProgress(int done, int total, string statusMessage, string sub) =>
		UpdateProgress((int) (((double) done / (double) total) * 100D), statusMessage, sub);
}