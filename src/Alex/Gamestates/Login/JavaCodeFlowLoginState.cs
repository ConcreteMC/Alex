using System;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Utils;
using Alex.Gamestates.Multiplayer;
using Alex.Gui;
using Alex.Utils;
using Alex.Utils.Auth;
using Alex.Worlds.Multiplayer.Java;
using NLog;
using PlayerProfile = Alex.Common.Services.PlayerProfile;

namespace Alex.Gamestates.Login
{
	public class JavaCodeFlowLoginState : CodeFlowLoginBase<MsaDeviceAuthConnectResponse>
	{
		private readonly ServerTypeImplementation _serverType;
		private readonly PlayerProfile _currentProfile;
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaCodeFlowLoginState));

		/// <inheritdoc />
		public JavaCodeFlowLoginState(ServerTypeImplementation serverType,
			GuiPanoramaSkyBox skyBox,
			UserSelectionState.ProfileSelected readyAction,
			PlayerProfile currentProfile = null) : base(skyBox, readyAction, serverType)
		{
			_serverType = serverType;
			_currentProfile = currentProfile;
		}

		/// <inheritdoc />
		protected override Task<MsaDeviceAuthConnectResponse> StartConnect()
		{
			return MojangApi.StartDeviceAuth();
		}

		/// <inheritdoc />
		protected override async Task<LoginResponse> ProcessLogin(CancellationToken cancellationToken)
		{
			try
			{
				var result = await MojangApi.DoDeviceCodeLogin(ConnectResponse, cancellationToken);

				if (result == null || !result.IsSuccess || result.Session == null)
				{
					Log.Warn(
						$"Login failed... (Result={result?.Result} ErrorMessage={result?.ErrorMessage} StatusCode={result?.StatusCode}) Error: {result?.Error}");

					return new LoginResponse(null, false, result?.ErrorMessage);
				}

				var session = result.Session;

				PlayerProfile playerProfile = _currentProfile ?? new PlayerProfile();
				playerProfile.UUID = session.UUID;
				playerProfile.Username = session.Username;
				playerProfile.AccessToken = session.AccessToken;
				playerProfile.ClientToken = session.ClientToken;
				playerProfile.PlayerName = result.Profile?.Name;
				playerProfile.RefreshToken = session.RefreshToken;
				playerProfile.ExpiryTime = session.ExpiryTime;

				playerProfile.Authenticated = true;
				playerProfile.Add(JavaServerType.AuthTypeIdentifier, true);

				return new LoginResponse(playerProfile, true);
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Unknown AUTH exception...");
			}

			return new LoginResponse(null, false);
		}
	}
}