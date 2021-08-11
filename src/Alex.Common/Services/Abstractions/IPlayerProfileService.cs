using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MojangAPI;
using MojangAPI.Model;
using Newtonsoft.Json;
using Skin = Alex.Common.Utils.Skin;

namespace Alex.Common.Services
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
        //public string Type { get; set; }

        [JsonIgnore] public bool Authenticated { get; set; } = false;

	    public PlayerProfile(string uuid, string username, string playerName, Skin skin, string accessToken, string clientToken)
        {
            Uuid = uuid;
            Username = username;
	        PlayerName = playerName;
            Skin = skin;
            AccessToken = accessToken;
            ClientToken = clientToken;
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

    public interface IPlayerProfileService
    {
        event EventHandler<PlayerProfileChangedEventArgs> ProfileChanged;
        event EventHandler<PlayerProfileAuthenticateEventArgs> Authenticate;

        PlayerProfile CurrentProfile { get; }

        Task<bool> TryAuthenticateAsync(string username, string password);
        void Force(PlayerProfile profile);
        
        PlayerProfile[] GetProfiles(string type);
    }
}
