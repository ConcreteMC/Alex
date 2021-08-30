using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Alex.Common.Services;
using MojangAPI;
using MojangAPI.Cache;
using MojangAPI.Model;
using MojangAPI.SecurityQuestion;
using PlayerProfile = MojangAPI.Model.PlayerProfile;


namespace Alex.Common.Utils
{
	public class AlexCacheManager<T> : ICacheManager<T>
	{
		private const string FileKey = "java-profile";
		private readonly IStorageSystem _storageSystem;

		public AlexCacheManager(IStorageSystem storageSystem)
		{
			_storageSystem = storageSystem;
		}
		
		/// <inheritdoc />
		public T GetDefaultObject()
		{
			return default(T);
		}

		/// <inheritdoc />
		public T ReadCache()
		{
			if (_storageSystem.TryReadJson<T>(FileKey, out var result))
			{
				return result;
			}

			return GetDefaultObject();
		}

		/// <inheritdoc />
		public void SaveCache(T obj)
		{
			_storageSystem.TryWriteJson(FileKey, obj);
		}
	}
	
	public static class MojangApi
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private static Mojang _mojang;
		private static MojangAuth _auth;
		private static QuestionFlow _qflow;

		private static bool _initialized = false;
		static MojangApi()
		{
			//_mojang = new Mojang(_httpClient);
			//_auth = new MojangAuth(_httpClient);
			//_qflow = new QuestionFlow(_httpClient);
		}

		public static void Init(IStorageSystem storageSystem)
		{
			_initialized = true;
			_mojang = new Mojang(_httpClient);
			_auth = new MojangAuth(_httpClient, new AlexCacheManager<Session>(storageSystem));
			_qflow = new QuestionFlow(_httpClient);
		}

		private static void CheckInit()
		{
			if (_initialized)
				return;

			throw new Exception("MojangAPI not initialized!");
		}
		
		public static async Task<MojangAuthResponse> Validate(string accessToken, string clientToken)
		{
			CheckInit();
			return await _auth.Validate(accessToken, clientToken);
		}
		
		public static async Task<PlayerProfile> GetPlayerProfileByUUID(string uuid)
		{
			CheckInit();
			return await _mojang.GetProfileUsingUUID(uuid);
		}

		public static async Task<PlayerProfile> GetPlayerProfile(string accessToken)
		{
			CheckInit();
			return await _mojang.GetProfileUsingAccessToken(accessToken);
		}

		public static async Task<bool> CheckGameOwnership(string accessToken)
		{
			CheckInit();
			return await _mojang.CheckGameOwnership(accessToken);
		}
		
		public static async Task<Session> TryMojangLogin(string username, string password, [Optional]Session session)
		{
			CheckInit();
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
		
		public static async Task<Session> TryMicrosoftAuth(string uhs, string xstsToken)
		{
			CheckInit();
			MojangAuthResponse res;
			res = await _auth.RequestSessionWithXbox(uhs, xstsToken);

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