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
using Alex.Worlds.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using MiNET.Utils;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaServerType : ServerTypeImplementation
	{
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
			var authenticationService = Alex.Services.GetService<IPlayerProfileService>();
			
			if (currentProfile != null)
			{
				if (await TryAuthenticate(authenticationService, currentProfile))
				{
					currentProfile.Authenticated= true;

					return true;
				}
			}
			else
			{
				foreach (var profile in authenticationService.GetProfiles(ProfileType))
				{
					if (await TryAuthenticate(authenticationService, profile))
					{
						return true;
					}
				}
			}

			return false;
		}

		private async Task<bool> TryAuthenticate(IPlayerProfileService authenticationService, PlayerProfile profile)
		{
		//	Requester.ClientToken = profile.ClientToken;

			if (await Validate(profile.AccessToken))
			{
				profile.Authenticated = true;
				authenticationService.Force(profile);

				//CurrentProfile = profile;
				return true;
			}

			return false;
		}
		
		private async Task<bool> Validate(string accessToken)
		{
			return await MojangApi.CheckGameOwnership(accessToken);
			
		}
	}
}