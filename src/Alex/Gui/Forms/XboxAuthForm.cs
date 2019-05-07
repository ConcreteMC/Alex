using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Eto.Forms;
using Newtonsoft.Json;

namespace Alex.Gui.Forms
{
    public class XboxAuthForm : Form
    {
        private WebView MainWebView { get; }
        
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

        private string CodeQueryString = "?client_id={0}&scope=service::user.auth.xboxlive.com::MBI_SSL&response_type=token&redirect_uri={1}&display=touch&locale=en";
        private string AccessBody = "client_id={0}&code={1}&grant_type=authorization_code&redirect_uri={2}";
        private string RefreshBody = "client_id={0}&grant_type=refresh_token&redirect_uri={1}&refresh_token={2}";

        private string _clientId = null;
        private string _uri = null;

        public string AccessToken { get { return this._accessToken; } }
        public string RefreshToken { get { return this._refreshToken; } }
        public int Expiration { get { return this._expiration; } }
        public string Error { get { return this._error; } }
        public string AuthCode => _authorizationCode;
        
        public XboxAuthForm()
        {
            this.ClientSize = new Eto.Drawing.Size(600, 400);

            this.Title = "Sign-in to Xbox Live";
            
            MainWebView = new WebView();

            MainWebView.Navigated += MainWebViewOnNavigated;
            this.Content = MainWebView;

            _clientId = "00000000441cc96b";
            MainWebView.Url = new Uri(string.Format(this.AuthorizationUri + this.CodeQueryString, this._clientId, RedirectUri));
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
                
                Console.WriteLine($"RESPONSE: {e.Uri.Query}");
     
                if (parameters.ContainsKey("code"))
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

                this.Close();
            }
            else if (e.Uri.AbsolutePath.Equals(ErrorPath))
            {
                if (parameters.ContainsKey("error_description"))
                {
                    this._error = WebUtility.UrlDecode(parameters["error_description"]);
                    this.Close(); ;
                }
            }
        }

        public string RefreshAccessToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("The refresh token is missing.");
            }

            try
            {
                var refreshTokenRequestBody = string.Format(this.RefreshBody, this._clientId, WebUtility.UrlEncode(RedirectUri), refreshToken);
                AccessTokens tokens = GetTokens(this.RefreshUri, refreshTokenRequestBody);
                this._accessToken = tokens.AccessToken;
                this._refreshToken = tokens.RefreshToken;
                this._expiration = tokens.Expiration;
            }
            catch (WebException)
            {
                this._error = "RefreshAccessToken failed likely due to an invalid client ID or refresh token";
            }

            return this._accessToken;
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