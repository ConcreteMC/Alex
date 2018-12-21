using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Alex.Utils;
using Newtonsoft.Json;
using NLog;
using Logger = NLog.Logger;

namespace Alex.Services
{
	public class XBLMSAService
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(XBLMSAService));
		public const string MSA_CLIENT_ID = "android-app://com.mojang.minecraftpe.H62DKCBHJP6WXXIV7RBFOGOL4NAK4E6Y";
		public const string MSA_COBRAND_ID = "90023";
		public const string PLATFORM_NAME = "android2.1.0504.0524";

		private static FastRandom RND = new FastRandom();

		public XBLMSAService()
		{
			//PublicClientApplication a = new PublicClientApplication(MSA_CLIENT_ID, "");
			
			//PublicClientApplication a = new PublicClientApplication(MSA_CLIENT_ID);
			//a.AcquireTokenByUsernamePasswordAsync()
		}

		private async Task<Response> Send(Request request)
		{
			var content = new FormUrlEncodedContent(request.PostData);

			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(request.Url);

			var res = await client.PostAsync(request.Url, content);


			string body = await res.Content.ReadAsStringAsync();
			return new Response(res.StatusCode, body);
		}

		public async Task<MsaDeviceAuthConnectResponse> StartDeviceAuthConnect(string scope)
		{
			Request request = new Request("https://login.live.com/oauth20_connect.srf");
			request.PostData["client_id"] = MSA_CLIENT_ID;
			request.PostData["scope"] = scope;
			request.PostData["response_type"] = "device_code";

			var response = await Send(request);
			if (response.Status != HttpStatusCode.OK)
				throw new Exception("Failed to start sign in flow: non-200 status code");
			Log.Info($"Body: " + response.Body);
			return JsonConvert.DeserializeObject<MsaDeviceAuthConnectResponse>(response.Body);
		}

		public async Task<MsaDeviceAuthConnectResponse> RequestToken(string scope)
		{
			Request request = new Request("https://login.live.com/oauth20_connect.srf");
			request.PostData["client_id"] = MSA_CLIENT_ID;
			request.PostData["scope"] = scope;
			request.PostData["response_type"] = "token";

			var response = await Send(request);
			if (response.Status != HttpStatusCode.OK)
				throw new Exception("Failed to start sign in flow: non-200 status code");
			Log.Info($"Body: " + response.Body);
			return JsonConvert.DeserializeObject<MsaDeviceAuthConnectResponse>(response.Body);
		}

		private struct Request
		{
			public string Url;
			public Dictionary<string, string> PostData;

			public Request(string url)
			{
				Url = url;
				PostData = new Dictionary<string, string>();
			}
		}

		struct Response
		{
			public HttpStatusCode Status;
			public string Body;

			public Response(HttpStatusCode status, string body)
			{
				Status = status;
				Body = body;
			}
		};

		public async Task<MsaBrowserResult> AsyncBrowserLogin()
		{
            //Login URL: https://login.live.com/ppsecure/InlineConnect.srf?id=80604&client_id={MSA_CLIENT_ID}&platform={PLATFORM_NAME}
            //Window Size: 480, 64

            return null;
			//request.PostData["client_id"] = MSA_CLIENT_ID;
			//	request.PostData["cobrandid"] = MSA_COBRAND_ID;
		}
	}

	public class MsaBrowserResult
	{
		public Dictionary<string, string> properties;
	}

	public class MsaDeviceAuthConnectResponse
	{
		public string userCode;
		public string deviceCode;
		public string verificationUri;
		public int interval;
		public int expiresIn;
	};

	public class MsaAuthTokenResponse
	{
		public string userId;
		public string tokenType;
		public string scope;
		public string accessToken;
		public string refreshToken;
		public int expiresIn;
	};

	public class MsaDeviceAuthPollResponse : MsaAuthTokenResponse
	{
		public bool userNotSignedInYet = false;
	};
}
