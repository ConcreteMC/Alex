using System;
using System.Threading.Tasks;
using Alex.Common;
using Alex.Common.Data.Servers;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Login;
using Alex.Gui;
using Alex.Net;
using Alex.Networking.Java;
using Alex.Services;
using Alex.Utils;
using Alex.Utils.Auth;
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

		public const string AuthTypeIdentifier = "MicrosoftAccount";
		/// <inheritdoc />
		public override Task Authenticate(GuiPanoramaSkyBox skyBox, PlayerProfile activeProfile, AuthenticationCallback callBack)
		{
			void UseMicrosoft()
			{
				Alex.GameStateManager.SetActiveState(
					new JavaCodeFlowLoginState(this, skyBox, () =>
					{
						callBack();
					}, activeProfile));
			}

			void UseMojang()
			{
				Alex.GameStateManager.SetActiveState(
					new MojangLoginState(
						this, skyBox, () => callBack(), activeProfile));
			}
			
			if (activeProfile != null && activeProfile.TryGet(AuthTypeIdentifier, out bool isMicrosoftAccount))
			{
				if (isMicrosoftAccount)
				{
					UseMicrosoft();
				}
				else
				{
					UseMojang();
				}

				return Task.CompletedTask;
			}

			Alex.GameStateManager.SetActiveState(new JavaProviderSelectionState(
				this,
				skyBox,
				UseMicrosoft, UseMojang), true);
			
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

		public override async Task<ProfileUpdateResult> UpdateProfile(PlayerProfile session)
		{
			var profileManager = Alex.Services.GetRequiredService<ProfileManager>();

			var profile = await MojangApi.GetPlayerProfile(session.AccessToken);

			if (!profile.IsSuccess)
			{
				return new ProfileUpdateResult(false, profile.Error, profile.ErrorMessage);
			}

			Common.Utils.Skin skin = session?.Skin;
			if (profile?.Skin?.Url != null)
			{
				Texture2D texture = null;

				if (SkinUtils.TryGetSkin(new Uri(profile?.Skin?.Url), Alex.Instance.GraphicsDevice, out texture))
				{
					skin = new Common.Utils.Skin()
					{
						Slim = (profile.Skin?.Model == SkinType.Alex),
						Texture = texture,
						Url = profile?.Skin?.Url
					};
				}
			}
			
			PlayerProfile playerProfile = session ?? new PlayerProfile();
			playerProfile.UUID = profile.UUID;
			playerProfile.Username = session.Username;
			playerProfile.AccessToken = session.AccessToken;
			playerProfile.ClientToken = session.ClientToken;
			playerProfile.PlayerName = profile.Name;
			playerProfile.RefreshToken = session.RefreshToken;
			playerProfile.Skin = skin;
			playerProfile.Authenticated = true;
			
			profileManager.CreateOrUpdateProfile("java", playerProfile, true);
			return new ProfileUpdateResult(true, playerProfile);
		}

		private async Task<bool> TryAuthenticate(PlayerProfile profile)
		{
			//	Requester.ClientToken = profile.ClientToken;
			var response = await MojangApi.TryAutoLogin(profile);

			if (!response.IsSuccess)
			{
				Log.Warn($"Could not auto-login user: {profile.Username} (Error={response.ErrorMessage} Result={response.Result})");
				response = await MojangApi.Validate(profile.AccessToken, profile.ClientToken);
			}

			if (!response.IsSuccess)
			{
				Log.Warn($"Could not restore user session: {profile.Username} (Error={response.ErrorMessage} Result={response.Result})");

				return false;
			}

			if (response.Session != null)
			{
				profile.Username = response.Session.Username;
				profile.UUID = response.Session.UUID;
				profile.ClientToken = response.Session.ClientToken;
				profile.AccessToken = response.Session.AccessToken;
			}

			if (response.Profile != null)
			{
				profile.PlayerName = response.Profile?.Name;

				if (response.Profile.Skin != null)
				{
					profile.Skin = new Common.Utils.Skin()
					{
						Texture = profile?.Skin?.Texture,
						Url = response.Profile.Skin.Url,
						Slim = response.Profile.Skin.Model == SkinType.Alex
					};
				}
			}

			var updateResult = await UpdateProfile(profile);

			if (updateResult.Success)
			{
				return true;
			}

			Log.Warn(
				$"Authentication failed (Could not get profile). Result={response.Result} Error={updateResult.Error} Errormessage={response.ErrorMessage}");
			return false;
		}
	}

	public class ProfileUpdateResult
	{
		public bool Success { get; }
		public string Error { get; }
		public string ErrorMessage { get; }
		
		public PlayerProfile Profile { get; }
		public ProfileUpdateResult(bool success, string error, string profileErrorMessage, PlayerProfile profile = null)
		{
			Success = success;
			Error = error;
			ErrorMessage = profileErrorMessage;
			Profile = profile;
		}

		public ProfileUpdateResult(bool success, PlayerProfile profile)
		{
			Success = success;
			Profile = profile;
		}
	}
}