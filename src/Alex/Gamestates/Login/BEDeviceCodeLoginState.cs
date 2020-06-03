using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using NLog;
using RocketUI;
using TextCopy;

namespace Alex.Gamestates.Login
{
    public class BEDeviceCodeLoginState : GuiMenuStateBase
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BEDeviceCodeLoginState));
	    
	    private readonly GuiPanoramaSkyBox _backgroundSkyBox;
		private XBLMSAService AuthenticationService { get; }
		private IPlayerProfileService _playerProfileService;
        protected GuiButton LoginButton;
		private Action<PlayerProfile> Ready { get; }
		private MsaDeviceAuthConnectResponse ConnectResponse { get; }
		private CancellationTokenSource CancellationToken { get; } = new CancellationTokenSource();
		private bool CanUseClipboard { get; }
        public BEDeviceCodeLoginState(GuiPanoramaSkyBox skyBox, Action<PlayerProfile> readyAction)
        {
            Title = "Bedrock Login";
            AuthenticationService = GetService<XBLMSAService>();
            _backgroundSkyBox = skyBox;
            Background = new GuiTexture2D(_backgroundSkyBox, TextureRepeatMode.Stretch);
            BackgroundOverlay = Color.Transparent;
            Ready = readyAction;

            CanUseClipboard = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            
            ConnectResponse = AuthenticationService.StartDeviceAuthConnect().Result;
			
            Initialize();
        }

        private void Initialize()
        {
	        _playerProfileService = GetService<IPlayerProfileService>();
	        _playerProfileService.Authenticate += PlayerProfileServiceOnAuthenticate;

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

            Body.BackgroundOverlay = new Color(Color.Black, 0.5f);
            Body.ChildAnchor = Alignment.MiddleCenter;
            
			Body.AddChild(new GuiTextElement()
			{
				TextColor = TextColor.Cyan,
				Text = ConnectResponse.user_code,
				FontStyle = FontStyle.Bold,
				Scale = 2f
			});

			if (CanUseClipboard)
			{
				AddGuiElement(new GuiTextElement()
				{
					Text = $"If you click Sign-In, the above auth code will be copied to your clipboard!"
				});
			}

			var buttonRow = AddGuiRow(LoginButton = new GuiButton(OnLoginButtonPressed)
            {
	            AccessKey = Keys.Enter,

	            Text = "Sign-In with Xbox",
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
        }

        private void PlayerProfileServiceOnAuthenticate(object sender, PlayerProfileAuthenticateEventArgs e)
        {
	        if (e.IsSuccess && e.Profile.IsBedrock)
	        {
		       // Alex.ProfileManager.CreateOrUpdateProfile(e.Profile.IsBedrock ? ProfileManager.ProfileType.Bedrock : ProfileManager.ProfileType.Java, e.Profile, true);
				//Ready?.Invoke();
	        }
	        else
	        {
		       // ErrorMessage.Text = "Could not login: " + e.ErrorMessage;
		       // ErrorMessage.TextColor = TextColor.Red;

		      //  EnableInput();
	        }
        }

        private void OnLoginButtonPressed()
        {
	  //      Log.Info("Login initiated...");
	        
	        LoginButton.Enabled = false;

	        var profileManager = GetService<ProfileManager>();
	        XBLMSAService.OpenBrowser(ConnectResponse.verification_uri);

	        if (CanUseClipboard)
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
	        AuthenticationService.DoDeviceCodeLogin(ConnectResponse.device_code, CancellationToken.Token).ContinueWith(
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
						        JsonConvert.SerializeObject(result.token),
						        true);

					        profileManager.CreateOrUpdateProfile(ProfileManager.ProfileType.Bedrock,profile, true);
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
