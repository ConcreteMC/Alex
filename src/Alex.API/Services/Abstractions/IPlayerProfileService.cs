using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Services
{
    public class PlayerProfile
    {
        public string Uuid { get; }
        public string Username { get; }

        public Texture2D Skin { get; }

        public string AccessToken { get; }
        public string ClientToken { get; }

        public PlayerProfile(string uuid, string username, Texture2D skin, string accessToken, string clientToken)
        {
            Uuid = uuid;
            Username = username;
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

    }
}
