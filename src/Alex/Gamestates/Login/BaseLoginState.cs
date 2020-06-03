using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.Gamestates.Login
{
	public abstract class BaseLoginState : GuiMenuStateBase
	{
		protected GuiTextInput NameInput;
		protected GuiTextInput PasswordInput;
		protected GuiButton LoginButton;
		protected GuiTextElement ErrorMessage;

		private readonly GuiPanoramaSkyBox _backgroundSkyBox;
		protected BaseLoginState(string title, GuiPanoramaSkyBox skyBox)
		{
			Title = title;

			_backgroundSkyBox = skyBox;
			Background = new GuiTexture2D(_backgroundSkyBox, TextureRepeatMode.Stretch);
			BackgroundOverlay = Color.Transparent;

			Initialize();
		}

		private void Initialize()
		{
			base.HeaderTitle.Anchor = Alignment.MiddleCenter;
			base.HeaderTitle.FontStyle = FontStyle.Bold | FontStyle.DropShadow;
			Footer.ChildAnchor = Alignment.MiddleCenter;
			GuiTextElement t;
			Footer.AddChild(t = new GuiTextElement()
			{
				Text = "We are NOT in anyway or form affiliated with Mojang/Minecraft or Microsoft!",
				TextColor = TextColor.Yellow,
				Scale = 1f,
				FontStyle = FontStyle.DropShadow,

				Anchor = Alignment.MiddleCenter
			});

			GuiTextElement info;
			Footer.AddChild(info = new GuiTextElement()
			{
				Text = "We will never collect/store or do anything with your data.",

				TextColor = TextColor.Yellow,
				Scale = 0.8f,
				FontStyle = FontStyle.DropShadow,

				Anchor = Alignment.MiddleCenter,
				Padding = new Thickness(0, 5, 0, 0)
			});

			/*
			 *  "We will never collect/store or do anything with your data.\n" +
					   "You can read more about the authentication method we use on here: https://wiki.vg/Authentication"
			 */
			Body.BackgroundOverlay = new Color(Color.Black, 0.5f);
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

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			_backgroundSkyBox.Update(gameTime);
		}

		protected override void OnDraw(IRenderArgs args)
		{
			base.OnDraw(args);
			_backgroundSkyBox.Draw(args);
		}
	}
}
