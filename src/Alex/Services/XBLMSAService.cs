using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Utils;
using Alex.Gui.Forms;
using Alex.Utils;
using Jose;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Logger = NLog.Logger;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Services
{
	public class XBLMSAService
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(XBLMSAService));
		public const string MSA_CLIENT_ID = "android-app://com.mojang.minecraftpe.H62DKCBHJP6WXXIV7RBFOGOL4NAK4E6Y";
		public const string MSA_COBRAND_ID = "90023";
		public const string PLATFORM_NAME = "android2.1.0504.0524";

		private const string UserAuth = "https://user.auth.xboxlive.com/user/authenticate";
		private const string DeviceAuth = "https://device.auth.xboxlive.com/device/authenticate";
		private const string TitleAuth = "https://title.auth.xboxlive.com/title/authenticate";
		private const string XblAuth = "https://xsts.auth.xboxlive.com/xsts/authorize";
		private const string MinecraftAuthUrl = "https://multiplayer.minecraft.net/authentication";
		
		private const string _clientId = "00000000441cc96b";
		
		private string AuthorizationUri = "https://login.live.com/oauth20_authorize.srf";  // Authorization code endpoint
		private string RedirectUri = "https://login.live.com/oauth20_desktop.srf";  // Callback endpoint
		private string RefreshUri = "https://login.live.com/oauth20_token.srf";  // Get tokens endpoint
		
		private static FastRandom RND = new FastRandom();
		
		public AsymmetricCipherKeyPair BouncyKeyPair { get; }
		
		public byte[] X { get; }
		public byte[] Y { get; }
		
		public ECDsa EcDsa { get; }

		private static ECDsa ConvertToSingKeyFormat(AsymmetricCipherKeyPair key)
		{
			ECPublicKeyParameters  pubAsyKey  = (ECPublicKeyParameters)key.Public;
			ECPrivateKeyParameters privAsyKey = (ECPrivateKeyParameters)key.Private;

			var signParam = new ECParameters
			{
				Curve = ECCurve.NamedCurves.nistP256,
				Q =
				{
					X = pubAsyKey.Q.AffineXCoord.GetEncoded(),
					Y = pubAsyKey.Q.AffineYCoord.GetEncoded()
				}
			};
			signParam.D = CryptoUtils.FixDSize(privAsyKey.D.ToByteArrayUnsigned(), signParam.Q.X.Length);
			signParam.Validate();

			return ECDsa.Create(signParam);
		}
		
		public XBLMSAService()
		{
			BouncyKeyPair = GenerateKeys();
			EcDsa = ConvertToSingKeyFormat(BouncyKeyPair);

			ECPublicKeyParameters pubAsyKey = (ECPublicKeyParameters)BouncyKeyPair.Public;
			X = pubAsyKey.Q.AffineXCoord.GetEncoded();
			Y = pubAsyKey.Q.AffineYCoord.GetEncoded();
		}
		
		public AsymmetricCipherKeyPair GenerateKeys()
		{
			var  curve        = NistNamedCurves.GetByName("P-256");
			var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

			var secureRandom = new SecureRandom();
			var keyParams    = new ECKeyGenerationParameters(domainParams, secureRandom);

			var generator = new ECKeyPairGenerator("ECDSA");
			generator.Init(keyParams);
			
			return generator.GenerateKeyPair();
		}

		static readonly char[] padding = { '=' };
		private static string UrlSafe(byte[] a)
		{
			return System.Convert.ToBase64String(a)
				.TrimEnd(padding).Replace('+', '-').Replace('/', '_');
		}

		public static void OpenBrowser(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url);
				}
				else
				{
					throw;
				}
			}
		}
		
		private class MCChainPostData
		{
			[JsonProperty("identityPublicKey")]
			public string IdentityPublicKey { get; set; }
		}

		public class ChainData
		{
			[JsonProperty("chain")]
			public CertificateData[] Chain { get; set; }
		}

		public byte[] MinecraftChain { get; private set; } = null;
		public ChainData DecodedChain { get; private set; } = null;

		public AsymmetricCipherKeyPair MinecraftKeyPair { get; private set; } = CryptoUtils.GenerateClientKey();
        public async Task<bool> RequestMinecraftChain(AuthResponse<XuiDisplayClaims<XstsXui>> token, AsymmetricCipherKeyPair key)
        {
			var b = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(MinecraftKeyPair.Public).GetEncoded().EncodeBase64();

            var body = new MCChainPostData()
			{
				IdentityPublicKey = b
			};
			
			var client = GetClient();
			//using (var client = new HttpClient())
			{
				using (var r = new HttpRequestMessage(HttpMethod.Post,
					MinecraftAuthUrl))
				{
					//r.Headers.Add("x-xbl-contract-version", "1");
					
					r.Content = SetHttpContent(body, out var jsonData);
					r.Headers.Add("Authorization", $"XBL3.0 x={token.DisplayClaims.Xui[0].Uhs};{token.Token}");
					r.Headers.Add("User-Agent", "MCPE/UWP");
					r.Headers.Add("Client-Version", McpeProtocolInfo.ProtocolVersion.ToString());
                    //Sign(r, jsonData);
                    try
                    {
	                    using (var response = await client
		                    .SendAsync(r, HttpCompletionOption.ResponseContentRead)
		                    .ConfigureAwait(false))
	                    {
		                    response.EnsureSuccessStatusCode();

		                    var rawResponse = await response.Content.ReadAsStringAsync();

		                    DecodedChain = new ChainData();
		                    dynamic a = JObject.Parse(rawResponse);
		                    var chain = ((JArray)a.chain).Values<string>().ToArray();
		                    DecodedChain.Chain = new CertificateData[chain.Length];
		                    for (int i = 0; i < chain.Length; i++)
		                    {
			                    var element = chain[i];
			                    try
			                    {
				                    DecodedChain.Chain[i] = JWT.Payload<CertificateData>(element);
			                    }
			                    catch (Exception ex)
			                    {
				                    Log.Error($"Could not parse chain element: {ex.ToString()}");
			                    }
		                    }

		                    //DecodedChain = JsonConvert.DeserializeObject<ChainData>(rawResponse);
		                    MinecraftChain = Encoding.UTF8.GetBytes(rawResponse);
                         //   //Log.Debug($"Chain: {rawResponse}");
	                    }
                    }
                    catch (Exception ex)
                    {
						Log.Warn($"AHHH: {ex.ToString()}");
						return false;
                    }
				}
			}
			
		//	//Log.Debug($"Xbox login processed!");
			return true;
        }
		
		private async Task<AuthResponse<XuiDisplayClaims<XstsXui>>> DoXsts(AuthResponse<DeviceDisplayClaims> deviceToken, AuthResponse<TitleDisplayClaims> titleToken, string userToken)
		{
			//var key = EcDsa.ExportParameters(false);
			var authRequest = new AuthRequest
			{
				RelyingParty = "https://multiplayer.minecraft.net/",
				TokenType = "JWT",
				Properties = new Dictionary<string, object>()
				{
					{"UserTokens", new string[] {userToken}},
					{ "SandboxId", "RETAIL" },
					{"ProofKey", new Dictionary<string, string>()
					{
						{"crv", "P-256"},
						{"alg", "ES256"},
						{"use", "sig"},
						{"kty", "EC"},
						{"x", UrlSafe(X)},
						{"y", UrlSafe(Y)}
					}}
				}
			};
			
			AuthResponse<XuiDisplayClaims<XstsXui>> titleAuthResponse;
			var client = GetClient();
			//using (var client = new HttpClient())
			{
				using (var r = new HttpRequestMessage(HttpMethod.Post,
					XblAuth))
				{
					r.Headers.Add("x-xbl-contract-version", "1");
					r.Content = SetHttpContent(authRequest, out var jsonData);

					Sign(r, jsonData);

					using (var response = await client
						.SendAsync(r, HttpCompletionOption.ResponseContentRead)
						.ConfigureAwait(false))
					{
						response.EnsureSuccessStatusCode();

						var rawResponse = await response.Content.ReadAsStringAsync();

						titleAuthResponse =
							JsonConvert.DeserializeObject<AuthResponse<XuiDisplayClaims<XstsXui>>>(
								rawResponse);

						//Log.Debug($"Xsts Auth: {rawResponse}");
					}
				}
			}

			return titleAuthResponse;
		}
		
		private async Task<AuthResponse<TitleDisplayClaims>> DoTitleAuth(AuthResponse<DeviceDisplayClaims> deviceToken, string accessToken)
		{
			var authRequest = new AuthRequest
			{
				RelyingParty = "http://auth.xboxlive.com",
				TokenType = "JWT",
				Properties = new Dictionary<string, object>()
				{
					{"AuthMethod", "RPS"},
					{"DeviceToken", deviceToken.Token},
					{"RpsTicket", "t=" + accessToken},
					{ "SiteName", "user.auth.xboxlive.com" },
					{"ProofKey", new Dictionary<string, string>()
					{
						{"crv", "P-256"},
						{"alg", "ES256"},
						{"use", "sig"},
						{"kty", "EC"},
						{"x", UrlSafe(X)},
						{"y", UrlSafe(Y)}
					}}
				}
			};
			
			AuthResponse<TitleDisplayClaims> titleAuthResponse;
			var client = GetClient();
			using (var r = new HttpRequestMessage(HttpMethod.Post,
				TitleAuth))
			{
				r.Headers.Add("x-xbl-contract-version", "1");
							
				//var json = JsonConvert.SerializeObject(authRequest);
				//Console.WriteLine(json);
				r.Content = SetHttpContent(authRequest, out var jsonData);
							
				Sign(r, jsonData);
							
				using (var response = await client
					.SendAsync(r, HttpCompletionOption.ResponseContentRead)
					.ConfigureAwait(false))
				{
					response.EnsureSuccessStatusCode();

					titleAuthResponse =
						JsonConvert.DeserializeObject<AuthResponse<TitleDisplayClaims>>(
							await response.Content.ReadAsStringAsync());

					//Log.Debug($"Title Auth: {JsonConvert.SerializeObject(titleAuthResponse)}");
				}
			}

			return titleAuthResponse;
		}

		private async Task<AuthResponse<XuiDisplayClaims<Xui>>> DoUserAuth(string accessToken)
		{
			//var key = EcDsa.ExportParameters(false);
			
			var authRequest = new AuthRequest
			{
				RelyingParty = "http://auth.xboxlive.com",
				TokenType = "JWT",
				Properties = new Dictionary<string, object>()
				{
					{"AuthMethod", "RPS"},
					{"RpsTicket", "t="+accessToken},
					{ "SiteName", "user.auth.xboxlive.com" },
					{"ProofKey", new Dictionary<string, string>()
					{
						{"crv", "P-256"},
						{"alg", "ES256"},
						{"use", "sig"},
						{"kty", "EC"},
						{"x", UrlSafe(X)},
						{"y", UrlSafe(Y)}
					}}
				}
			};

			AuthResponse<XuiDisplayClaims<Xui>> authResponse;
			//using (var client = new HttpClient())
			var client = GetClient();
			{
				using (var r = new HttpRequestMessage(HttpMethod.Post,
					UserAuth))
				{
					r.Headers.Add("x-xbl-contract-version", "1");

					r.Content = SetHttpContent(authRequest, out var jsonData);
					Sign(r, jsonData);

					using (var response = await client
						.SendAsync(r, HttpCompletionOption.ResponseContentRead)
						.ConfigureAwait(false))
					{
						response.EnsureSuccessStatusCode();

						authResponse =
							JsonConvert.DeserializeObject<AuthResponse<XuiDisplayClaims<Xui>>>(
								await response.Content.ReadAsStringAsync());

						//Log.Debug($"User Auth Result: {JsonConvert.SerializeObject(authResponse)}");
					}
				}
			}

			return authResponse;
		}

		private async Task<AuthResponse<DeviceDisplayClaims>> DoDeviceAuth(string token)
		{
			var id = Guid.NewGuid().ToString();
			var serial = Guid.NewGuid().ToString();
			//UUID uuid = new UUID(Guid.NewGuid().ToByteArray());

		//	var key = EcDsa.ExportParameters(false);
			var authRequest = new AuthRequest
			{
				RelyingParty = "http://auth.xboxlive.com",
				TokenType = "JWT",
				Properties = new Dictionary<string, object>()
				{
					//	{"RpsTicket", token},
					//	{"SiteName", "user.auth.xboxlive.com"},
					{"DeviceType", "Nintendo"},
					{"Id",id},
					{"SerialNumber", serial},
					{"Version", "0.0.0.0"},
					{"AuthMethod", "ProofOfPossession"},
					{
						"ProofKey", new Dictionary<string, string>()
						{
							{"crv", "P-256"},
							{"alg", "ES256"},
							{"use", "sig"},
							{"kty", "EC"},
							{"x",UrlSafe(X)},
							{"y", UrlSafe(Y)}
						}
					}
				}
			};

			AuthResponse<DeviceDisplayClaims> deviceAuthResponse;
			//using (var client = new HttpClient())
			var client = GetClient();
			using (var r = new HttpRequestMessage(HttpMethod.Post,
				DeviceAuth))
			{
				r.Headers.Clear();
				
				r.Headers.Add("x-xbl-contract-version", "1");

				//var json = JsonConvert.SerializeObject(authRequest);
				//Console.WriteLine($"Device Request: " + json);
				r.Content = SetHttpContent(authRequest, out var jsonData);

				Sign(r, jsonData);

				using (var response = await client
					.SendAsync(r, HttpCompletionOption.ResponseContentRead)
					.ConfigureAwait(false))
				{
					var resp = await response.Content.ReadAsStringAsync();

					response.EnsureSuccessStatusCode();

					deviceAuthResponse =
						JsonConvert.DeserializeObject<AuthResponse<DeviceDisplayClaims>>(
							resp);

					Console.WriteLine();
				}
			}

			return deviceAuthResponse;
		}

		private void Sign(HttpRequestMessage request, byte[] body)
		{
				var hash = SHA256.Create();

		var time = TimeStamp();
			byte[] p = new byte[8];
			p[0] = (byte)(time >> 56);
			p[1] = (byte)(time >> 48);
			p[2] = (byte)(time >> 40);
			p[3] = (byte)(time >> 32);
			p[4] = (byte)(time >> 24);
			p[5] = (byte)(time >> 16);
			p[6] = (byte)(time >> 8);
			p[7] = (byte)time;
		
			//signer.

			byte[] signed;
			using (MemoryStream buffer = new MemoryStream())
			{
				buffer.WriteByte(0);
				buffer.WriteByte(0);
				buffer.WriteByte(0);
				buffer.WriteByte(1);
				buffer.WriteByte(0);
				
				//Write time
				buffer.Write(p, 0, p.Length);
				
				buffer.WriteByte(0);

				//using (BinaryWriter writer = new BinaryWriter(buffer, Encoding.UTF8))
				{
					buffer.Write(Encoding.UTF8.GetBytes("POST"));
					buffer.WriteByte((byte) 0);
					
					buffer.Write(Encoding.UTF8.GetBytes(request.RequestUri.PathAndQuery));
					buffer.WriteByte((byte) 0);
					
					buffer.WriteByte((byte) 0);
					
					buffer.Write(body);
					buffer.WriteByte((byte) 0);
				}

				byte[] input = buffer.ToArray();

			signed = EcDsa.SignHash(hash.ComputeHash(input));
			}

			byte[] final;
			;
			using (MemoryStream ms = new MemoryStream())
			{
				ms.WriteByte(0);
				ms.WriteByte(0);
				ms.WriteByte(0);
				ms.WriteByte(1);

				//Write Time
				ms.Write(p, 0, p.Length);
				
				//Write signature
				ms.Write(signed, 0, signed.Length);

				final = ms.ToArray();
			}
			
			request.Headers.Add("Signature", Convert.ToBase64String(final));
		}

		private long TimeStamp()
		{
			//return DateTime.UtcNow.ToFileTime();
			long unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1601, 1, 1))).TotalSeconds;
			unixTimestamp += 11644473600;

			unixTimestamp *= 10000000;
			
			return unixTimestamp;
		}
		
		public static void SerializeJsonIntoStream(object value, Stream stream)
		{
			using (var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
			using (var jtw = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
			{
				var js = new JsonSerializer();
				js.Serialize(jtw, value);
				jtw.Flush();
			}
		}
		
		private static HttpContent SetHttpContent(object content, out byte[] data)
		{
			HttpContent httpContent = null;

			if (content != null)
			{
				var ms = new MemoryStream();
				SerializeJsonIntoStream(content, ms);
				ms.Seek(0, SeekOrigin.Begin);
				httpContent = new StreamContent(ms);
				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				data = ms.ToArray();
			}
			else
			{
				data = null;
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

		public async Task<(bool success, BedrockTokenPair token)> DoDeviceCodeLogin(string deviceCode, CancellationToken cancellationToken)
		{
			try
			{
				string r = "authorization_pending";
				MsaDeviceAuthPollState token = null;
				while (r == "authorization_pending" && !cancellationToken.IsCancellationRequested)
				{
					var poll = await DevicePollState(deviceCode);
					r = poll.Error;
					token = poll;
				}

				var userToken = await DoUserAuth(token.AccessToken);
				var deviceAuth = await DoDeviceAuth(userToken.Token);
				var titleAuth = await DoTitleAuth(deviceAuth, token.AccessToken);
				
				var xsts = await DoXsts(deviceAuth, titleAuth, userToken.Token);

				return (await RequestMinecraftChain(xsts, MinecraftKeyPair), new BedrockTokenPair()
				{
					AccessToken = token.AccessToken,
					ExpiryTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn),
					RefreshToken = token.RefreshToken
				});
			}
			catch (OperationCanceledException c)
			{
				return (false, null);
			}
		}

		public async Task<bool> TryAuthenticate(string accessToken)
		{
			var userToken = await DoUserAuth(accessToken);
			var xsts = await DoXsts(null, null, userToken.Token);

			return await RequestMinecraftChain(xsts, MinecraftKeyPair);
		}

		public async Task<(bool success, BedrockTokenPair token)> RefreshTokenAsync(string refreshToken)
		{
			var token = RefreshAccessToken(refreshToken);
			if (token?.AccessToken == null)
			{
				return (false, null);
			}
			
			return (await TryAuthenticate(token.AccessToken), new BedrockTokenPair()
			{
				AccessToken = token.AccessToken,
				ExpiryTime = token.ExpiryTime,
				RefreshToken = token.RefreshToken
			});
		}
		
		public BedrockTokenPair RefreshAccessToken(string refreshToken)
		{
			if (string.IsNullOrEmpty(refreshToken))
			{
				throw new ArgumentException("The refresh token is missing.");
			}

			try
			{
				AccessTokens tokens = GetTokensUsingGET($"{this.RefreshUri}", new Dictionary<string, string> { 
					{ "client_id", _clientId  },
					{ "grant_type", "refresh_token" },
					{ "scope", "service::user.auth.xboxlive.com::MBI_SSL" },
					{ "redirect_uri", RedirectUri },
					{ "refresh_token", refreshToken }
				});

				return new BedrockTokenPair()
				{
					AccessToken = tokens.AccessToken,
					ExpiryTime = DateTime.UtcNow.AddSeconds(tokens.Expiration),
					RefreshToken = tokens.RefreshToken
				};
			}
			catch (WebException ex)
			{
				Log.Warn("RefreshAccessToken failed likely due to an invalid client ID or refresh token\n" + ex.ToString());
			}

			return null;
		}

		public async Task<MsaDeviceAuthConnectResponse> StartDeviceAuthConnect()
		{
			Request request = new Request("https://login.live.com/oauth20_connect.srf");
			request.PostData["client_id"] = _clientId;
			request.PostData["scope"] = "service::user.auth.xboxlive.com::MBI_SSL";
			request.PostData["response_type"] = "device_code";

			var response = await Send(request);
			if (response.Status != HttpStatusCode.OK)
				throw new Exception("Failed to start sign in flow: non-200 status code");
		//	Log.Info($"Body: " + response.Body);
			return JsonConvert.DeserializeObject<MsaDeviceAuthConnectResponse>(response.Body);
		}
		//MsaDeviceAuthPollState
		
		public async Task<MsaDeviceAuthPollState> DevicePollState(string deviceCode)
		{
			Request request = new Request("https://login.live.com/oauth20_token.srf");
			request.PostData["client_id"] = _clientId;
			request.PostData["device_code"] = deviceCode;
			request.PostData["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code";
			//request.PostData["response_type"] = "device_code";
			
			var response = await Send(request);
			if (response.Status != HttpStatusCode.OK && (int)response.Status != 400)
				throw new Exception($"Failed to start sign in flow: non-200 status code: {response.Status}");
			//Log.Info($"Body: " + response.Body);
			return JsonConvert.DeserializeObject<MsaDeviceAuthPollState>(response.Body);
		}
		
		public async Task<MsaDeviceAuthConnectResponse> RequestToken(string scope)
		{
			Request request = new Request("https://login.live.com/oauth20_connect.srf");
			request.PostData["client_id"] = _clientId;
			request.PostData["scope"] = scope;
			request.PostData["response_type"] = "token";

			var response = await Send(request);
			if (response.Status != HttpStatusCode.OK)
				throw new Exception("Failed to start sign in flow: non-200 status code");
		//	Log.Info($"Body: " + response.Body);
			return JsonConvert.DeserializeObject<MsaDeviceAuthConnectResponse>(response.Body);
		}

		private HttpClient _httpClient = new HttpClient();

		private HttpClient GetClient()
		{
			_httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
			return _httpClient;
		}
		
		private static AccessTokens GetTokens(string uri, string body)
		{
			AccessTokens tokens = null;
			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.Method = "POST";
			request.Accept = "application/json";
			request.ContentType = "application/x-www-form-urlencoded";

			request.ContentLength = body.Length;

			using (Stream requestStream = request.GetRequestStream())
			{
				StreamWriter writer = new StreamWriter(requestStream);
				writer.Write(body);
				writer.Close();
			}

			var response = (HttpWebResponse)request.GetResponse();

			using (Stream responseStream = response.GetResponseStream())
			{
				var reader = new StreamReader(responseStream);
				string json = reader.ReadToEnd();
				reader.Close();
				tokens = JsonConvert.DeserializeObject(json, typeof(AccessTokens)) as AccessTokens;
			}

			return tokens;
		}
        
		private static AccessTokens GetTokensUsingGET(string uri, Dictionary<string, string> parameters)
		{
			AccessTokens tokens = null;

			using (var client = GetHttpClient())
			{
				var encodedContent = new FormUrlEncodedContent (parameters);
				var response = client.PostAsync(uri, encodedContent).Result;

				var res = response.Content.ReadAsStringAsync().Result;
                
				tokens = JsonConvert.DeserializeObject<AccessTokens>(res);
			}

			return tokens;
		}
        
		private static HttpClient GetHttpClient()
		{
			HttpClient client = new HttpClient();
			client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

			return client;
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
			public Dictionary<string, object> Properties { get; set; }
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
		
		public class AuthResponse<TClaims>
		{
			[JsonProperty("IssueInstant")]
			public string IssueInstant { get; set; }

			[JsonProperty("NotAfter")]
			public string NotAfter { get; set; }

			[JsonProperty("Token")]
			public string Token { get; set; }

			[JsonProperty("DisplayClaims")]
			public TClaims DisplayClaims { get; set; }
		}

		public class XstsXui
		{
			[JsonProperty("agg")]
			public string AgeGroup { get; set; }
			
			[JsonProperty("gtg")]
			public string Gamertag { get; set; }
			
			[JsonProperty("prv")]
			public string Privileges { get; set; }
			
			[JsonProperty("xid")]
			public string XUID { get; set; }
			
			[JsonProperty("uhs")]
			public string Uhs { get; set; }
		}
		
		public class XuiDisplayClaims<TType>
		{
			[JsonProperty("xui")]
			public TType[] Xui { get; set; }
		}

		public class Xui
		{
			[JsonProperty("uhs")]
			public string Uhs { get; set; }
		}
		
		public class DeviceDisplayClaims
		{
			[JsonProperty("xdi")]
			public XDI Xdi { get; set; }
		}

		public class XDI
		{
			[JsonProperty("did")]
			public string DID { get; set; }
		}
		
		public class TitleDisplayClaims
		{
			[JsonProperty("xti")]
			public XTI Xti { get; set; }
		}

		public class XTI
		{
			[JsonProperty("tid")]
			public string TID { get; set; }
		}
	}

	public class MsaBrowserResult
	{
		public Dictionary<string, string> properties;
	}

	public class MsaDeviceAuthConnectResponse
	{
		public string user_code;
		public string device_code;
		public string verification_uri;
		public int interval;
		public int expires_in;
	};
	
	public class MsaDeviceAuthPollState
	{
		[JsonProperty("user_id")]
		public string UserId;
		
		[JsonProperty("token_type")]
		public string TokenType;
		
		[JsonProperty("scope")]
		public string Scope;
		
		[JsonProperty("access_token")]
		public string AccessToken;
		
		[JsonProperty("refresh_token")]
		public string RefreshToken;
		
		//public int interval;
		[JsonProperty("expires_in")]
		public int ExpiresIn;
		
		[JsonProperty("error")]
		public string Error;
	};

	public class BedrockTokenPair
	{
		[JsonProperty("access_token")]
		public string AccessToken;
		
		[JsonProperty("refresh_token")]
		public string RefreshToken;

		[JsonProperty("expiry_time")]
		public DateTime ExpiryTime;
	}

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
