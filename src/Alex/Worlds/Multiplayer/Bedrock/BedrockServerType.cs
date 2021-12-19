using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Alex.Common;
using Alex.Common.Data.Servers;
using Alex.Common.Services;
using Alex.Gamestates.Login;
using Alex.Gamestates.Multiplayer;
using Alex.Gui;
using Alex.Gui.Dialogs;
using Alex.Gui.Elements;
using Alex.Net;
using Alex.Services;
using Alex.Utils;
using Alex.Utils.Auth;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Extensions.DependencyInjection;
using MiNET.Net;
using MiNET.Utils;
using MojangAPI.Model;
using Newtonsoft.Json;
using NLog;
using PlayerProfile = Alex.Common.Services.PlayerProfile;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockServerType : ServerTypeImplementation<BedrockServerQueryProvider>
	{
		private static readonly Logger Log         = LogManager.GetCurrentClassLogger(typeof(BedrockServerType));
		
		private Alex Alex { get; }
		private XboxAuthService XboxAuthService { get; }

		/// <inheritdoc />
		public BedrockServerType(Alex game) : base(game.ServiceContainer, "Bedrock", "bedrock")
		{
			DefaultPort = 19132;
			Alex = game;
			ProtocolVersion = McpeProtocolInfo.ProtocolVersion;
			XboxAuthService = game.ServiceContainer.GetRequiredService<XboxAuthService>();

			SponsoredServers = new SavedServerEntry[]
			{
				new SavedServerEntry()
				{
					CachedIcon = ResourceManager.NethergamesLogo,
					Name = $"{ChatColors.Gold}NetherGames",
					Host = "play.nethergames.org",
					Port = 19132,
					ServerType = TypeIdentifier
				}
			};
		}

		/// <inheritdoc />
		public override bool TryGetWorldProvider(ServerConnectionDetails connectionDetails, PlayerProfile profile,
			out WorldProvider worldProvider,
			out NetworkProvider networkProvider)
		{
			var wp = new BedrockWorldProvider(Alex, connectionDetails.EndPoint,
				profile);
				
			wp.Init(out networkProvider);

			worldProvider = wp;
			return true;
		}

		private PlayerProfile Process(bool success, PlayerProfile profile)
		{
			profile.Authenticated = success;
			if (ProfileProvider is ProfileManager pm)
			{
				pm.CreateOrUpdateProfile(profile, true);
			}
			
			return profile;
		}

		private async Task<PlayerProfile> ReAuthenticate(PlayerProfile profile)
		{
			string deviceId = Guid.NewGuid().ToString();// profile.ClientToken ?? Alex.Resources.DeviceID;
			try
			{
				return await XboxAuthService.RefreshTokenAsync(profile.RefreshToken, deviceId).ContinueWith(
					task =>
					{
						if (task.IsFaulted)
						{
							Log.Error( task.Exception, $"Failed to refresh xbox token");
							profile.AuthError = task?.Exception?.Message ?? "Unknown error.";
							return profile;
						}

						var r = task.Result;

						if (r.token != null)
						{
							profile.AccessToken = r.token.AccessToken;
							profile.RefreshToken = r.token.RefreshToken;
							profile.ExpiryTime = r.token.ExpiryTime;
							profile.ClientToken = r.token.DeviceId;
						}

						return Process(r.success, profile);
					});
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Failed to refresh bedrock access token.");
			}

			return Process(false, profile);
		}

		/// <inheritdoc />
		public override Task Authenticate(GuiPanoramaSkyBox skyBox,  UserSelectionState.ProfileSelected callBack, PlayerProfile currentProfile)
		{
			BedrockLoginState loginState = new BedrockLoginState(
				skyBox, callBack, XboxAuthService, this, currentProfile);

			if (currentProfile != null)
			{
				loginState.LoginFailed(currentProfile.AuthError);
			}
			
			Alex.GameStateManager.SetActiveState(loginState, true);

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override async Task<PlayerProfile> VerifySession(PlayerProfile profile)
		{
			if (profile == null)
				return null;

			try
			{
				var deviceId = Guid.NewGuid().ToString();

				if (profile.ExpiryTime != null
				    && profile.ExpiryTime.GetValueOrDefault(DateTime.MinValue) > DateTime.UtcNow
				    && await XboxAuthService.TryAuthenticate(profile.AccessToken, deviceId))
				{
					profile.ClientToken = deviceId;

					return Process(true, profile);
				}
			}
			catch (Exception ex)
			{
				if (ex is not HttpRequestException requestException
				    || requestException.StatusCode != HttpStatusCode.BadRequest)
				{
					Log.Warn(ex, $"Failed authentication.");
				}
			}

			return await ReAuthenticate(profile);
		}

		private void OnCancel()
		{
			
		}

		/// <inheritdoc />
		public override Task<ProfileUpdateResult> UpdateProfile(PlayerProfile session)
		{
			if (ProfileProvider is ProfileManager pm)
			{
				pm.CreateOrUpdateProfile(session, true);
			}

			return Task.FromResult(new ProfileUpdateResult(true, null, null, session));
		}
	}
}