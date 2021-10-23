using System;
using Alex.Common;
using MojangAPI.Model;

namespace Alex.Utils.Auth
{
	public class JavaSession : Session, ISession
	{
		public string RefreshToken { get; set; }
		public DateTime? ExpiryTime { get; set; } = null;
		
		public JavaSession(Session session)
		{
			Username = session.Username;
			AccessToken = session.AccessToken;
			ClientToken = session.ClientToken;
			UUID = session.UUID;
			
		}
		
		public JavaSession(){}
	}
}