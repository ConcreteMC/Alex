namespace Alex.Common
{
	public interface ISession
	{
		string? Username { get; set; }
		string? AccessToken { get; set; }
		string? UUID { get; set; }
		string? ClientToken { get; set; }
	}
}