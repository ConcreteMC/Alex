using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.GameStates;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.Multiplayer;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Utils;
using Alex.Utils.Auth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using NLog;
using RocketUI;
using RocketUI.Utilities.IO;
using Skin = Alex.Common.Utils.Skin;


namespace Alex.Gamestates.Login
{
	public interface ILoginState : IGameState
	{
		void LoginFailed(string text);
	}

	public abstract class CodeFlowLoginBase<TType> : GuiMenuStateBase, ILoginState
		where TType : IDeviceAuthConnectResponse
	{
		public delegate void LoginSuccessfulCallback(PlayerProfile profile);

		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(CodeFlowLoginBase<TType>));

		private readonly GuiPanoramaSkyBox _backgroundSkyBox;
		protected Button LoginButton;

		private DateTime _requestTime = DateTime.UtcNow;
		private TType _connectResponse;

		protected TType ConnectResponse
		{
			get
			{
				return _connectResponse;
			}
			set
			{
				_connectResponse = value;

				if (_authCodeElement != null)
				{
					ShowCode();
					InvalidateLayout();
				}
			}
		}

		protected TimeSpan TimeRemaining =>
			DateTime.UtcNow - (_requestTime + TimeSpan.FromSeconds(_connectResponse?.ExpiresIn ?? 0));

		private CancellationTokenSource CancellationToken { get; } = new CancellationTokenSource();
		private bool CanUseClipboard { get; }

		private TextElement _authCodeElement;
		private TextElement _subTextElement;
		private UserSelectionState.ProfileSelected _loginSuccessful;
		private readonly ServerTypeImplementation _serverType;

		protected CodeFlowLoginBase(GuiPanoramaSkyBox skyBox,
			UserSelectionState.ProfileSelected readyAction,
			ServerTypeImplementation serverType)
		{
			Title = $"{serverType.DisplayName} Login";
			_backgroundSkyBox = skyBox;
			Background = new GuiTexture2D(_backgroundSkyBox, TextureRepeatMode.Stretch);
			BackgroundOverlay = Color.Transparent;
			_loginSuccessful = readyAction;
			_serverType = serverType;

			_authCodeElement = new TextElement()
			{
				TextColor = (Color)TextColor.Cyan,
				Text = "Please wait...\nStarting authentication process...",
				FontStyle = FontStyle.Italic,
				Scale = 1.1f
			};

			_subTextElement = new TextElement()
			{
				Text = $"If you click Sign-In, the above auth code will be copied to your clipboard!"
			};

			CanUseClipboard = Clipboard.IsClipboardAvailable();

			Initialize();
		}

		public void LoginFailed(string text)
		{
			_subTextElement.Text = text;
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

			Body.BackgroundOverlay = new Color(Color.Black, 0.5f);
			Body.ChildAnchor = Alignment.MiddleCenter;

			Body.AddChild(_authCodeElement);
			//ShowCode();

			_subTextElement.IsVisible = CanUseClipboard;

			//if (CanUseClipboard)
			{
				AddRocketElement(_subTextElement);
			}

			var buttonRow = AddGuiRow(
				LoginButton = new AlexButton(OnLoginButtonPressed)
				{
					AccessKey = Keys.Enter,
					Text = $"Sign-In",
					Margin = new Thickness(5),
					Width = 100,
					Enabled = ConnectResponse != null
				}.ApplyModernStyle(false),
				new AlexButton(OnCancelButtonPressed)
				{
					AccessKey = Keys.Escape, TranslationKey = "gui.cancel", Margin = new Thickness(5), Width = 100
				}.ApplyModernStyle(false));

			buttonRow.ChildAnchor = Alignment.MiddleCenter;
		}

		protected abstract Task<TType> StartConnect();

		/// <inheritdoc />
		protected override void OnShow()
		{
			base.OnShow();

			RefreshCode();
		}

		private void RefreshCode()
		{
			RegisterLoading();

			try
			{
				_requestTime = DateTime.UtcNow;

				Task.Run(StartConnect).ContinueWith(
					r =>
					{
						ConnectResponse = r.Result;
						ShowCode();
					});
			}
			finally
			{
				UnregisterLoading();
			}
		}

