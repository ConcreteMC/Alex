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
using Jose;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Logger = NLog.Logger;

namespace Alex.Utils.Auth
{
	public class XboxAuthService
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(XboxAuthService));
		public const string MSA_CLIENT_ID = "android-app://com.mojang.minecraftpe.H62DKCBHJP6WXXIV7RBFOGOL4NAK4E6Y";
		public const string MSA_COBRAND_ID = "90023";
		public const string PLATFORM_NAME = "android2.1.0504.0524";

		private const string UserAuth = "https://user.auth.xboxlive.com/user/authenticate";
		private const string DeviceAuth = "https://device.auth.xboxlive.com/device/authenticate";
		private const string TitleAuth = "https://title.auth.xboxlive.com/title/authenticate";
		private const string XblAuth = "https://xsts.auth.xboxlive.com/xsts/authorize";
		private const string MinecraftAuthUrl = "https://multiplayer.minecraft.net/authentication";

		private const string ClientId = "0000000048183522";
		//"00000000441cc96b";
		
		private const string AuthorizationUri = "https://login.live.com/oauth20_authorize.srf";  // Authorization code endpoint
		private const string RedirectUri = "https://login.live.com/oauth20_desktop.srf";  // Callback endpoint
		private const string RefreshUri = "https://login.live.com/oauth20_token.srf";  // Get tokens endpoint
		private string X { get; set; }
		private string Y { get; set; }
		
		private  ECDsa   EcDsa  { get; }

		private CookieContainer   _cookieContainer;
		private HttpClientHandler _clienthandler;
		private readonly HttpClient        _httpClient;
		public XboxAuthService()
		{
			EcDsa = ConvertToSingKeyFormat(GenerateKeys());
			
			_cookieContainer = new CookieContainer();
			
			_clienthandler = new HttpClientHandler
			{
				AllowAutoRedirect = true, 
				UseCookies = true, 
				CookieContainer = _cookieContainer
			};
			_httpClient = new HttpClient(_clienthandler);
			_httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
			_httpClient.DefaultRequestHeaders.Add("x-xbl-contract-version", "1");
		}

		private static AsymmetricCipherKeyPair GenerateKeys()
		{
			var  curve        = NistNamedCurves.GetByName("P-256");
			var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

			var secureRandom = new SecureRandom();
			var keyParams    = new ECKeyGenerationParameters(domainParams, secureRandom);

			var generator = new ECKeyPairGenerator("ECDSA");
			generator.Init(keyParams);
			
			return generator.GenerateKeyPair();
		}
		
		private ECDsa ConvertToSingKeyFormat(AsymmetricCipherKeyPair key)
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

			X = UrlSafe(pubAsyKey.Q.AffineXCoord.GetEncoded());
			Y = UrlSafe(pubAsyKey.Q.AffineYCoord.GetEncoded());

			return ECDsa.Create(signParam);
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

		public async Task<bool> RequestMinecraftChain(HttpClient client, AuthResponse<XuiDisplayClaims<XstsXui>> token)
		{
			var b = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(MinecraftKeyPair.Public).GetEncoded()
			   .EncodeBase64();

			var body = new MCChainPostData() {IdentityPublicKey = b};

			using (var r = new HttpRequestMessage(HttpMethod.Post, MinecraftAuthUrl))
			{
				r.Headers.Add("Authorization", $"XBL3.0 x={token.DisplayClaims.Xui[0].UserHash};{token.Token}");
				r.Headers.Add("User-Agent", "MCPE/Android");
				r.Headers.Add("Client-Version", McpeProtocolInfo.ProtocolVersion.ToString());

				SetHeadersAndContent(r, body);

				try
				{
					using (var response = await client.SendAsync(r, HttpCompletionOption.ResponseContentRead)
					   .ConfigureAwait(false))
					{
						response.EnsureSuccessStatusCode();

						var rawResponse = await response.Content.ReadAsStringAsync();

						DecodedChain = new ChainData();
						dynamic a     = JObject.Parse(rawResponse);
						var     chain = ((JArray) a.chain).Values<string>().ToArray();
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

			//	//Log.Debug($"Xbox login processed!");
			return true;
		}

		private async Task<AuthResponse<XuiDisplayClaims<XstsXui>>> DoXsts(HttpClient client,
	        AuthResponse<DeviceDisplayClaims> deviceToken,
	        string userToken)
        {
	        //var key = EcDsa.ExportParameters(false);
	        var authRequest = new AuthRequest
	        {
		        RelyingParty = "https://multiplayer.minecraft.net/",
		        TokenType = "JWT",
		        Properties = new Dictionary<string, object>()
		        {
			        {"UserTokens", new string[] {userToken}},
			        /* {"DeviceToken", $"d={deviceToken.Token}"},*/
			        // {/* {"TitleToken", titleToken.Token},*/}
			        {"SandboxId", "RETAIL"},
			        {"ProofKey", new ProofKey(X, Y)}
		        }
	        };


	        using (var r = new HttpRequestMessage(HttpMethod.Post, XblAuth))
	        {
		        r.Headers.Add("User-Agent", "MCPE/Android");
		        r.Headers.Add("Client-Version", McpeProtocolInfo.ProtocolVersion.ToString());
		        SetHeadersAndContent(r, authRequest);

		        using (var response = await client.SendAsync(r, HttpCompletionOption.ResponseContentRead)
			       .ConfigureAwait(false))
		        {
			        response.EnsureSuccessStatusCode();

			        var rawResponse = await response.Content.ReadAsStringAsync();

			        // Console.WriteLine(rawResponse);
			        // Console.WriteLine();
			        return JsonConvert.DeserializeObject<AuthResponse<XuiDisplayClaims<XstsXui>>>(rawResponse);

			        //Log.Debug($"Xsts Auth: {rawResponse}");
		        }
	        }
        }

        private async Task<AuthResponse<XuiDisplayClaims<XstsXui>>> ObtainXbox(HttpClient client,
	        AuthResponse<DeviceDisplayClaims> deviceToken,
	        string accessToken)
        {
	        var authRequest = new Dictionary<string, object>()
	        {
		        {"AccessToken", $"t={accessToken}"},
		        {"AppId", ClientId},
		        {"deviceToken", deviceToken.Token},
		        {"Sandbox", "RETAIL"},
		        {"UseModernGamertag", true},
		        {"SiteName", "user.auth.xboxlive.com"},
		        {"RelyingParty", "https://multiplayer.minecraft.net/"},
		        {"ProofKey", new ProofKey(X, Y)}
	        };

	        var r = new HttpRequestMessage(HttpMethod.Post, "https://sisu.xboxlive.com/authorize");
	        {
		        SetHeadersAndContent(r, authRequest);

		        using (var response = await client.SendAsync(r)
			       .ConfigureAwait(false))
		        {
			        var rawResponse = await response.Content.ReadAsStringAsync();

			        Console.WriteLine($"Response: {rawResponse}");

			        if (!string.IsNullOrWhiteSpace(response.ReasonPhrase))
				        Console.WriteLine($"Reason: {response.ReasonPhrase}");

			        Console.WriteLine($"Response Headers: ");

			        foreach (var header in response.Headers)
			        {
				        Console.WriteLine($"{header.Key}={string.Join(',', header.Value)}");
			        }

			        foreach (var header in response.TrailingHeaders)
			        {
				        Console.WriteLine($"{header.Key}={string.Join(',', header.Value)}");
			        }

			        Console.WriteLine();

			        response.EnsureSuccessStatusCode();

			       return JsonConvert.DeserializeObject<AuthResponse<XuiDisplayClaims<XstsXui>>>(rawResponse);
		        }
	        }
        }

        private async Task<AuthResponse<TitleDisplayClaims>> DoTitleAuth(HttpClient client, AuthResponse<DeviceDisplayClaims> deviceToken, string accessToken)
		{
			var authRequest = new AuthRequest
			{
				RelyingParty = "http://auth.xboxlive.com",
				TokenType = "JWT",
				Properties = new Dictionary<string, object>()
				{
					{"AuthMethod", "RPS"},
					{"DeviceToken", deviceToken.Token},
					{"RpsTicket", $"t={accessToken}" },
					{ "SiteName", "user.auth.xboxlive.com" },
					{"ProofKey", new ProofKey(X, Y)}
				}
			};
			
			using (var r = new HttpRequestMessage(HttpMethod.Post,
				TitleAuth))
			{
				SetHeadersAndContent(r, authRequest);

				using (var response = await client
					.SendAsync(r, HttpCompletionOption.ResponseContentRead)
					.ConfigureAwait(false))
				{
					response.EnsureSuccessStatusCode();

					return
						JsonConvert.DeserializeObject<AuthResponse<TitleDisplayClaims>>(
							await response.Content.ReadAsStringAsync());
				}
			}
		}

        private async Task<AuthResponse<XuiDisplayClaims<Xui>>> ObtainUserToken(HttpClient client, string accessToken)
        {
	        //var key = EcDsa.ExportParameters(false);

	        var authRequest = new AuthRequest
	        {
		        RelyingParty = "http://auth.xboxlive.com",
		        TokenType = "JWT",
		        Properties = new Dictionary<string, object>()
		        {
			        {"AuthMethod", "RPS"},
			        {"RpsTicket", "t=" + accessToken},
			        {"SiteName", "user.auth.xboxlive.com"},
			        {"ProofKey", new ProofKey(X, Y)}
		        }
	        };

	        using (var r = new HttpRequestMessage(HttpMethod.Post, UserAuth))
	        {
		        SetHeadersAndContent(r, authRequest);

		        using (var response = await client.SendAsync(r, HttpCompletionOption.ResponseContentRead)
			       .ConfigureAwait(false))
		        {
			        response.EnsureSuccessStatusCode();

			        return
				        JsonConvert.DeserializeObject<AuthResponse<XuiDisplayClaims<Xui>>>(
					        await response.Content.ReadAsStringAsync());
		        }
	        }
        }

        private void SetHeadersAndContent(HttpRequestMessage request, object data)
		{
			request.Content = SetHttpContent(data, out var jsonData);
			Sign(request, jsonData);
		}

        private async Task<AuthResponse<DeviceDisplayClaims>> ObtainDeviceToken(HttpClient client, string deviceId)
        {
	      //  var id = Guid.NewGuid().ToString();

	        var authRequest = new AuthRequest
	        {
		        RelyingParty = "http://auth.xboxlive.com",
		        TokenType = "JWT",
		        Properties = new Dictionary<string, object>()
		        {
			        {"AuthMethod", "ProofOfPossession"},
			        {"Id", $"{{{deviceId}}}"},
			        {"DeviceType", "Nintendo"},
			        {"SerialNumber", Guid.NewGuid().ToString()},
			        {"Version", "0.0.0.0"},
			        {"ProofKey", new ProofKey(X, Y)}
		        }
	        };

	        var r = new HttpRequestMessage(HttpMethod.Post, DeviceAuth);
	        {
		        SetHeadersAndContent(r, authRequest);

		        using (var response = await client.SendAsync(r, HttpCompletionOption.ResponseContentRead)
			       .ConfigureAwait(false))
		        {
			        var resp = await response.Content.ReadAsStringAsync();

			        response.EnsureSuccessStatusCode();

			        //Console.WriteLine($"Device Response: " + resp);

			        return JsonConvert.DeserializeObject<AuthResponse<DeviceDisplayClaims>>(resp);
		        }
	        }
        }

        private void Sign(HttpRequestMessage request, byte[] body)
		{
			var hash = SHA256.Create();

			var    time = TimeStamp();
			byte[] p    = new byte[8];
			p[0] = (byte) (time >> 56);
			p[1] = (byte) (time >> 48);
			p[2] = (byte) (time >> 40);
			p[3] = (byte) (time >> 32);
			p[4] = (byte) (time >> 24);
			p[5] = (byte) (time >> 16);
			p[6] = (byte) (time >> 8);
			p[7] = (byte) time;

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
					
					buffer.Write(Encoding.UTF8.GetBytes(request.Headers?.Authorization?.ToString() ?? ""));
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

		private static HttpContent SetHttpContent(object content, out byte[] data)
		{
			HttpContent httpContent = null;

			if (content != null)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (TextWriter tw = new StreamWriter(ms))
					{
						using (var jtw = new JsonTextWriter(tw) {Formatting = Formatting.Indented})
						{
							new JsonSerializer().Serialize(jtw, content);
						}
					}

					data = ms.ToArray();
				}

				httpContent = new ByteArrayContent(data);
				httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
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

			HttpClient client = _httpClient;

			var res = await client.PostAsync(request.Url, content);

			string body = await res.Content.ReadAsStringAsync();
			
			return new Response(res.StatusCode, body);
		}

		public async Task<(bool success, BedrockTokenPair token)> DoDeviceCodeLogin(string deviceId, string deviceCode, CancellationToken cancellationToken)
		{
			try
			{
				HttpClient client = _httpClient;
				
				string r = "authorization_pending";
				MsaDeviceAuthPollState token = null;
				while (r == "authorization_pending" && !cancellationToken.IsCancellationRequested)
				{
					var poll = await DevicePollState(client, deviceCode);
					r = poll.Error;
					token = poll;
				}
				
				//Console.WriteLine();
				
				//Console.WriteLine($"Live: {JsonConvert.SerializeObject(token, Formatting.Indented)}");

				//Console.WriteLine();
				
				var deviceToken = await ObtainDeviceToken(client, deviceId);

				//Console.WriteLine();
				
				//Console.WriteLine($"Device Token: {JsonConvert.SerializeObject(deviceToken, Formatting.Indented)}");
				
				//Console.WriteLine();

				var userToken = await ObtainUserToken(client, token.AccessToken);

				//var xsts = await ObtainXbox(client, deviceToken, token.AccessToken);
				var xsts = await DoXsts(client, deviceToken, userToken.Token);
				
				//Console.WriteLine();
				
				//Console.WriteLine($"XSTS Token: {JsonConvert.SerializeObject(xsts, Formatting.Indented)}");
				
				//Console.WriteLine();
				
				
				return (await RequestMinecraftChain(client, xsts), new BedrockTokenPair()
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
			var userToken   = await ObtainUserToken(_httpClient, accessToken);
			var deviceToken = await ObtainDeviceToken(_httpClient, Guid.NewGuid().ToString());
			//var xsts        = await ObtainXbox(_httpClient, deviceToken, accessToken);
			var xsts = await DoXsts(_httpClient, deviceToken, userToken.Token);
			
			return await RequestMinecraftChain(_httpClient, xsts);
		}

		public async Task<(bool success, BedrockTokenPair token)> RefreshTokenAsync(string refreshToken)
		{
			var token = await RefreshAccessToken(refreshToken);
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
		
		private async Task<BedrockTokenPair> RefreshAccessToken(string refreshToken)
		{
			if (string.IsNullOrEmpty(refreshToken))
			{
				throw new ArgumentException("The refresh token is missing.");
			}

			try
			{
				AccessTokens tokens = await Get($"{RefreshUri}", new Dictionary<string, string> { 
					{ "client_id", ClientId  },
					{ "grant_type", "refresh_token" },
					{ "scope", "service::user.auth.xboxlive.com::MBI_SSL" },
					{ "redirect_uri", RedirectUri },
					{ "refresh_token", refreshToken }
				}).ConfigureAwait(false);

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
			Request request = new Request($"https://login.live.com/oauth20_connect.srf?client_id={ClientId}");
			request.PostData["client_id"] = ClientId;
			request.PostData["scope"] = "service::user.auth.xboxlive.com::MBI_SSL";
			request.PostData["response_type"] = "device_code";

			var response = await Send(request);
			if (response.Status != HttpStatusCode.OK)
				throw new Exception("Failed to start sign in flow: non-200 status code");
			
			return JsonConvert.DeserializeObject<MsaDeviceAuthConnectResponse>(response.Body);
		}
		private async Task<MsaDeviceAuthPollState> DevicePollState(HttpClient client, string deviceCode)
		{
			Request request = new Request($"https://login.live.com/oauth20_token.srf?client_id={ClientId}");
			request.PostData["client_id"] = ClientId;
			request.PostData["device_code"] = deviceCode;
			request.PostData["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code";

			var response = await Send(request);
			if (response.Status != HttpStatusCode.OK && (int)response.Status != 400)
				throw new Exception($"Failed to start sign in flow: non-200 status code: {response.Status}");

			return JsonConvert.DeserializeObject<MsaDeviceAuthPollState>(response.Body);
		}


		private async Task<AccessTokens> Get(string uri, Dictionary<string, string> parameters)
		{
			AccessTokens tokens = null;

			var client         = _httpClient;
			var encodedContent = new FormUrlEncodedContent(parameters);
			var response       = await client.PostAsync(uri, encodedContent);

			var res = await response.Content.ReadAsStringAsync();

			tokens = JsonConvert.DeserializeObject<AccessTokens>(res);

			return tokens;
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
	}
}
