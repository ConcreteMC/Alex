using System;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gui;
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
	public class JavaLoginState : BaseLoginState
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaLoginState));
		private ProfileManager _profileManager;

		private readonly JavaServerType _serverType;
		private Action _loginSuccesAction;
		private PlayerProfile _activeProfile;
		public JavaLoginState(JavaServerType serverType, GuiPanoramaSkyBox skyBox, Action loginSuccesAction, PlayerProfile activeProfile = null) : base("Minecraft Login", skyBox)
		{
			_serverType = serverType;
			_loginSuccesAction = loginSuccesAction;
			_activeProfile = activeProfile;
			//AddChild(new AlexButton("XBOX", DoXboxLogin));
		}

		private void DoXboxLogin()
		{
			MojangApi.StartDeviceAuth().ContinueWith(
				async task =>
				{
					var r = task.Result;
					LoginFailed(r.user_code);

					try
					{
						var result = await MojangApi.DoDeviceCodeLogin(r, CancellationToken.None);

						if (result != null)
						{
							if (result.IsSuccess)
							{
								Log.Info($"Login ok! :)");
							}

							else
							{
								Log.Warn($"Login failed: {result.Error} (Result={result.Result})");
							}
						}
						else
						{
							Log.Warn($"Auth response = null");
						}
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Auth failed.");
					}
				});
		}

		protected override void Initialized()
		{
			_profileManager = GetService<ProfileManager>();
			var profiles = _profileManager.GetProfiles("java");

			if (profiles.Length == 1)
				_activeProfile = profiles[0];

			if (_activeProfile != null)
			{
				NameInput.Value = _activeProfile.Username;
			}
			else
			{
				var activeProfile = _profileManager.LastUsedProfile;

				if (activeProfile != null)
				{
					NameInput.Value = activeProfile.Profile.Username;
				}
			}
		}

		protected override void LoginButtonPressed(string username, string password)
		{
			TryAuthenticateAsync(NameInput.Value, PasswordInput.Value);
			//	_playerProfileService.TryAuthenticateAsync(NameInput.Value, PasswordInput.Value);
		}

		private void LoginFailed(string error)
		{
			ErrorMessage.Text      = "Could not login: " + error;
			ErrorMessage.TextColor = (Color) TextColor.Red;

			EnableInput();
		}

		private void LoginFailed(PlayerProfileAuthenticateEventArgs e)
		{
			LoginFailed(e.ToUserFriendlyString());
		}
		
		private async Task<bool> TryAuthenticateAsync(string username, string password)
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
						UUID = _activeProfile.Uuid
					};
				
				session = await MojangApi.TryMojangLogin(username, password, session);
			}
			catch (LoginFailedException ex)
			{
				LoginFailed(new PlayerProfileAuthenticateEventArgs(ex.Response.ErrorMessage, ex.Response.Result));
				
				Log.Warn($"Error: {ex.Response.Result} - {ex.Response.ErrorMessage}");
				return false;
			}

			if (session == null || session.IsEmpty)
			{
				LoginFailed(new PlayerProfileAuthenticateEventArgs("Unknown error.", MojangAuthResult.UnknownError));
				return false;
			}

			session.Username = username;
			var updateResult = await _serverType.UpdateProfile(session);
			if (updateResult.Success)
			{
				_loginSuccesAction?.Invoke();

				return true;
			}

			LoginFailed(new PlayerProfileAuthenticateEventArgs(updateResult.ErrorMessage, MojangAuthResult.UnknownError));
			return false;
		}
	}
}
