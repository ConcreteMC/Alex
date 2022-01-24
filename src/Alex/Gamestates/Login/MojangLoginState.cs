using System;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Multiplayer;
using Alex.Gui;
using Alex.Utils;
using Alex.Utils.Skins;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MojangAPI.Model;
using NLog;
using PlayerProfile = Alex.Common.Services.PlayerProfile;
using Skin = Alex.Common.Utils.Skin;

namespace Alex.Gamestates.Login
{
	public class MojangLoginState : BaseLoginState, ILoginState
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MojangLoginState));

		private readonly JavaServerType _serverType;
		private UserSelectionState.ProfileSelected _loginSuccesAction;
		private PlayerProfile _activeProfile;

		public MojangLoginState(JavaServerType serverType,
			GuiPanoramaSkyBox skyBox,
			UserSelectionState.ProfileSelected loginSuccesAction,
			PlayerProfile activeProfile = null) : base("Minecraft Login", skyBox)
		{
			_serverType = serverType;
			_loginSuccesAction = loginSuccesAction;
			_activeProfile = activeProfile;

			if (activeProfile != null)
			{
				NameInput.Value = activeProfile.Username;
			}
		}

		protected override void Initialized() { }

		protected override void LoginButtonPressed(string username, string password)
		{
			Task.Run(
				async () =>
				{
					Session session = null;

					try
					{
						if (_activeProfile != null)
							session = new Session()
							{
								Username = username,
								AccessToken = _activeProfile.AccessToken,
								ClientToken = _activeProfile.ClientToken,
								UUID = _activeProfile.UUID
							};

						session = await MojangApi.TryMojangLogin(username, password);
					}
					catch (LoginFailedException ex)
					{
						LoginFailed(
							new PlayerProfileAuthenticateEventArgs(ex.Response.ErrorMessage, ex.Response.Result));

						Log.Warn($"Error: {ex.Response.Result} - {ex.Response.ErrorMessage}");

						return false;
					}

					if (session == null || session.IsEmpty)
					{
						LoginFailed(
							new PlayerProfileAuthenticateEventArgs("Unknown error.", MojangAuthResult.UnknownError));

						return false;
					}

					session.Username = username;

					PlayerProfile playerProfile = _activeProfile ?? new PlayerProfile();
					playerProfile.UUID = session.UUID;
					playerProfile.Username = session.Username;
					playerProfile.AccessToken = session.AccessToken;
					playerProfile.ClientToken = session.ClientToken;
					playerProfile.Add(JavaServerType.AuthTypeIdentifier, false);

					var updateResult = await _serverType.UpdateProfile(playerProfile);

					if (!updateResult.Success)
					{
						LoginFailed(
							new PlayerProfileAuthenticateEventArgs(
								updateResult.ErrorMessage, MojangAuthResult.UnknownError));

						return false;
					}

					_activeProfile = playerProfile = updateResult.Profile;
					_loginSuccesAction?.Invoke(playerProfile);

					return true;
				});
		}

		public void LoginFailed(string error)
		{
			ErrorMessage.Text = error;
			ErrorMessage.TextColor = (Color)TextColor.Red;

			EnableInput();
		}

		private void LoginFailed(PlayerProfileAuthenticateEventArgs e)
		{
			LoginFailed($"Could not login: {e.ToUserFriendlyString()}");
			Log.Warn($"Failed to login to Mojang Account: {e.ToUserFriendlyString()}");
		}
	}
}