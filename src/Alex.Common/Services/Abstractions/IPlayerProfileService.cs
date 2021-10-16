using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MojangAPI;
using MojangAPI.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skin = Alex.Common.Utils.Skin;

namespace Alex.Common.Services
{
    public class PlayerProfile : ISession
    {
        [JsonProperty("Uuid")]
        public string UUID { get; set; }
        public string Username { get; set; }
		public string PlayerName { get; set; }

        [JsonIgnore]
        public Skin Skin { get; set; }

        public string AccessToken { get; set; }
        public string ClientToken { get; set; }
        
        public string RefreshToken { get; set; }
        public DateTime? ExpiryTime { get; set; }

        [JsonIgnore] public bool Authenticated { get; set; } = false;

        public Dictionary<string, JToken> ExtraData { get; set; } = new Dictionary<string, JToken>();
	    public PlayerProfile(string uuid, string username, string playerName, Skin skin, string accessToken, string clientToken, string refreshToken = null, DateTime? expiryTime = null)
        {
            UUID = uuid;
            Username = username;
	        PlayerName = playerName;
            Skin = skin;
            AccessToken = accessToken;
            ClientToken = clientToken;
            RefreshToken = refreshToken;
            ExpiryTime = expiryTime;
        }
        
        public PlayerProfile(){}

        public bool TryGet<T>(string key, out T value)
        {
            value = default;
            if (!ExtraData.TryGetValue(key, out var jObject))
                return false;

            value = jObject.ToObject<T>();
            return true;
        }
        
        public bool Add<T>(string key, T value)
        {
            return ExtraData.TryAdd(key, JToken.FromObject(value));
        }
    }

    public class PlayerProfileChangedEventArgs : EventArgs
    {
        public PlayerProfile Profile { get; }

        public PlayerProfileChangedEventArgs(PlayerProfile profile)
        {
            Profile = profile;
        }
    }
    
    public class PlayerProfileAuthenticateEventArgs : EventArgs
    {
        public bool IsSuccess { get; }

        public PlayerProfile Profile { get; }

        public string ErrorMessage { get; }

        public PlayerProfileAuthenticateEventArgs(PlayerProfile profile)
        {
            IsSuccess = true;
            Profile = profile;
            AuthResult = MojangAuthResult.Success;
        }

        public MojangAuthResult AuthResult { get; }
        public PlayerProfileAuthenticateEventArgs(string errorMessage, MojangAuthResult result)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
            AuthResult = result;
        }

        public string ToUserFriendlyString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ErrorMessage);

            switch (AuthResult)
            {
                case MojangAuthResult.Success:
                    sb.AppendJoin(' ', "Login Successful!");
                    break;

                case MojangAuthResult.BadRequest:
                    sb.AppendJoin(' ', "Server received bad request.");
                    break;

                case MojangAuthResult.InvalidCredentials:
                    sb.AppendJoin(' ', "Invalid Credentials");
                    break;

                case MojangAuthResult.InvalidSession:
                    sb.AppendJoin(' ', "Invalid session.");
                    break;

                case MojangAuthResult.NoProfile:
                    sb.AppendJoin(' ', "No profile found for user.");
                    break;

                case MojangAuthResult.UnknownError:
                    sb.AppendJoin(' ', "Un unknown error occured.");
                    break;
            }

            return sb.ToString();
        }
    }
}
