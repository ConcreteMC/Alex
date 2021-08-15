using System;
using System.Threading.Tasks;
using Alex.Common.Data.Servers;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Login;
using Alex.Gui;
using Alex.Net;
using Alex.Networking.Java;
using Alex.Services;
using Alex.Utils;
using Alex.Utils.Skins;
using Alex.Worlds.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using MojangAPI;
using MojangAPI.Model;
using NLog;
using PlayerProfile = Alex.Common.Services.PlayerProfile;
using Skin = MiNET.Utils.Skins.Skin;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaServerType : ServerTypeImplementation
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaServerType));
		private const string ProfileType = "java";
		
		private       Alex   Alex { get; }
		/// <inheritdoc />
		public JavaServerType(Alex alex) : base(new JavaServerQueryProvider(alex), "Java", "java")
		{
			Alex = alex;
			ProtocolVersion = JavaProtocol.ProtocolVersion;
			
			SponsoredServers = new SavedServerEntry[]
			{
				new SavedServerEntry()
				{
					Name = $"{ChatColors.Gold}Hypixel",
					Host = "mc.hypixel.net",
					Port = 25565,
					ServerType = TypeIdentifier
				},
				new SavedServerEntry()
				{
					Name = $"{ChatColors.Gold}Mineplex",
					Host = "eu.mineplex.com",
					Port = 25565,
					ServerType = TypeIdentifier
				}
			};
		}

		/// <inheritdoc />
		public override bool TryGetWorldProvider(ServerConnectionDetails connectionDetails,
			PlayerProfile playerProfile,
			out WorldProvider worldProvider,
			out NetworkProvider networkProvider)
		{
			worldProvider = new JavaWorldProvider(Alex, connectionDetails.EndPoint, playerProfile, out networkProvider)
			{
				Hostname = connectionDetails.Hostname
			};

			return true;
		}

		/// <inheritdoc />
		public override Task Authenticate(GuiPanoramaSkyBox skyBox, PlayerProfile activeProfile, Action<bool> callBack)
		{
			JavaLoginState loginState = new JavaLoginState(
				this,
				skyBox,
				() =>
				{
					callBack(true);
				}, activeProfile);


			Alex.GameStateManager.SetActiveState(loginState, true);
			
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override async Task<bool> VerifyAuthentication(PlayerProfile currentProfile)
		{
			var profileManager = Alex.Services.GetRequiredService<ProfileManager>();
			
			if (currentProfile != null)
			{
				if (await TryAuthenticate(currentProfile))
				{
					currentProfile.Authenticated= true;

					return true;
				}
			}
			else
			{
				foreach (var profile in profileManager.GetProfiles(ProfileType))
				{
					if (await TryAuthenticate(profile))
					{
						return true;
					}
				}
			}

			return false;
		}

		public async Task<ProfileUpdateResult> UpdateProfile(Session session)
		{
			var profileManager = Alex.Services.GetRequiredService<ProfileManager>();
			
			var profile = await MojangApi.GetPlayerProfile(session.AccessToken);

			if (profile.IsSuccess)
			{
				Texture2D texture = null;

				if (profile?.Skin?.Url != null)
				{
					SkinUtils.TryGetSkin(new Uri(profile?.Skin?.Url), Alex.Instance.GraphicsDevice, out texture);
				}

				var playerProfile = new PlayerProfile(
					profile.UUID, session.Username, profile.Name,
					new Common.Utils.Skin() { Slim = (profile.Skin?.Model == SkinType.Alex), Texture = texture },
					session.AccessToken, session.ClientToken)
				{
					Authenticated = true
				};

				profileManager.CreateOrUpdateProfile("java", playerProfile, true);
				//profileService.CurrentProfile = playerProfile;//.Force(playerProfile);
				return new ProfileUpdateResult(true, null, null);
			}

			return new ProfileUpdateResult(false, profile.Error, profile.ErrorMessage);
		}
		
		private async Task<bool> TryAuthenticate(PlayerProfile profile)
		{
		//	Requester.ClientToken = profile.ClientToken;
			var response = await MojangApi.Validate(profile.AccessToken, profile.ClientToken);
			if (response.IsSuccess)
			{
				Session session;
				
				if (response.Session != null)
				{
					session = response.Session;
				//	await UpdateProfile(response.Session);

				//	return true;
				}
				else
				{
					session = new Session()
					{
						Username = profile.Username,
						AccessToken = profile.AccessToken,
						ClientToken = profile.ClientToken,
						UUID = profile.Uuid
					};
				}

				var updateResult = await UpdateProfile(session);
				if (updateResult.Success)
				{
					return true;
				}
				
				Log.Warn($"Authentication failed. Result={response.Result} Error={updateResult.Error} Errormessage={response.ErrorMessage}");
			}
			else
			{
				Log.Warn(
					$"Authentication failed. Result={response.Result} Error={response.Error} ErrorMessage={response.ErrorMessage}");
			}

			return false;
		}
	}

	public class ProfileUpdateResult
	{
		public bool Success { get; }
		public string Error { get; }
		public string ErrorMessage { get; }
		
		public ProfileUpdateResult(bool success, string error, string profileErrorMessage)
		{
			Success = success;
			Error = error;
			ErrorMessage = profileErrorMessage;
		}
	}
}