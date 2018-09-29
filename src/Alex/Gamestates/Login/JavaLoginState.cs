using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.GameStates.Gui.Common;
using Alex.Networking.Java;
using Microsoft.Xna.Framework;
using MojangSharp.Api;
using MojangSharp.Endpoints;
using MojangSharp.Responses;

namespace Alex.Gamestates.Login
{
	public class JavaLoginState : GuiMenuStateBase
	{
		private readonly GuiTextInput _nameInput;
		private readonly GuiTextInput _passwordInput;
		private readonly GuiButton _loginButton;
		private readonly GuiTextElement _errorMessage;
		public JavaLoginState()
		{
			Title = "Mojang Login";
			Body.ChildAnchor = Alignment.MiddleCenter;
			
			var usernameRow = AddGuiRow(new GuiTextElement()
			{
				Text = "Username:",
				Margin = new Thickness(0, 0, 5, 0)
			}, _nameInput = new GuiTextInput()
			{
				Width = 200,

				PlaceHolder = "Username...",
				Margin = new Thickness(5),
			});
			usernameRow.ChildAnchor = Alignment.MiddleCenter;

			var passwordRow = AddGuiRow(new GuiTextElement()
			{
				Text = "Password:",
				Margin = new Thickness(0, 0, 5, 0)
			}, _passwordInput = new GuiTextInput()
			{
				Width = 200,

				PlaceHolder = "Password...",
				Margin = new Thickness(5),
				IsPasswordInput = true
			});
			passwordRow.ChildAnchor = Alignment.MiddleCenter;

			var buttonRow = AddGuiRow(_loginButton = new GuiButton(OnLoginButtonPressed)
			{
				Text = "Login",
				Margin = new Thickness(5),
				Modern = false,
				Width = 100
			}, new GuiButton(OnCancelButtonPressed)
			{
				TranslationKey = "gui.cancel",
				Margin = new Thickness(5),
				Modern = false,
				Width = 100
			});
			buttonRow.ChildAnchor = Alignment.MiddleCenter;

			AddGuiElement(_errorMessage = new GuiTextElement()
			{
				TextColor = TextColor.Yellow
			});


			if (Alex.TryLoadJava(out var info))
			{
				Requester.ClientToken = info.ClientToken;

				_loginButton.Enabled = false;

				_nameInput.Value = info.RawUsername;

				_errorMessage.Text = "Authenticating...";
				Validate(info.AccessToken);
			}
		}

		private void OnLoginButtonPressed()
		{
			var auth =
				new Authenticate(new Credentials() {Username = _nameInput.Value, Password = _passwordInput.Value})
					.PerformRequestAsync().ContinueWith(JavaLoginResponse);

			_loginButton.Enabled = false;

			_errorMessage.Text = "Authenticating...";

			//auth.Start();
		}

		private void JavaLoginResponse(Task<AuthenticateResponse> obj)
		{
			var auth = obj.Result;
			if (auth.IsSuccess)
			{
				Alex.Username = auth.SelectedProfile.PlayerName;
				Alex.AccessToken = auth.AccessToken;
				Alex.UUID = auth.SelectedProfile.Value;
				Alex.ClientToken = auth.ClientToken;

				Validate(auth.AccessToken);
			}
			else
			{
				_errorMessage.Text = "Could not login: " + auth.Error.ErrorMessage;
				_errorMessage.TextColor	= TextColor.Red;

				_loginButton.Enabled = true;
			}
		}

		private void Validate(string accessToken)
		{
			Validate validate = new Validate(accessToken);
			validate.PerformRequestAsync().ContinueWith(task =>
			{
				var r = task.Result;
				if (r.IsSuccess)
				{
					Alex.SaveJava(_nameInput.Value);
					Alex.GameStateManager.SetActiveState<TitleState>();
				}
				else
				{
					_errorMessage.Text = "Could not login: " + r.Error.ErrorMessage;
					_errorMessage.TextColor = TextColor.Red;

					_loginButton.Enabled = true;
				}
			});
		}

		private void OnCancelButtonPressed()
		{
			Alex.GameStateManager.Back();
		}

		private void MenuButtonClicked()
		{
			Alex.GameStateManager.SetActiveState<TitleState>();
		}
	}
}
