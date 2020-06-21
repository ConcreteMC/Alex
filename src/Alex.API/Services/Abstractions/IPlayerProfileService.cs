using System;
using System.Threading.Tasks;
using Alex.API.Utils;
using Newtonsoft.Json;

namespace Alex.API.Services
{
    public class PlayerProfile
    {
        public string Uuid { get; }
        public string Username { get; }
		public string PlayerName { get; }

        [JsonIgnore]
        public Skin Skin { get; set; }

        public string AccessToken { get; }
        public string ClientToken { get; }

		//[JsonIgnore]
		//public bool IsBedrock { get; set; }
        public string Type { get; set; }

        [JsonIgnore] public bool Authenticated { get; set; } = false;

	    public PlayerProfile(string uuid, string username, string playerName, Skin skin, string accessToken, string clientToken, string type = "java")
        {
            Uuid = uuid;
            Username = username;
	        PlayerName = playerName;
            Skin = skin;
            AccessToken = accessToken;
            ClientToken = clientToken;
	        Type = type;
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
        }

        public PlayerProfileAuthenticateEventArgs(string errorMessage)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
        }
    }

    public interface IPlayerProfileService
    {
        event EventHandler<PlayerProfileChangedEventArgs> ProfileChanged;
        event EventHandler<PlayerProfileAuthenticateEventArgs> Authenticate;

        PlayerProfile CurrentProfile { get; }

        Task<bool> TryAuthenticateAsync(string username, string password);
	    Task<bool> TryAuthenticateAsync(PlayerProfile profile);
        void Force(PlayerProfile profile);
        
        PlayerProfile[] GetProfiles(string type);
    }
}
