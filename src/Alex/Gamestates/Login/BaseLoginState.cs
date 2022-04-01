using Alex.Common;
using Alex.Common.Gui.Elements;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;


namespace Alex.Gamestates.Login
{
	public abstract class BaseLoginState : GuiMenuStateBase
	{
		protected TextInput NameInput;
		protected TextInput PasswordInput;
		protected Button LoginButton;
		protected TextElement ErrorMessage;

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
			TextElement t;

			Footer.AddChild(
				t = new TextElement()
				{
					Text = "We are NOT in anyway or form affiliated with Mojang/Minecraft or Microsoft!",
					TextColor = (Color)TextColor.Yellow,
					Scale = 1f,
					FontStyle = FontStyle.DropShadow,
					Anchor = Alignment.MiddleCenter
				});

			TextElement info;

			Footer.AddChild(
				info = new TextElement()
				{
					Text = "We will never collect/store or do anything with your data.",
					TextColor = (Color)TextColor.Yellow,
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

			var usernameRow = AddGuiRow(
				new TextElement() { Text = "Username:", Margin = new Thickness(0, 0, 5, 0) },
				NameInput = new TextInput()
				{
					TabIndex = 1, Width = 200, PlaceHolder = "Username...", Margin = new Thickness(5),
				});

			usernameRow.ChildAnchor = Alignment.MiddleCenter;

			var passwordRow = AddGuiRow(
				new TextElement() { Text = "Password:", Margin = new Thickness(0, 0, 5, 0) },
				PasswordInput = new TextInput()
				{
					TabIndex = 2,
					Width = 200,
					PlaceHolder = "Password...",
					Margin = new Thickness(5),
					IsPasswordInput = true
				});

			passwordRow.ChildAnchor = Alignment.MiddleCenter;

			var buttonRow = AddGuiRow(
				LoginButton = new AlexButton(OnLoginButtonPressed)
				{
					AccessKey = Keys.Enter,
					Text = "Login",
					Margin = new Thickness(5),
					Width = 100,
					TabIndex = 3
				}.ApplyModernStyle(false),
				new AlexButton(OnCancelButtonPressed)
				{
					AccessKey = Keys.Escape,
					TranslationKey = "gui.cancel",
					Margin = new Thickness(5),
					Width = 100,
					TabIndex = 4
				}.ApplyModernStyle(false));

			buttonRow.ChildAnchor = Alignment.MiddleCenter;

			AddRocketElement(ErrorMessage = new TextElement() { TextColor = (Color)TextColor.Yellow });

			Initialized();
		}

		protected abstract void Initialized();

		private void OnLoginButtonPressed()
		{
			DisableInput();

			//	LoginButton.Enabled = false;
			ErrorMessage.Text = "Authenticating...";

			LoginButtonPressed(NameInput.Value, PasswordInput.Value);
		}

		protected abstract void LoginButtonPressed(string username, string password);

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

		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			_backgroundSkyBox.Draw(new RenderArgs()
			{
				GameTime = gameTime,
				GraphicsDevice = graphics.Context.GraphicsDevice,
				SpriteBatch = graphics.SpriteBatch
			});
			base.OnDraw(graphics, gameTime);
		}
	}
}