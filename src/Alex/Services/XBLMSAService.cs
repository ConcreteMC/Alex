extern alias CefGlueWIN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Alex.MSA;
using Alex.Utils;
using CefGlueWIN::Chromely.CefGlue.Winapi;
using CefGlueWIN::Chromely.CefGlue.Winapi.Browser.Handlers;
using CefGlueWIN::Chromely.CefGlue.Winapi.ChromeHost;
using CefGlueWIN::Xilium.CefGlue;
using Chromely.Core;
using Chromely.Core.Helpers;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NLog;
using WinApi.User32;
using WinApi.Windows;
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

			DeviceAuth auth = new DeviceAuth();
			auth.Randomize();

			if (string.IsNullOrWhiteSpace(auth.Puid))
			{

				//a.AcquireTokenByUsernamePasswordAsync()
			}

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
			var jsHandler = new MSAJSHandler();

			ChromelyConfiguration config = ChromelyConfiguration
				.Create()
				//.WithAppArgs(new string[] { })
				.WithHostSize(480, 640)
#if RELEASE
					.WithCustomSetting(CefSettingKeys.SingleProcess, false)
#else
				
				.WithCustomSetting(CefSettingKeys.BrowserSubprocessPath, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CefSharp.BrowserSubprocess.exe"))
			//	.WithCustomSetting(CefSettingKeys.SingleProcess, true)
#endif
				.WithDependencyCheck(true)
				.WithDebuggingMode(true)
				
				.WithStartUrl(
					$"https://login.live.com/ppsecure/InlineConnect.srf?id=80604&client_id={MSA_CLIENT_ID}&platform={PLATFORM_NAME}")
				.RegisterCustomHandler(CefHandlerKey.RequestHandler, typeof(MSARequestHandler))
				.RegisterJsHandler(new ChromelyJsHandler("loginHelper", jsHandler, null, false));
			//.RegisterJsHandler("loginHelper", jsHandler, null, false);
		//	ChromelySchemeHandler c = new ChromelySchemeHandler();
			var factory = WinapiHostFactory.Init();

			
		//	var browserHost = new CefGlueBrowserHost(config);
			CefGlueBrowserHost browserHost = new CefGlueBrowserHost(config);
			await Task.Run(() => {
				using (var window = factory.CreateWindow(() => browserHost,
					"Microsoft Account Sign-In", constructionParams: new FrameWindowConstructionParams()))
				{
					window.SetSize(config.HostWidth, config.HostHeight);
					window.CenterToScreen();
					window.Show();

					IEventLoop c = new EventLoop();
					c.Run(window);
				}
			});

			return null;
			//request.PostData["client_id"] = MSA_CLIENT_ID;
			//	request.PostData["cobrandid"] = MSA_COBRAND_ID;
		}

		private class MSAJSHandler
		{
			private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MSAJSHandler));
			public void Done(string xml)
			{
				Log.Info($"Done: {xml}");
			}
		}

		private class MSARequestHandler : CefGlueRequestHandler
		{
			protected override CefReturnValue OnBeforeResourceLoad(CefBrowser browser, CefFrame frame, CefRequest request,
				CefRequestCallback callback)
			{
			
				if (request.Method == "POST")
				{
					
				}
				return base.OnBeforeResourceLoad(browser, frame, request, callback);
			}

			protected override void OnResourceLoadComplete(CefBrowser browser, CefFrame frame, CefRequest request, CefResponse response,
				CefUrlRequestStatus status, long receivedContentLength)
			{
				if (request.Method == "post" && request.Url.Contains("post.srf"))
				{
					frame.ExecuteJavaScript("loginHelper.done(ServerData);", null, 0);
				}

				base.OnResourceLoadComplete(browser, frame, request, response, status, receivedContentLength);
			}
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
