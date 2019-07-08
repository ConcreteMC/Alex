using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Alex.Services;
using Eto.Forms;
using Newtonsoft.Json;

namespace Alex.Gui.Forms
{
    public class XboxAuthForm //: Form
    {
        public WebView MainWebView { get; }
        
        private string _accessToken = null;
        private string _refreshToken = null;
        private string _authorizationCode = null;
        private int _expiration;
        private string _error = null;

        // Production OAuth server endpoints.

        private string AuthorizationUri = "https://login.live.com/oauth20_authorize.srf";  // Authorization code endpoint
        private string RedirectUri = "https://login.live.com/oauth20_desktop.srf";  // Callback endpoint
        private string RefreshUri = "https://login.live.com/oauth20_token.srf";  // Get tokens endpoint

        private string RedirectPath = "/oauth20_desktop.srf";
        private string ErrorPath = "/err.srf";

        // Parameters to pass to requests. 
        // codeQueryString is the query string for the authorizationUri. To force user log in, include the &prompt=login parameter.
        // accessBody is the request body used with the refreshUri to get the access token using the authorization code.
        // refreshBody is the request body used with the refreshUri to get the access token using a refresh token.

        private string CodeQueryString = "?client_id={0}&redirect_uri={1}&response_type={3}&display=touch&scope={2}&locale=en";
        private string AccessBody = "client_id={0}&code={1}&grant_type=authorization_code&redirect_uri={2}";
        private string RefreshBody = "?client_id={0}&grant_type=refresh_token&refresh_token={1}&scope={2}";

        private string _clientId = null;
        private string _uri = null;

        public string AccessToken { get { return this._accessToken; } }
        public string RefreshToken { get { return this._refreshToken; } }
        public int Expiration { get { return this._expiration; } }
        public string Error { get { return this._error; } }
        public string AuthCode => _authorizationCode;

        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        private XBLMSAService Service { get; }
        public bool UseTokenRequest { get; set; } = true;
        public XboxAuthForm(XBLMSAService service, bool useTokenRequest)
        {
            UseTokenRequest = useTokenRequest;
            Service = service;
            _clientId = "00000000441cc96b";
            
          //  this.ClientSize = new Eto.Drawing.Size(600, 400);

          //  this.Title = "Sign-in to Xbox Live";
            
            /*MainWebView = new WebView();

            MainWebView.Navigated += MainWebViewOnNavigated;
         //   this.Content = MainWebView;

            
          // _clientId = "0000000048093EE3";
            MainWebView.Url = new Uri(string.Format(this.AuthorizationUri + this.CodeQueryString, this._clientId, RedirectUri, WebUtility.UrlEncode("service::user.auth.xboxlive.com::MBI_SSL"), UseTokenRequest ? "token" : "code"));
            MainWebView.DocumentLoaded += MainWebViewOnDocumentLoaded;*/
        }

       // protected override void OnShown(EventArgs e)
       // {
       //     base.OnShown(e);
           
       // }

        private void MainWebViewOnDocumentLoaded(object sender, WebViewLoadedEventArgs e)
        {
            
        }

