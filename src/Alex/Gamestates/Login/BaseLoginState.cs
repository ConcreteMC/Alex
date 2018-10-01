using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.GameStates.Gui.Common;
using Microsoft.Xna.Framework.Input;
using MojangSharp.Api;
using MojangSharp.Endpoints;

namespace Alex.Gamestates.Login
{
	public abstract class BaseLoginState : GuiMenuStateBase
	{
		protected readonly GuiTextInput NameInput;
		protected readonly GuiTextInput PasswordInput;
		protected readonly GuiButton LoginButton;
		protected readonly GuiTextElement ErrorMessage;

		protected BaseLoginState(string title)
		{
			Title = title;
			base.HeaderTitle.Anchor = Alignment.MiddleCenter;
			Header.AddChild(new GuiTextElement()
			{
				Text = "We are NOT in anyway or form affiliated with Mojang/Minecraft or Microsoft!",
				TextColor = TextColor.Yellow,
				Scale = 1f,
				FontStyle = FontStyle.DropShadow,

				Anchor = Alignment.BottomCenter
			});

			Body.ChildAnchor = Alignment.MiddleCenter;

			var usernameRow = AddGuiRow(new GuiTextElement()
			{
				Text = "Username:",
				Margin = new Thickness(0, 0, 5, 0)
			}, NameInput = new GuiTextInput()
			{
				TabIndex = 1,

				Width = 200,

				PlaceHolder = "Username...",
				Margin = new Thickness(5),
			});
			usernameRow.ChildAnchor = Alignment.MiddleCenter;

			var passwordRow = AddGuiRow(new GuiTextElement()
			{
				Text = "Password:",
				Margin = new Thickness(0, 0, 5, 0)
			}, PasswordInput = new GuiTextInput()
			{
				TabIndex = 2,

				Width = 200,

				PlaceHolder = "Password...",
				Margin = new Thickness(5),
				IsPasswordInput = true
			});
			passwordRow.ChildAnchor = Alignment.MiddleCenter;

			var buttonRow = AddGuiRow(LoginButton = new GuiButton(OnLoginButtonPressed)
			{
				AccessKey = Keys.Enter,

				Text = "Login",
				Margin = new Thickness(5),
				Modern = false,
				Width = 100
			}, new GuiButton(OnCancelButtonPressed)
			{
				AccessKey = Keys.Escape,

				TranslationKey = "gui.cancel",
				Margin = new Thickness(5),
				Modern = false,
				Width = 100
			});
			buttonRow.ChildAnchor = Alignment.MiddleCenter;

			AddGuiElement(ErrorMessage = new GuiTextElement()
			{
				TextColor = TextColor.Yellow
			});

			Initialized();
		}

		protected abstract void Initialized();

		private void OnLoginButtonPressed()
		{
			LoginButton.Enabled = false;
			ErrorMessage.Text = "Authenticating...";

			LOginButtonPressed(NameInput.Value, PasswordInput.Value);
		}

		protected abstract void LOginButtonPressed(string username, string password);

		private void OnCancelButtonPressed()
		{
			Alex.GameStateManager.Back();
		}

		protected void DisableInput()
		{
			LoginButton.Enabled = false;
			PasswordInput.Enabled = false;
			NameInput.Enabled = false;
		}

		protected void EnableInput()
		{
			LoginButton.Enabled = true;
			PasswordInput.Enabled = true;
			NameInput.Enabled = true;
		}
	}
}
