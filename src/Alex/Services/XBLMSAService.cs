using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Alex.Gui.Forms;
using Alex.Utils;
using Eto.Forms;
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
		
		private Application EtoApplication { get; }
		public XBLMSAService(Application etoApplication)
		{
			EtoApplication = etoApplication;
			//PublicClientApplication a = new PublicClientApplication(MSA_CLIENT_ID, "");

			//PublicClientApplication a = new PublicClientApplication(MSA_CLIENT_ID);
			//a.AcquireTokenByUsernamePasswordAsync()
		}

		public async Task DoXboxAuth()
		{
			//var a = await StartDeviceAuthConnect("service::user.auth.xboxlive.com::MBI_SSL");
			
			//return;
			
			await Task.Run(() =>
			{
				EtoApplication.AsyncInvoke(() =>
				{
					XboxAuthForm authForm = new XboxAuthForm();
					authForm.Closed += async (sender, args) =>
					{
						return;
						Console.WriteLine($"Token {authForm.AccessToken}");
Console.WriteLine($"A: {authForm.AuthCode}");

						Console.WriteLine($"ERROR: {authForm.Error}");
						
						var authRequest = new AuthRequest
						{
							RelyingParty = "http://auth.xboxlive.com",
							TokenType = "JWT",
							Properties = new Properties()
							{
								AuthMethod = "RPS",
								RpsTicket = "t=" + authForm.AccessToken,
								SiteName = "user.auth.xboxlive.com"
							}
						};

						using (var client = new HttpClient())
						using (var r = new HttpRequestMessage(HttpMethod.Post,
							"https://user.auth.xboxlive.com/user/authenticate"))
						{
							r.Headers.Add("x-xbl-contract-version", "1");
							
							var json = JsonConvert.SerializeObject(authRequest);
							Console.WriteLine(json);
							r.Content = CreateHttpContent(authRequest);
							
							using (var response = await client
								.SendAsync(r, HttpCompletionOption.ResponseContentRead)
								.ConfigureAwait(false))
							{
								response.EnsureSuccessStatusCode();

								AuthResponse authResponse =
									JsonConvert.DeserializeObject<AuthResponse>(
										await response.Content.ReadAsStringAsync());

								Console.WriteLine($"Response Token: {authResponse.Token}");
							}
						}
					};
					
					authForm.Show();
				});
			});
		}
		
		public static void SerializeJsonIntoStream(object value, Stream stream)
		{
			using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
			using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
			{
				var js = new JsonSerializer();
				js.Serialize(jtw, value);
				jtw.Flush();
			}
		}
		
		private static HttpContent CreateHttpContent(object content)
		{
			HttpContent httpContent = null;

			if (content != null)
			{
				var ms = new MemoryStream();
				SerializeJsonIntoStream(content, ms);
				ms.Seek(0, SeekOrigin.Begin);
				httpContent = new StreamContent(ms);
				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			}

			return httpContent;
		}
		
		private async Task<Response> Send(Request request)
		{
			var content = new FormUrlEncodedContent(request.PostData);

			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(request.Url);

			if (request.Content != null)
			{
				
			}
			
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

			public object Content;
			
			public Request(string url)
			{
				Url = url;
				PostData = new Dictionary<string, string>();

				Content = null;
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
		
		public class AuthRequest
		{
			[JsonProperty("RelyingParty")]
			public string RelyingParty { get; set; }

			[JsonProperty("TokenType")]
			public string TokenType { get; set; }

			[JsonProperty("Properties")]
			public Properties Properties { get; set; }
		}

		public class Properties
		{
			[JsonProperty("AuthMethod")]
			public string AuthMethod { get; set; }

			[JsonProperty("SiteName")]
			public string SiteName { get; set; }

			[JsonProperty("RpsTicket")]
			public string RpsTicket { get; set; }
		}
		
		public class AuthResponse
		{
			[JsonProperty("IssueInstant")]
			public string IssueInstant { get; set; }

			[JsonProperty("NotAfter")]
			public string NotAfter { get; set; }

			[JsonProperty("Token")]
			public string Token { get; set; }

			[JsonProperty("DisplayClaims")]
			public DisplayClaims DisplayClaims { get; set; }
		}

		public class DisplayClaims
		{
			[JsonProperty("xui")]
			public Xui[] Xui { get; set; }
		}

		public class Xui
		{
			[JsonProperty("uhs")]
			public string Uhs { get; set; }
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
