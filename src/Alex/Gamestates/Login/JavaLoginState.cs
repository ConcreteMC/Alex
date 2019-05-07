using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.GameStates.Gui.Common;
using Alex.Gui;
using Alex.Networking.Java;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MojangSharp.Api;
using MojangSharp.Endpoints;
using MojangSharp.Responses;

namespace Alex.Gamestates.Login
{
	public class JavaLoginState : BaseLoginState
	{
		private IPlayerProfileService _playerProfileService;
		private Action _loginSuccesAction;
		public JavaLoginState(GuiPanoramaSkyBox skyBox, Action loginSuccesAction) : base("Minecraft Login", skyBox)
		{
			_loginSuccesAction = loginSuccesAction;
		}

		protected override void Initialized()
		{
			_playerProfileService = Alex.Services.GetService<IPlayerProfileService>();
			_playerProfileService.Authenticate += PlayerProfileServiceOnAuthenticate;

			var activeProfile = Alex.ProfileManager.LastUsedProfile;
			if (activeProfile != null && activeProfile.Type == ProfileManager.ProfileType.Java)
			{
				Requester.ClientToken = activeProfile.Profile.ClientToken;
				NameInput.Value = activeProfile.Profile.Username;
			}
		}

		private void PlayerProfileServiceOnAuthenticate(object sender, PlayerProfileAuthenticateEventArgs e)
		{
			if (e.IsSuccess)
			{
				Alex.ProfileManager.CreateOrUpdateProfile(ProfileManager.ProfileType.Java, e.Profile, true);
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
