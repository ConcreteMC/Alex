using System;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gui;
using MojangSharp.Api;

namespace Alex.Gamestates.Login
{
	public class JavaLoginState : BaseLoginState
	{
		private IPlayerProfileService _playerProfileService;
		private ProfileManager _profileManager;
		
		private Action _loginSuccesAction;
		private PlayerProfile _activeProfile;
		public JavaLoginState(GuiPanoramaSkyBox skyBox, Action loginSuccesAction, PlayerProfile activeProfile = null) : base("Minecraft Login", skyBox)
		{
			_loginSuccesAction = loginSuccesAction;
			_activeProfile = activeProfile;
		}

		protected override void Initialized()
		{
			_playerProfileService = GetService<IPlayerProfileService>();
			_playerProfileService.Authenticate += PlayerProfileServiceOnAuthenticate;

			_profileManager = GetService<ProfileManager>();
			var profiles = _profileManager.GetProfiles("java");

			if (profiles.Length == 1)
				_activeProfile = profiles[0];
			//var javaProfiles = _profileManager.GetProfiles("java");

			if (_activeProfile != null)
			{
				NameInput.Value = _activeProfile.Username;
			}
			else
			{
				var activeProfile = _profileManager.LastUsedProfile;

				if (activeProfile != null)
				{
					Requester.ClientToken = activeProfile.Profile.ClientToken;
					NameInput.Value = activeProfile.Profile.Username;
				}
			}
		}

		private void PlayerProfileServiceOnAuthenticate(object sender, PlayerProfileAuthenticateEventArgs e)
		{
			if (e.IsSuccess)
			{
				_profileManager.CreateOrUpdateProfile("java", e.Profile, true);
				_loginSuccesAction?.Invoke();
				//Alex.SaveJava(_nameInput.Value);
				//Alex.GameStateManager.SetActiveState("serverlist");
			}
			else
			{
				ErrorMessage.Text      = "Could not login: " + e.ErrorMessage;
				ErrorMessage.TextColor = TextColor.Red;

				EnableInput();
			}
		}

		protected override void LOginButtonPressed(string username, string password)
		{
			_playerProfileService.TryAuthenticateAsync(NameInput.Value, PasswordInput.Value);
		}
	}
}
