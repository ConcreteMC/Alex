using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Alex.Common;
using Alex.Common.Data.Servers;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Login;
using Alex.Gamestates.Multiplayer;
using Alex.Gui;
using Alex.Gui.Dialogs;
using Alex.Gui.Elements;
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
using MojangAuthResponse = Alex.Utils.Auth.MojangAuthResponse;
using PlayerProfile = Alex.Common.Services.PlayerProfile;
using Skin = MiNET.Utils.Skins.Skin;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaServerType : ServerTypeImplementation<JavaServerQueryProvider>
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaServerType));
		private Alex Alex { get; }

		/// <inheritdoc />
		public JavaServerType(Alex alex) : base(alex.ServiceContainer, "Java", "java")
		{
			Alex = alex;
			ProtocolVersion = JavaProtocol.ProtocolVersion;

			SponsoredServers = new SavedServerEntry[]
			{
				new()
				{
					Name = $"{ChatColors.Gold}Hypixel",
					Host = "mc.hypixel.net",
					Port = 25565,
					ServerType = TypeIdentifier
				},
				new()
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
		public override Task Authenticate(GuiPanoramaSkyBox skyBox,
			UserSelectionState.ProfileSelected callBack,
			PlayerProfile profile)
		{
			if (profile is not null)
			{
				ILoginState loginState;

				if (profile.TryGet(AuthTypeIdentifier, out bool isMicrosoftAccount) && isMicrosoftAccount)
				{
					loginState = new JavaCodeFlowLoginState(this, skyBox, callBack, profile);
				}
				else
				{
					loginState = new MojangLoginState(this, skyBox, callBack, profile);
				}

				loginState.LoginFailed(profile.AuthError);
				Alex.GameStateManager.SetActiveState(loginState);

				return Task.CompletedTask;
			}

			JavaProviderSelectionState providerSelectionState = new JavaProviderSelectionState(
				this, skyBox, () => new JavaCodeFlowLoginState(this, skyBox, callBack),
				() => new MojangLoginState(this, skyBox, callBack));

			Alex.GameStateManager.SetActiveState(providerSelectionState);

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override async Task<PlayerProfile> VerifySession(PlayerProfile profile)
		{
			var updateResult = await UpdateProfile(profile);

			if (updateResult.Success)
			{
				profile = updateResult.Profile;
				profile.Authenticated = true;

				return profile;
			}

			MojangAuthResponse response; // = await MojangApi.Validate(profile.AccessToken, profile.ClientToken);

			bool isMicrosoftAccount = profile.TryGet(AuthTypeIdentifier, out bool isMSA) && isMSA;

			if (isMicrosoftAccount)
			{
				if (string.IsNullOrWhiteSpace(profile.RefreshToken))
				{
					Log.Warn(
						$"Refresh token was empty!");

					profile.AuthError = "Session expired.";
					profile.Authenticated = false;
					return profile;
				}

				response = await MojangApi.RefreshXboxSession(profile);
			}
			else
			{
				response = await MojangApi.RefreshMojangSession(profile);
			}


			if (!response.IsSuccess)
			{
				Log.Warn(
					$"Could not refresh session for user: {profile.Username} (ErrorMessage={response.ErrorMessage} Error={response.Error} Result={response.Result} IsMSA={isMicrosoftAccount})");

				profile.AuthError = new PlayerProfileAuthenticateEventArgs(response.ErrorMessage, response.Result)
				   .ToUserFriendlyString();

				profile.Authenticated = false;

				return profile;
			}

			profile.Authenticated = response.IsSuccess;
			if (response.Session != null)
			{
				profile.Username = response.Session.Username;
				profile.UUID = response.Session.UUID;
				profile.ClientToken = response.Session.ClientToken;
				profile.AccessToken = response.Session.AccessToken;
				profile.RefreshToken = response.Session.RefreshToken;
				profile.ExpiryTime = response.Session.ExpiryTime;
			}

			return profile;
		}

		public override async Task<ProfileUpdateResult> UpdateProfile(PlayerProfile profile)
		{
			try
			{
				var mojangProfile = await MojangApi.GetPlayerProfile(profile.AccessToken);

				if (!mojangProfile.IsSuccess)
				{
					return new ProfileUpdateResult(false, mojangProfile.Error, mojangProfile.ErrorMessage);
				}


				profile.PlayerName = mojangProfile?.Name ?? profile.PlayerName;

				if (mojangProfile.Skin != null)
				{
					profile.Add("skin", mojangProfile.Skin);
				}

				profile.UUID = mojangProfile.UUID;
				profile.Authenticated = true;

				if (ProfileProvider is ProfileManager pm)
				{
					pm.CreateOrUpdateProfile(profile, true);
				}

				return new ProfileUpdateResult(true, profile);
			}
			catch (Exception ex)
			{
				return new ProfileUpdateResult(false, ex.ToString(), ex.Message);
			}
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