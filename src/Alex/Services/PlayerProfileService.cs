using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alex.API.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Utils;
using MojangSharp.Api;
using MojangSharp.Endpoints;
using NLog;

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
			var auth = await new Authenticate(new Credentials()
			{
				Username = username,
				Password = password
			})
				  .PerformRequestAsync();

			if (auth.IsSuccess)
			{
				var profile = await new Profile(auth.SelectedProfile.Value).PerformRequestAsync();

				bool skinSlim = false;
				PooledTexture2D texture = null;
			
				if (profile.Properties.SkinUri != null)
				{
					SkinUtils.TryGetSkin(profile.Properties.SkinUri, Alex.Instance.GraphicsDevice, out texture);
				}


				//if (profile.Properties.SkinUri != null)
				//	{
				//	SkinUtils.TryGetSkin(profile.Properties.SkinUri, Alex.Instance.GraphicsDevice, out texture);
				//	}

				var playerProfile = new PlayerProfile(auth.SelectedProfile.Value, username, auth.SelectedProfile.PlayerName, new Skin()
					{
						Slim = skinSlim, Texture = texture
					},
					auth.AccessToken, auth.ClientToken);

			//	Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(playerProfile));
				CurrentProfile = playerProfile;
				return await Validate(auth.AccessToken);
			}
			else
			{
				Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(auth.Error.ErrorMessage));
				return false;
			}
		}

		public async Task<bool> TryAuthenticateAsync(PlayerProfile profile)
		{
			return false;
			Requester.ClientToken = profile.ClientToken;
			if (await Validate(profile.AccessToken))
			{
				CurrentProfile = profile;
				return true;
			}

			return false;
		}

		public void Force(PlayerProfile profile)
		{
			CurrentProfile = profile;
		}

		private async Task<bool> Validate(string accessToken)
		{
			return await new Validate(accessToken)
				.PerformRequestAsync()
				.ContinueWith(task =>
			{
                if (task.IsFaulted)
                {
                    Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Validation faulted!"));
                    return false;
                }

				var r = task.Result;
				if (r.IsSuccess)
				{
					Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(CurrentProfile));
					//Alex.Instance.GameStateManager.SetActiveState<TitleState>();
					return true;
				}
				else
				{
					Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(r.Error.ErrorMessage));
					return false;
				}
			});
		}

		public PlayerProfile[] GetProfiles(string type)
		{
			return ProfileManager.GetProfiles(type);
		}
	}
}
