using System;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.Gamestates.Login;
using Alex.Gui;
using Alex.Net;
using Alex.Networking.Java;
using Alex.Services;
using Alex.Utils;
using Alex.Worlds.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using MiNET.Utils;
using MojangSharp.Api;
using MojangSharp.Endpoints;
using DedicatedThreadPool = Alex.API.Utils.DedicatedThreadPool;
using DedicatedThreadPoolSettings = Alex.API.Utils.DedicatedThreadPoolSettings;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaServerType : ServerTypeImplementation
	{
		private Alex Alex { get; }
		/// <inheritdoc />
		public JavaServerType(Alex alex) : base(new JavaServerQueryProvider(alex), "Java")
		{
			Alex = alex;
			ProtocolVersion = JavaProtocol.ProtocolVersion;
		}

		/// <inheritdoc />
		public override bool TryGetWorldProvider(ServerConnectionDetails connectionDetails,
			PlayerProfile playerProfile,
			out WorldProvider worldProvider,
			out NetworkProvider networkProvider)
		{
			worldProvider = new JavaWorldProvider(Alex, connectionDetails.EndPoint, playerProfile, new DedicatedThreadPool(new DedicatedThreadPoolSettings(2)), out networkProvider)
			{
				Hostname = connectionDetails.Hostname
			};

			return true;
		}

		/// <inheritdoc />
		public override Task Authenticate(GuiPanoramaSkyBox skyBox, Action<bool> callBack)
		{
			JavaLoginState loginState = new JavaLoginState(
				skyBox,
				() =>
				{
					callBack(true);
				});


			Alex.GameStateManager.SetActiveState(loginState, true);
			
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public override async Task<bool> VerifyAuthentication(PlayerProfile currentProfile)
		{
			if (currentProfile == null || (currentProfile.Type != "java")  || !currentProfile.Authenticated)
			{
				var authenticationService = Alex.Services.GetService<IPlayerProfileService>();
				foreach (var profile in authenticationService.GetProfiles("java"))
				{
					profile.Type = "java";

					Requester.ClientToken = profile.ClientToken;

					if (await Validate(profile.AccessToken))
					{
						profile.Authenticated = true;
						authenticationService.Force(profile);

						//CurrentProfile = profile;
						return true;
					}
				}
			}
		
			return false;
		}
		
		private async Task<bool> Validate(string accessToken)
		{
			return await new Validate(accessToken)
			   .PerformRequestAsync()
			   .ContinueWith(task =>
				{
					if (task.IsFaulted)
					{
						//Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs("Validation faulted!"));
						return false;
					}

					var r = task.Result;
					if (r.IsSuccess)
					{
					//	Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(CurrentProfile));
						//Alex.Instance.GameStateManager.SetActiveState<TitleState>();
						return true;
					}
					else
					{
					//	Authenticate?.Invoke(this, new PlayerProfileAuthenticateEventArgs(r.Error.ErrorMessage));
						return false;
					}
				});
		}
	}
}