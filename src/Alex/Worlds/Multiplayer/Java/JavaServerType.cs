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
		private       Alex   Alex { get; }
		/// <inheritdoc />
		public JavaServerType(Alex alex) : base(alex.ServiceContainer, "Java", "java")
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
				else
				{
					profile.Authenticated = false;
				}
			}

			return profile.Authenticated;
		}
		
		/// <inheritdoc />
		public override Task Authenticate(GuiPanoramaSkyBox skyBox, AuthenticationCallback callBack)
		{
			UserSelectionState pss = new UserSelectionState(this, skyBox);
			
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
						Log.Warn($"User session was not valid: {p.AuthError}");
						
						if (p.TryGet(AuthTypeIdentifier, out bool isMicrosoftAccount) && isMicrosoftAccount)
						{
							var codeFlow = new JavaCodeFlowLoginState(
								this, skyBox, LoginCallBack, p);
							codeFlow.SetSubText(p.AuthError);
								
							Alex.GameStateManager.SetActiveState(codeFlow);

							return;
						}
						
						var javaLoginState = new MojangLoginState(
							this, skyBox, LoginCallBack, p);
						javaLoginState.LoginFailed(p.AuthError);
						Alex.GameStateManager.SetActiveState(javaLoginState);
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

		private async Task<bool> TryAuthenticate(PlayerProfile profile)
		{
			MojangAuthResponse response = await MojangApi.Validate(profile.AccessToken, profile.ClientToken);

			if (!response.IsSuccess)
			{
				bool isMicrosoftAccount = profile.TryGet(AuthTypeIdentifier, out bool isMSA) && isMSA;

				if (isMicrosoftAccount)
				{
					if (string.IsNullOrWhiteSpace(profile.RefreshToken))
					{
						Log.Warn($"Could not validate accesstoken: {profile.Username} (Error={response.ErrorMessage} Result={response.Result})");
						profile.AuthError = new PlayerProfileAuthenticateEventArgs(response.ErrorMessage, response.Result).ToUserFriendlyString();
						return false;
					}
					
					response = await MojangApi.RefreshXboxSession(profile);
				}
				else
				{
					response = await MojangApi.RefreshMojangSession(profile);
				}

			
				if (!response.IsSuccess)
				{
					Log.Warn($"Could not refresh session for user: {profile.Username} (Error={response.ErrorMessage} Result={response.Result} IsMSA={isMicrosoftAccount})");
					profile.AuthError = new PlayerProfileAuthenticateEventArgs(response.ErrorMessage, response.Result).ToUserFriendlyString();
					
					return false;
				}
				else
				{
					Log.Info($"Success!");
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