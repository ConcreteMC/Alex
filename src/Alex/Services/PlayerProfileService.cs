using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Utils;
using Alex.Utils.Skins;
using Microsoft.Xna.Framework.Graphics;
using MojangAPI.Model;
using NLog;
using PlayerProfile = Alex.Common.Services.PlayerProfile;
using Skin = Alex.Common.Utils.Skin;

namespace Alex.Services
{
	public class PlayerProfileService : IPlayerProfileService
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PlayerProfileService));
		private PlayerProfile _currentProfile;
		
		private ProfileManager ProfileManager { get; }
		public PlayerProfileService(ProfileManager profileManager)
		{
			ProfileManager = profileManager;
		}
		
		public event EventHandler<PlayerProfileChangedEventArgs> ProfileChanged;
		public event EventHandler<PlayerProfileAuthenticateEventArgs> Authenticate;

		public PlayerProfile CurrentProfile
		{
			get => _currentProfile;
			private set
			{
				if (_currentProfile?.Equals(value) ?? false)
					return;

				_currentProfile = value;
				ProfileChanged?.Invoke(this, new PlayerProfileChangedEventArgs(_currentProfile));
			}
		}

		public async Task<bool> TryAuthenticateAsync(string username, string password)
		{
			Session session = null;

			try
			{
				session = await MojangApi.TryMojangLogin(username, password);
			}
			catch (LoginFailedException ex)
			{
				Log.Warn($"Error: {ex.Response.Result} - {ex.Response.ErrorMessage}");
				Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(ex.Response.ErrorMessage, ex.Response.Result));
				return false;
			}

			if (session == null || session.IsEmpty)
			{
				Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Unknown error.", MojangAuthResult.UnknownError));
				return false;
			}

			var profile = await MojangApi.GetPlayerProfile(session.AccessToken);

			if (profile.IsSuccess)
			{
				Texture2D texture = null;

				if (profile?.Skin?.Url != null)
				{
					SkinUtils.TryGetSkin(new Uri(profile?.Skin?.Url), Alex.Instance.GraphicsDevice, out texture);
				}

				var playerProfile = new PlayerProfile(
					profile.UUID, profile.Name, profile.Name,
					new Skin() { Slim = (profile.Skin?.Model == SkinType.Alex), Texture = texture },
					session.AccessToken, session.ClientToken);

				Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(playerProfile));
				CurrentProfile = playerProfile;

				return true;
			}

			Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(profile.ErrorMessage, MojangAuthResult.UnknownError));
			return false;
		}

		public void Force(PlayerProfile profile)
		{
			CurrentProfile = profile;
		}

		private async Task<bool> Validate(string accessToken)
		{
			var result = await MojangApi.CheckGameOwnership(accessToken);
			if (result)
			{
				Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(CurrentProfile));
				return true;
			}
			
			Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Game ownership check failed.", MojangAuthResult.UnknownError));
			return false;
		}

		public PlayerProfile[] GetProfiles(string type)
		{
			return ProfileManager.GetProfiles(type);
		}
	}
}
