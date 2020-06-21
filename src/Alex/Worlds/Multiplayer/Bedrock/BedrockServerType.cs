using System;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.Gamestates.Login;
using Alex.Gui;
using Alex.Net;
using Alex.Services;
using Alex.Utils;
using Alex.Worlds.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using MiNET.Net;
using MiNET.Utils;
using Newtonsoft.Json;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockServerType : ServerTypeImplementation
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockServerType));
		
		private Alex Alex { get; }
		private XboxAuthService XboxAuthService { get; }
		/// <inheritdoc />
		public BedrockServerType(Alex game, XboxAuthService xboxAuthService) : base(new BedrockServerQueryProvider(game), "Bedrock")
		{
			DefaultPort = 19132;
			Alex = game;
			ProtocolVersion = McpeProtocolInfo.ProtocolVersion;
			XboxAuthService = xboxAuthService;
		}

		/// <inheritdoc />
		public override bool TryGetWorldProvider(ServerConnectionDetails connectionDetails, PlayerProfile profile,
			out WorldProvider worldProvider,
			out NetworkProvider networkProvider)
		{
			worldProvider = new BedrockWorldProvider(Alex, connectionDetails.EndPoint,
				profile, new DedicatedThreadPool(new DedicatedThreadPoolSettings(2)), out networkProvider);
				
			return true;
		}

		private async Task<PlayerProfile> ReAuthenticate(PlayerProfile profile)
		{
			var profileManager = Alex.Services.GetService<ProfileManager>();
			var profileService = Alex.Services.GetService<IPlayerProfileService>();
			try
				{
					//Validate Bedrock account.
					//BedrockTokenPair tokenPair = JsonConvert.DeserializeObject<BedrockTokenPair>(profile.ClientToken);
					BedrockTokenPair tokenPair = JsonConvert.DeserializeObject<BedrockTokenPair>(profile.ClientToken);
					if (tokenPair.ExpiryTime < DateTime.UtcNow && await XboxAuthService.TryAuthenticate(profile.AccessToken))
					{
						var p = new PlayerProfile(profile.Uuid, profile.Username, profile.PlayerName,
							profile.Skin, profile.AccessToken,
							profile.ClientToken,
							"xbox");

						p.Authenticated = true;
								
						profileManager.CreateOrUpdateProfile("xbox", p);

						profileService.Force(p);// = p;
								
						//Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(CurrentProfile));
						return p;
					}
					
					return await XboxAuthService.RefreshTokenAsync(tokenPair.RefreshToken).ContinueWith(task =>
						{
							if (task.IsFaulted)
							{
								//Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Validation faulted!"));
								return profile;
							}

							var r = task.Result;
							if (r.success)
							{
								var p = new PlayerProfile(profile.Uuid, profile.Username, profile.PlayerName,
									profile.Skin, r.token.AccessToken,
									JsonConvert.SerializeObject(r.token),
									"xbox");

								p.Authenticated = true;
								
								profileManager.CreateOrUpdateProfile("xbox", p);

								profileService.Force(p);
								
							//	Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(CurrentProfile));
								return p;
							}
							else
							{
								Log.Warn($"Authentication unknown error.");
								
							//	Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Unknown error!"));
								return profile;
							}
							
							return profile;
						});
				}
				catch (Exception ex)
				{
					Log.Warn($"Failed to refresh bedrock access token: {ex.ToString()}");
				}

				return profile;
		}
		
		/// <inheritdoc />
		public override async Task<bool> VerifyAuthentication(PlayerProfile currentProfile)
		{
			if (currentProfile == null || (currentProfile.Type != "xbox")  || !currentProfile.Authenticated)
			{
				var authenticationService = Alex.Services.GetService<IPlayerProfileService>();
				foreach (var profile in authenticationService.GetProfiles("xbox"))
				{
					profile.Type = "xbox";

					var task = await ReAuthenticate(profile);

					if (task.Authenticated)
					{
						currentProfile = profile;

						return true;
					}
				}
			}

			if ((currentProfile == null || (currentProfile.Type != "xbox")) || !currentProfile.Authenticated)
			{
				return false;
			}
			else
			{
				return true;
			}
			//return base.VerifyAuthentication(profile);
		}

		/// <inheritdoc />
		public override async Task Authenticate(GuiPanoramaSkyBox skyBox, Action<bool> callBack)
		{
			BedrockLoginState loginState = new BedrockLoginState(
				skyBox, (profile) =>
				{
					callBack(true);
				}, XboxAuthService);

			Alex.GameStateManager.SetActiveState(loginState, true);
		}
	}
}