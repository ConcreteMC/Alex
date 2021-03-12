using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements;

using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Utils;
using Alex.Utils.Auth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using NLog;
using RocketUI;


namespace Alex.Gamestates.Login
{
    public class BedrockLoginState : GuiMenuStateBase
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockLoginState));
	    
	    private readonly GuiPanoramaSkyBox            _backgroundSkyBox;
		private          XboxAuthService              AuthenticationService { get; }
		private          IPlayerProfileService        _playerProfileService;
        protected        Button                    LoginButton;
		private          Action<PlayerProfile>        Ready             { get; }

		private MsaDeviceAuthConnectResponse _connectResponse;

		private MsaDeviceAuthConnectResponse ConnectResponse
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
		private CancellationTokenSource      CancellationToken { get; }      = new CancellationTokenSource();
		private bool                         CanUseClipboard   { get; }

		private TextElement _authCodeElement;
        public BedrockLoginState(GuiPanoramaSkyBox skyBox, Action<PlayerProfile> readyAction, XboxAuthService xboxAuthService)
        {
            Title = "Bedrock Login";
            AuthenticationService = xboxAuthService;
            _backgroundSkyBox = skyBox;
            Background = new GuiTexture2D(_backgroundSkyBox, TextureRepeatMode.Stretch);
            BackgroundOverlay = Color.Transparent;
            Ready = readyAction;

            _authCodeElement = new TextElement()
            {
	            TextColor = (Color) TextColor.Cyan,
	            Text = "Please wait...\nStarting authentication process...",
	            FontStyle = FontStyle.Italic,
	            Scale = 1.1f
            };
            
            CanUseClipboard = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            
            Initialize();
        }

        private void Initialize()
        {
	        _playerProfileService = GetService<IPlayerProfileService>();
	        _playerProfileService.Authenticate += PlayerProfileServiceOnAuthenticate;

            base.HeaderTitle.Anchor = Alignment.MiddleCenter;
            base.HeaderTitle.FontStyle = FontStyle.Bold | FontStyle.DropShadow;
            Footer.ChildAnchor = Alignment.MiddleCenter;
            TextElement t;
            Footer.AddChild(t = new TextElement()
            {
                Text = "We are NOT in anyway or form affiliated with Mojang/Minecraft or Microsoft!",
                TextColor = (Color) TextColor.Yellow,
                Scale = 1f,
                FontStyle = FontStyle.DropShadow,

                Anchor = Alignment.MiddleCenter
            });

            TextElement info;
            Footer.AddChild(info = new TextElement()
            {
                Text = "We will never collect/store or do anything with your data.",

                TextColor = (Color) TextColor.Yellow,
                Scale = 0.8f,
                FontStyle = FontStyle.DropShadow,

                Anchor = Alignment.MiddleCenter,
                Padding = new Thickness(0, 5, 0, 0)
            });

            Body.BackgroundOverlay = new Color(Color.Black, 0.5f);
            Body.ChildAnchor = Alignment.MiddleCenter;
            
			Body.AddChild(_authCodeElement);
			//ShowCode();

			if (CanUseClipboard)
			{
				AddRocketElement(new TextElement()
				{
					Text = $"If you click Sign-In, the above auth code will be copied to your clipboard!"
				});
			}

			var buttonRow = AddGuiRow(LoginButton = new AlexButton(OnLoginButtonPressed)
            {
	            AccessKey = Keys.Enter,

	            Text = "Sign-In with Xbox",
	            Margin = new Thickness(5),
	            Width = 100,
	            Enabled = ConnectResponse != null
            }.ApplyModernStyle(false), new AlexButton(OnCancelButtonPressed)
            {
	            AccessKey = Keys.Escape,

	            TranslationKey = "gui.cancel",
	            Margin = new Thickness(5),
	            Width = 100
            }.ApplyModernStyle(false));
            buttonRow.ChildAnchor = Alignment.MiddleCenter;
        }

        /// <inheritdoc />
        protected override void OnShow()
        {
	        base.OnShow();
	        
	        AuthenticationService.StartDeviceAuthConnect().ContinueWith(
		        async r =>
		        {
			        ConnectResponse = await r;

			        LoginButton.Enabled = true;
			        ShowCode();
		        });
        }

        private void ShowCode()
        {
	        if (ConnectResponse != null)
	        {
		        _authCodeElement.TextColor = (Color) TextColor.Cyan;
		        _authCodeElement.FontStyle = FontStyle.Bold;
		        _authCodeElement.Scale = 2f;
		        _authCodeElement.Text = ConnectResponse.user_code;
	        }
        }
        
        private void PlayerProfileServiceOnAuthenticate(object sender, PlayerProfileAuthenticateEventArgs e)
        {
			
        }

        private void OnLoginButtonPressed()
        {
	  //      Log.Info("Login initiated...");
	        
	        LoginButton.Enabled = false;

	        var profileManager = GetService<ProfileManager>();

	        XboxAuthService.OpenBrowser(ConnectResponse.verification_uri);

	        if (Clipboard.IsClipboardAvailable())
	        {
		        try
		        {
			        Clipboard.SetText(ConnectResponse.user_code);
		        }
		        catch (Exception ex)
		        {
			        Log.Warn(ex, $"Could not set keyboard contents!");
		        }
	        }

	        //   Log.Info($"Browser opened...");
	        AuthenticationService.DoDeviceCodeLogin(GetService<ResourceManager>().DeviceID, ConnectResponse.device_code, CancellationToken.Token).ContinueWith(
		        (task) =>
		        {
			        try
			        {
				        var result = task.Result;
				        if (result.success)
				        {

					        var r = AuthenticationService.DecodedChain.Chain.FirstOrDefault(x =>
						        x.ExtraData != null && !string.IsNullOrWhiteSpace(x.ExtraData.Xuid));

					        var profile = new PlayerProfile(r.ExtraData.Xuid, r.ExtraData.DisplayName,
						        r.ExtraData.DisplayName,
						       new Skin()
						       {
							       Slim = true,
							       Texture = null
						       }, result.token.AccessToken,
						        JsonConvert.SerializeObject(result.token));

					        profileManager.CreateOrUpdateProfile("bedrock" ,profile, true);
					        Ready?.Invoke(profile);

					        //Log.Info($"Continuewith Success!");
				        }
				        else
				        {
					        //Log.Info($"Continuewith fail!");
				        }
			        }
			        catch (Exception ex)
			        {
				        Log.Warn($"Authentication issue: {ex.ToString()}");
			        }
		        });
        }

        private void OnCancelButtonPressed()
        {
	        Alex.GameStateManager.Back();
	        CancellationToken.Cancel();
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