		private void ShowCode()
		{
			if (ConnectResponse != null)
			{
				LoginButton.Enabled = true;

				_authCodeElement.TextColor = (Color)TextColor.Cyan;
				_authCodeElement.FontStyle = FontStyle.Bold;
				_authCodeElement.Scale = 2f;
				_authCodeElement.Text = ConnectResponse.UserCode;
			}
			else
			{
				LoginButton.Enabled = false;
			}
		}

		/// <summary>
		/// Should return true if successfull.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected abstract Task<LoginResponse> ProcessLogin(CancellationToken cancellationToken);

		private void OnLoginButtonPressed()
		{
			//      Log.Info("Login initiated...");

			LoginButton.Enabled = false;
			XboxAuthService.OpenBrowser(ConnectResponse.VerificationUrl);

			if (Clipboard.IsClipboardAvailable())
			{
				try
				{
					Clipboard.SetText(ConnectResponse.UserCode);
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Could not set keyboard contents!");
				}
			}


			Task.Run(
				async () =>
				{
					RegisterLoading();

					UpdateLoadingText("Please wait...");

					try
					{
						var loginResponse = await ProcessLogin(CancellationToken.Token);

						if (loginResponse == null || !loginResponse.Success)
						{
							ShowAuthenticationError(loginResponse?.Error);

							return;
						}

						var profileUpdateResult = await _serverType.UpdateProfile(loginResponse.Profile);

						if (!profileUpdateResult.Success)
						{
							ShowAuthenticationError(profileUpdateResult.ErrorMessage);

							return;
						}

						Alex.GameStateManager.Back();
						_loginSuccessful?.Invoke(profileUpdateResult.Profile);
					}
					finally
					{
						UnregisterLoading();
					}
				});
		}

		protected void ShowAuthenticationError(string reason = null)
		{
			_authCodeElement.TextColor = (Color)TextColor.Red;
			_authCodeElement.FontStyle = FontStyle.Bold;
			_authCodeElement.Scale = 2f;
			_authCodeElement.Text = $"Authentication failed.";

			if (string.IsNullOrWhiteSpace(reason))
			{
				_subTextElement.Text = $"Check the logs for more information.";
			}
			else
			{
				_subTextElement.Text = reason;
			}

			_subTextElement.IsVisible = true;
		}

		private void OnCancelButtonPressed()
		{
			CancellationToken.Cancel();
			Alex.GameStateManager.Back();
		}

		private int _loading = 0;
		private object _loadingLock = new object();

		protected void RegisterLoading()
		{
			lock (_loadingLock)
			{
				UpdateLoadingState(_loading, _loading++);
			}
		}

		protected void UnregisterLoading()
		{
			lock (_loadingLock)
			{
				UpdateLoadingState(_loading, _loading--);
			}
		}

		private GenericLoadingDialog _genericLoadingDialog = null;

		private void UpdateLoadingState(int previous, int loading)
		{
			lock (_loadingLock)
			{
				if (previous == 0 && loading == 1)
				{
					//Started loading.
					_genericLoadingDialog = GuiManager?.CreateDialog<GenericLoadingDialog>();

					if (_genericLoadingDialog != null)
					{
						_genericLoadingDialog.SizeToWindow = true;
						_genericLoadingDialog.AutoSizeMode = AutoSizeMode.GrowAndShrink;

						_genericLoadingDialog.Show();
					}
				}
				else if (previous == 1 && loading == 0)
				{
					//Stopped loading
					var loadingOverlay = _genericLoadingDialog;

					if (loadingOverlay != null)
					{
						_genericLoadingDialog.Close();
						_genericLoadingDialog = null;
					}
				}
			}
		}

		protected void UpdateLoadingText(string text)
		{
			lock (_loadingLock)
			{
				var overlay = _genericLoadingDialog;

				if (overlay == null)
					return;

				overlay.Text = text;
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			// _backgroundSkyBox.Update(gameTime);
			var overlay = _genericLoadingDialog;

			if (overlay != null)
			{
				var timeRemaining = TimeRemaining;
				var totalTime = TimeSpan.FromSeconds(_connectResponse?.ExpiresIn ?? 0);

				overlay.UpdateProgress(
					(int)((totalTime.TotalSeconds / 100) * timeRemaining.TotalSeconds), "Processing...");
			}
		}

		protected override void OnDraw(IRenderArgs args)
		{
			base.OnDraw(args);
			_backgroundSkyBox.Draw(args);
		}
	}
}