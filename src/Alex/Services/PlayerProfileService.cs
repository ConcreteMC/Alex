using System;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;
using MojangSharp.Api;
using MojangSharp.Endpoints;
using Newtonsoft.Json;
using NLog;

namespace Alex.Services
{
	public class PlayerProfileService : IPlayerProfileService
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PlayerProfileService));
		private PlayerProfile _currentProfile;

		private XBLMSAService XblService { get; }
		private ProfileManager ProfileManager { get; }
		public PlayerProfileService(XBLMSAService xblmsaService, ProfileManager profileManager)
		{
			XblService = xblmsaService;
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
				Texture2D texture = null;
			
				if (profile.Properties.SkinUri != null)
				{
					SkinUtils.TryGetSkin(profile.Properties.SkinUri, Alex.Instance.GraphicsDevice, out texture);
				}


				//if (profile.Properties.SkinUri != null)
				//	{
				//	SkinUtils.TryGetSkin(profile.Properties.SkinUri, Alex.Instance.GraphicsDevice, out texture);
				//	}

				var playerProfile = new PlayerProfile(auth.SelectedProfile.Value, username, auth.SelectedProfile.PlayerName, new Skin(){Slim = skinSlim, Texture = texture},
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
			if (profile.IsBedrock)
			{
				try
				{
					//Validate Bedrock account.
					//BedrockTokenPair tokenPair = JsonConvert.DeserializeObject<BedrockTokenPair>(profile.ClientToken);
					BedrockTokenPair tokenPair = JsonConvert.DeserializeObject<BedrockTokenPair>(profile.ClientToken);
					if (tokenPair.ExpiryTime < DateTime.UtcNow && await XblService.TryAuthenticate(profile.AccessToken))
					{
						var p = new PlayerProfile(profile.Uuid, profile.Username, profile.PlayerName,
							profile.Skin, profile.AccessToken,
							profile.ClientToken,
							true);
								
						ProfileManager.CreateOrUpdateProfile(ProfileManager.ProfileType.Bedrock, p);

						CurrentProfile = p;
								
						Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(CurrentProfile));
						return true;
					}
					
					return await XblService.RefreshTokenAsync(tokenPair.RefreshToken).ContinueWith(task =>
						{
							if (task.IsFaulted)
							{
								Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Validation faulted!"));
								return false;
							}

							var r = task.Result;
							if (r.success)
							{
								var p = new PlayerProfile(profile.Uuid, profile.Username, profile.PlayerName,
									profile.Skin, r.token.AccessToken,
									JsonConvert.SerializeObject(r.token),
									true);
								
								ProfileManager.CreateOrUpdateProfile(ProfileManager.ProfileType.Bedrock, p);

								CurrentProfile = p;
								
								Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(CurrentProfile));
								return true;
							}
							else
							{
								Log.Warn($"Authentication unknown error.");
								
								Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Unknown error!"));
								return false;
							}
							
							return false;
						});
				}
				catch (Exception ex)
				{
					Log.Warn($"Failed to refresh bedrock access token: {ex.ToString()}");
				}

				return false;
			}

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

		public PlayerProfile[] GetJavaProfiles()
		{
			return ProfileManager.GetJavaProfiles();
		}

		public PlayerProfile[] GetBedrockProfiles()
		{
			return ProfileManager.GetBedrockProfiles();
		}
	}
}
