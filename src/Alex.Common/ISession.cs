using System;

namespace Alex.Common
{
	public interface ISession
	{
		string? Username { get; set; }
		string? AccessToken { get; set; }
		string? UUID { get; set; }
		string? ClientToken { get; set; }
		string RefreshToken { get; set; }
		
		public DateTime? ExpiryTime { get; set; }
	}
}