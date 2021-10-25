using System;
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
	public class JavaServerType : ServerTypeImplementation
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaServerType));
		private const string ProfileType = "java";
		
		private       Alex   Alex { get; }
		
		/// <inheritdoc />
		public override IListStorageProvider<SavedServerEntry> StorageProvider { get; }
		
		/// <inheritdoc />
		public JavaServerType(Alex alex) : base(new JavaServerQueryProvider(alex), "Java", "java")
		{
			Alex = alex;
			ProtocolVersion = JavaProtocol.ProtocolVersion;
			
			SponsoredServers = new SavedServerEntry[]
			{
				new ()
				{
					Name = $"{ChatColors.Gold}Hypixel",
					Host = "mc.hypixel.net",
					Port = 25565,
					ServerType = TypeIdentifier
				},
				new ()
				{
					Name = $"{ChatColors.Gold}Mineplex",
					Host = "eu.mineplex.com",
					Port = 25565,
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
		private async Task<bool> IsAuthenticated(PlayerProfile profile)
		{
			if (profile == null)
				return false;
			
			if (!profile.Authenticated)
			{
				if (await TryAuthenticate(profile))
				{
					profile.Authenticated = true;
					return true;
				}
			}

			return profile.Authenticated;
		}
		
		/// <inheritdoc />
		public override Task Authenticate(GuiPanoramaSkyBox skyBox, AuthenticationCallback callBack)
		{
			var profileManager = Alex.Services.GetRequiredService<ProfileManager>();
			UserSelectionState pss = new UserSelectionState(this, skyBox);
			pss.ReloadData(profileManager.GetProfiles(ProfileType));
			
			async void LoginCallBack(PlayerProfile p)
			{
				var overlay = new LoadingOverlay();
				Alex.GuiManager.AddScreen(overlay);

				try
				{
					if (await IsAuthenticated(p))
					{
						callBack?.Invoke(p);
					}
					else
					{
						Log.Warn($"Java authentication failed...");

						if (p.TryGet(AuthTypeIdentifier, out bool isMicrosoftAccount))
						{
							if (isMicrosoftAccount)
							{
								Alex.GameStateManager.SetActiveState(
									new JavaCodeFlowLoginState(this, skyBox, c =>
									{
										callBack?.Invoke(c);
									}, p));
							}
							else
							{
								Alex.GameStateManager.SetActiveState(
									new MojangLoginState(
										this, skyBox, c =>
										{
											callBack?.Invoke(c);
										}, p));
							}
						}
						pss.ReloadData(profileManager.GetProfiles(ProfileType));
						Alex.GameStateManager.SetActiveState(pss);
					}
				}
				finally
				{
					Alex.GuiManager.RemoveScreen(overlay);
				}
			}
			
			void UseMicrosoft()
			{
				Alex.GameStateManager.SetActiveState(
					new JavaCodeFlowLoginState(this, skyBox, LoginCallBack));
			}

			void UseMojang()
			{
				Alex.GameStateManager.SetActiveState(
					new MojangLoginState(
						this, skyBox, LoginCallBack));
			}

			pss.OnProfileSelection = LoginCallBack;
			pss.OnCancel = () =>
			{
				
			};
			pss.OnAddAccount = () =>
			{
				Alex.GameStateManager.SetActiveState(new JavaProviderSelectionState(
					this,
					skyBox,
					UseMicrosoft, UseMojang), true);
			};
			Alex.GameStateManager.SetActiveState(pss);

			return Task.CompletedTask;
		}

		public override async Task<ProfileUpdateResult> UpdateProfile(PlayerProfile profile)
		{
			var profileManager = Alex.Services.GetRequiredService<ProfileManager>();

			var mojangProfile = await MojangApi.GetPlayerProfile(profile.AccessToken);

			if (!mojangProfile.IsSuccess)
			{
				return new ProfileUpdateResult(false, mojangProfile.Error, mojangProfile.ErrorMessage);
			}

			profile.UUID = mojangProfile.UUID;
			profile.Authenticated = true;
			
			profileManager.CreateOrUpdateProfile(ProfileType, profile, true);
			return new ProfileUpdateResult(true, profile);
		}

		private async Task<bool> TryAuthenticate(PlayerProfile profile)
		{
			MojangAuthResponse response = await MojangApi.Validate(profile.AccessToken, profile.ClientToken);

			if (!response.IsSuccess)
			{
				if (string.IsNullOrWhiteSpace(profile.RefreshToken))
				{
					Log.Warn($"Could not validate accesstoken: {profile.Username} (Error={response.ErrorMessage} Result={response.Result})");
					profile.AuthError = new PlayerProfileAuthenticateEventArgs(response.ErrorMessage, response.Result).ToUserFriendlyString();
					return false;
				}

				response = await MojangApi.RefreshSession(profile);
				if (!response.IsSuccess)
				{
					Log.Warn($"Could not refresh session for user: {profile.Username} (Error={response.ErrorMessage} Result={response.Result})");
					profile.AuthError = new PlayerProfileAuthenticateEventArgs(response.ErrorMessage, response.Result).ToUserFriendlyString();
						
					return false;
				}
			}

			if (response.Session != null)
			{
				profile.Username = response.Session.Username;
				profile.UUID = response.Session.UUID;
				profile.ClientToken = response.Session.ClientToken;
				profile.AccessToken = response.Session.AccessToken;
				profile.RefreshToken = response.Session.RefreshToken;
				profile.ExpiryTime = response.Session.ExpiryTime;
			}

			if (response.Profile != null)
			{
				profile.PlayerName = response.Profile?.Name;

				if (response.Profile.Skin != null)
				{
					profile.Add("skin", response.Profile.Skin);
				}
			}

			var updateResult = await UpdateProfile(profile);

			if (updateResult.Success)
			{
				updateResult.Profile.Authenticated = true;
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