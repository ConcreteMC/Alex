using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MojangAPI;
using MojangAPI.Model;
using MojangAPI.SecurityQuestion;


namespace Alex.Common.Utils
{
	public static class MojangApi
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private static readonly Mojang _mojang;
		private static readonly MojangAuth _auth;
		private static readonly QuestionFlow _qflow;

		static MojangApi()
		{
			_mojang = new Mojang(_httpClient);
			_auth = new MojangAuth(_httpClient);
			_qflow = new QuestionFlow(_httpClient);
		}

		public static async Task<PlayerProfile> GetPlayerProfileByUUID(string uuid)
		{
			return await _mojang.GetProfileUsingUUID(uuid);
		}

		public static async Task<PlayerProfile> GetPlayerProfile(string accessToken)
		{
			return await _mojang.GetProfileUsingAccessToken(accessToken);
		}

		public static async Task<bool> CheckGameOwnership(string accessToken)
		{
			return await _mojang.CheckGameOwnership(accessToken);
		}
		
		public static async Task<Session> TryMojangLogin(string username, string password, [Optional]Session session)
		{
			MojangAuthResponse res;
			if (session != null)
			{
				res = await _auth.TryAutoLogin(session);
				
				if (res.IsSuccess)
					return res.Session;
			}

			res = await _auth.Authenticate(username, password); // fill your mojang email and password

			if (!res.IsSuccess)
				throw new LoginFailedException(res);

			return res.Session;
		}
		
		public static async Task<Session> TryMicrosoftAuth(MojangAuth auth, string uhs, string xstsToken)
		{
			MojangAuthResponse res;

			res = await auth.RequestSessionWithXbox(uhs, xstsToken);

			if (!res.IsSuccess)
				throw new LoginFailedException(res);

			return res.Session;
		}
	}

	public class LoginFailedException : Exception
	{
		public readonly MojangAuthResponse Response;
		public LoginFailedException(MojangAuthResponse response)
		{
			Response = response;
		}
	}
}