        private void MainWebViewOnNavigated(object sender, WebViewLoadedEventArgs e)
        {
            Dictionary<string, string> parameters = null;

            if (!string.IsNullOrEmpty(e.Uri.Query))
            {
                parameters = ParseFragment(e.Uri.Query, new char[] { '&', '?' });
            }

            if (e.Uri.AbsolutePath.Equals(RedirectPath))
            {
               // Console.WriteLine($"{sender.GetType()}");
                //Console.WriteLine($"RESPONSE: {e.Uri.Query} | {e.Uri.ToString()}");

                if (UseTokenRequest)
                {
                    HandleTokenResponse(e.Uri);
                //    this.Close();
                    return;
                }
                
                
                if (parameters != null && parameters.ContainsKey("code"))
                {
                    this._authorizationCode = parameters["code"];
                    
                    try
                    {
                        if (!string.IsNullOrEmpty(this._authorizationCode))
                        {
                            var accessTokenRequestBody = string.Format(this.AccessBody, this._clientId, this._authorizationCode, WebUtility.UrlEncode(RedirectUri));
                            AccessTokens tokens = GetTokens(this.RefreshUri, accessTokenRequestBody);
                            this._accessToken = tokens.AccessToken;
                            this._refreshToken = tokens.RefreshToken;
                            this._expiration = tokens.Expiration;
                        }
                    }
                    catch (WebException)
                    {
                        this._error = "GetAccessToken failed likely due to an invalid client ID, client secret, or authorization code";
                    }
                    
                }
                else if (parameters.ContainsKey("error_description"))
                {
                    this._error = WebUtility.UrlDecode(parameters["error_description"]);
                }
                
             //   this.Close();
            }
            else if (e.Uri.AbsolutePath.Equals(ErrorPath))
            {
                if (parameters.ContainsKey("error_description"))
                {
                    this._error = WebUtility.UrlDecode(parameters["error_description"]);
               //     this.Close(); ;
                }
            }
        }

        private void HandleTokenResponse(Uri uri)
        {
            const string i = "access_token=";
            const string b = "&token_type=";
                
            var s = uri.ToString();
            var idx = s.IndexOf(i);
            var idx2 = s.IndexOf(b);
                
            var accesToken = s.Substring(idx + i.Length, idx2 - (idx + i.Length));
            accesToken = WebUtility.UrlDecode(accesToken);
                
            this._accessToken = accesToken;
                
            Console.WriteLine($"YAY TOKEN: {accesToken}");
            Console.WriteLine();

            var theRest = s.Substring(idx2);

            if (!string.IsNullOrEmpty(theRest))
            {
                Dictionary<string, string> parameters = ParseFragment(theRest, new char[] { '&', '?' });
                this._refreshToken = parameters["refresh_token"];
                this._expiration = int.Parse(parameters["expires_in"]);
            }
            
            Console.WriteLine(theRest);
            Console.WriteLine();
        }

        public BedrockTokenPair RefreshAccessToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("The refresh token is missing.");
            }

            try
            {
               // var refreshTokenRequestBody = string.Format(this.RefreshBody, this._clientId,  WebUtility.UrlEncode(refreshToken), WebUtility.UrlEncode("service::user.auth.xboxlive.com::MBI_SSL"));
               // Console.WriteLine($"{refreshTokenRequestBody}");
                AccessTokens tokens = GetTokensUsingGET($"{this.RefreshUri}", new Dictionary<string, string> { 
                    { "client_id", _clientId  }, 
                   // { "client_secret", ClientSecret },
                    { "grant_type", "refresh_token" },
                    { "scope", "service::user.auth.xboxlive.com::MBI_SSL" },
                    { "redirect_uri", RedirectUri },
                    { "refresh_token", refreshToken }
                });
                this._accessToken = tokens.AccessToken;
                this._refreshToken = tokens.RefreshToken;
                this._expiration = tokens.Expiration;
                
                return new BedrockTokenPair()
                {
                    AccessToken = tokens.AccessToken,
                    ExpiryTime = DateTime.UtcNow.AddSeconds(tokens.Expiration),
                    RefreshToken = tokens.RefreshToken
                };
            }
            catch (WebException ex)
            {
                this._error = "RefreshAccessToken failed likely due to an invalid client ID or refresh token\n" + ex.ToString();
            }

            return null;
        }
        
        private Dictionary<string, string> ParseFragment(string queryString, char[] delimeters)
        {
            var parameters = new Dictionary<string, string>();

            string[] pairs = queryString.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pair in pairs)
            {
                string[] nameValaue = pair.Split(new char[] { '=' });
                parameters.Add(nameValaue[0], nameValaue[1]);
            }

            return parameters;
        }

        // Called by GetAccessToken and RefreshAccessToken to get an access token.

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
                
              //  Console.WriteLine($"GET RESULT: {res}");
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

    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class AccessTokens
    {
        [JsonProperty("expires_in")]
        public int Expiration { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}