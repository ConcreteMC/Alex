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
	public class BedrockServerType : ServerTypeImplementation
	{
		private const           string AccountType = "bedrock";
		
		private static readonly Logger Log         = LogManager.GetCurrentClassLogger(typeof(BedrockServerType));
		
		private Alex Alex { get; }
		private XboxAuthService XboxAuthService { get; }
		
		/// <inheritdoc />
		public override IListStorageProvider<SavedServerEntry> StorageProvider { get; }
		
		/// <inheritdoc />
		public BedrockServerType(Alex game) : base(new BedrockServerQueryProvider(game), "Bedrock", "bedrock")
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
			
			StorageProvider = new SavedServerDataProvider(Alex.Storage.Open("storage", "savedservers"), TypeIdentifier);
			MigrateServers();	
		}
		
		private void MigrateServers()
		{
			SavedServerDataProvider defaultDataProvider = new SavedServerDataProvider(Alex.Storage);
			defaultDataProvider.Load();

			foreach (var server in defaultDataProvider.Data.ToArray().Where(
				         x => x.ServerType.Equals(TypeIdentifier, StringComparison.InvariantCultureIgnoreCase)))
			{
				defaultDataProvider.RemoveEntry(server);
				StorageProvider.AddEntry(server);
			}
		}

		/// <inheritdoc />
		public override bool TryGetWorldProvider(ServerConnectionDetails connectionDetails, PlayerProfile profile,
			out WorldProvider worldProvider,
			out NetworkProvider networkProvider)
		{
			worldProvider = new BedrockWorldProvider(Alex, connectionDetails.EndPoint,
				profile, out networkProvider);
				
			return true;
		}

		private PlayerProfile Process(bool success, PlayerProfile profile)
		{
			if (success)
			{
				var profileManager = Alex.ServiceContainer.GetRequiredService<ProfileManager>();
				
				profile.Authenticated = true;
				profileManager.CreateOrUpdateProfile(AccountType, profile, true);

				return profile;
			}

			return profile;
		}

		private async Task<PlayerProfile> ReAuthenticate(PlayerProfile profile)
		{
			try
			{
				try
				{
					if (profile.ExpiryTime.GetValueOrDefault(DateTime.MaxValue) < DateTime.UtcNow
					    && await XboxAuthService.TryAuthenticate(profile.AccessToken))
					{
						return Process(true, profile);
					}
				}
				catch (HttpRequestException requestException)
				{
					if (requestException.StatusCode.GetValueOrDefault() != HttpStatusCode.Unauthorized)
					{
						throw;
					}
				}

				return await XboxAuthService.RefreshTokenAsync(profile.RefreshToken).ContinueWith(
					task =>
					{
						if (task.IsFaulted)
						{
							//Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Validation faulted!"));
							profile.AuthError = "Task faulted";
							return profile;
						}

						var r = task.Result;
						profile.AccessToken = r.token.AccessToken;
						profile.RefreshToken = r.token.RefreshToken;
						profile.ExpiryTime = r.token.ExpiryTime;
						
						return Process(r.success, profile);
					});
			}
			catch (Exception ex)
			{
				Log.Warn($"Failed to refresh bedrock access token: {ex.ToString()}");
			}

			return profile;
		}

		/// <inheritdoc />
		public override Task Authenticate(GuiPanoramaSkyBox skyBox,  AuthenticationCallback callBack)
		{
			var profileManager = Alex.ServiceContainer.GetRequiredService<ProfileManager>();
			UserSelectionState pss = new UserSelectionState(this, skyBox);
			pss.ReloadData(profileManager.GetProfiles(AccountType));
			
			pss.OnProfileSelection = async (p) =>
			{
				var overlay = new LoadingOverlay();
				Alex.GuiManager.AddScreen(overlay);

				try
				{
					if (!p.Authenticated)
					{
						p = await ReAuthenticate(p);
					}

					if (p.Authenticated)
					{
						callBack(p);
					}
					else
					{
						Log.Warn($"Bedrock authentication failed!");
						
						pss.ReloadData(profileManager.GetProfiles(AccountType));
						Alex.GameStateManager.SetActiveState(pss);
					}
				}
				finally
				{
					Alex.GuiManager.RemoveScreen(overlay);
				}
			};
			pss.OnCancel = () =>
			{
				
			};
			pss.OnAddAccount = () =>
			{
				BedrockLoginState loginState = new BedrockLoginState(
					skyBox, (p) => callBack(p), XboxAuthService, this);

				Alex.GameStateManager.SetActiveState(loginState, true);
			};
			Alex.GameStateManager.SetActiveState(pss);

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override Task<ProfileUpdateResult> UpdateProfile(PlayerProfile session)
		{
			var profileManager = Alex.ServiceContainer.GetRequiredService<ProfileManager>();
			profileManager.CreateOrUpdateProfile(AccountType, session, true);

			return Task.FromResult(new ProfileUpdateResult(true, null, null));
		}
	}
}