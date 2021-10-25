using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Services;
using Alex.Utils.Auth;
using MojangAPI;
using MojangAPI.Cache;
using MojangAPI.Model;
using MojangAPI.SecurityQuestion;
using MojangAuthResponse = Alex.Utils.Auth.MojangAuthResponse;
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
		private const string ClientID = "00000000402B5328";

		private static XboxAuthService _xboxAuth = new XboxAuthService();
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

		public static async Task<MojangAuthResponse> TryAutoLogin(ISession session)
		{
			return new MojangAuthResponse(await _auth.TryAutoLogin(
				new Session()
				{
					AccessToken = session.AccessToken,
					UUID = session.UUID,
					Username = session.Username,
					ClientToken = session.ClientToken
				}));
		}
		
		public static async Task<MojangAuthResponse> Validate(string accessToken, string clientToken)
		{
			CheckInit();
			return new MojangAuthResponse(await _auth.Validate(accessToken, clientToken));
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
			MojangAPI.Model.MojangAuthResponse res;
			if (session != null)
			{
				res = await _auth.TryAutoLogin(session);
				
				if (res.IsSuccess)
					return res.Session;
			}

			res = await _auth.Authenticate(username, password, session?.ClientToken ?? Guid.NewGuid().ToString()); // fill your mojang email and password

			if (!res.IsSuccess)
				throw new LoginFailedException(res);

			return res.Session;
		}
		
		public static async Task<Session> TryMicrosoftAuth(string uhs, string xstsToken)
		{
			CheckInit();
			var res = await _auth.RequestSessionWithXbox(uhs, xstsToken);

			if (!res.IsSuccess)
				throw new LoginFailedException(res);

			return res.Session;
		}

		public static async Task<MsaDeviceAuthConnectResponse> StartDeviceAuth()
		{
			return await _xboxAuth.StartDeviceAuthConnect(ClientID, "XboxLive.signin", "XboxLive.offline_access");
			//return await _xboxAuth.StartDeviceAuthConnect(ClientID, "service::user.auth.xboxlive.com::MBI_SSL");
		}

		public static async Task<MojangAuthResponse> DoDeviceCodeLogin(MsaDeviceAuthConnectResponse authResponse,
			CancellationToken cancellationToken, params string[] scopes)
		{
			var deviceCodeExpiryTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(authResponse.ExpiresIn));
			string r = "authorization_pending";
			MsaDeviceAuthPollState token = null;
			Stopwatch sw = Stopwatch.StartNew();

			while (r == "authorization_pending" && !cancellationToken.IsCancellationRequested
			                                    && DateTime.UtcNow < deviceCodeExpiryTime)
			{
				sw.Restart();
				var poll = await _xboxAuth.DevicePollState(authResponse.DeviceCode, ClientID);
				r = poll.Error;
				token = poll;
				SpinWait.SpinUntil(() => sw.ElapsedMilliseconds >= 250);
			}

			if (DateTime.UtcNow > deviceCodeExpiryTime)
			{
				return new MojangAuthResponse(MojangAuthResult.UnknownError)
				{
					StatusCode = (int)HttpStatusCode.RequestTimeout,
		//			IsSuccess = false,
					ErrorMessage = "You took too long to login... Please try again."
				};
			}
			
			if (token == null)
				return null;

			return await ExchangeLiveForXbox(token);
		}

		private static async Task<MojangAuthResponse> ExchangeLiveForXbox(BedrockTokenPair token)
		{
			HttpClient client = _httpClient;
			var xblResponse = await _xboxAuth.AuthenticateWithXBL(client, token.AccessToken);
			var xblToken = xblResponse.Token;
			var userHash = xblResponse.DisplayClaims.Xui[0].Uhs;
			
			var xsts = await _xboxAuth.AuthenticatewithJavaXSTS(client, xblToken);

			userHash = xsts.DisplayClaims.Xui[0].UserHash;
			var xstsToken = xsts.Token;
			var rawLoginResult = await _xboxAuth.AuthenticateWithMinecraft(client, userHash, xstsToken);
			
			//var loginResult = await _auth.LoginWithXbox(userHash, xstsToken);

			if (!rawLoginResult.IsSuccess)
				return new MojangAuthResponse(MojangAuthResult.InvalidCredentials)
				{
					ErrorMessage = "Invalid credentials...",
					StatusCode = (int)rawLoginResult.StatusCode
				};

			var minecraftTokens = rawLoginResult.Result;
			
			var ownsGame = await _mojang.CheckGameOwnership(minecraftTokens.AccessToken);

			if (!ownsGame)
				return new MojangAuthResponse(MojangAuthResult.NoProfile)
				{
					ErrorMessage = "You do not own Minecraft!"
				};

			var userProfile = await _mojang.GetProfileUsingAccessToken(minecraftTokens.AccessToken);

			if (!userProfile.IsSuccess)
				return new MojangAuthResponse(MojangAuthResult.NoProfile)
				{
					ErrorMessage = userProfile.ErrorMessage,
					Error = userProfile.Error,
					StatusCode = userProfile.StatusCode
				};
			
			return new MojangAuthResponse(MojangAuthResult.Success)
			{
				Session = new JavaSession()
				{
					AccessToken = minecraftTokens.AccessToken,
					UUID = userProfile.UUID,
					Username =  userProfile.Name,
					ClientToken = Guid.NewGuid().ToString(),
					RefreshToken = token.RefreshToken,
					ExpiryTime = DateTime.UtcNow.Add(TimeSpan.FromSeconds(minecraftTokens.ExpiresIn))
				},
				Profile = userProfile,
				StatusCode = userProfile.StatusCode,
				//	IsSuccess = true
			};
		}
		
		public static Task<MojangAuthResponse> DoDeviceCodeLogin(MsaDeviceAuthConnectResponse authResponse, CancellationToken cancellationToken)
		{
			return DoDeviceCodeLogin(authResponse, cancellationToken, new string[0]);
		}

		public static async Task<MojangAuthResponse> RefreshXboxSession(ISession session)
		{
			var refreshedToken = await _xboxAuth.RefreshAccessToken(session.RefreshToken, ClientID, "XboxLive.signin", "XboxLive.offline_access");
			return await ExchangeLiveForXbox(refreshedToken);
		}

		public static async Task<MojangAuthResponse> RefreshMojangSession(ISession token)
		{
			return new MojangAuthResponse(await _auth.Refresh(
				new Session()
				{
					AccessToken = token.AccessToken,
					UUID = token.UUID,
					Username = token.Username,
					ClientToken = token.ClientToken
				}));
		}
	}

	public class LoginFailedException : Exception
	{
		public readonly MojangAuthResponse Response;
		public LoginFailedException(MojangAuthResponse response)
		{
			Response = response;
		}

		public LoginFailedException(MojangAPI.Model.MojangAuthResponse response)
		{
			Response = new MojangAuthResponse(response);
		}
	}
}