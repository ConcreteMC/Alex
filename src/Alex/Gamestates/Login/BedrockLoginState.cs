using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Graphics;
using Alex.Common.Gui.Elements;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Utils;
using Alex.Utils.Auth;
using NLog;
using Skin = Alex.Common.Utils.Skin;


namespace Alex.Gamestates.Login
{
    public class BedrockLoginState : CodeFlowLoginBase<MsaDeviceAuthConnectResponse>
    {
	    private readonly XboxAuthService _xboxAuthService;
	    private readonly PlayerProfile _currentProfile;
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockLoginState));
	    public BedrockLoginState(GuiPanoramaSkyBox skyBox, LoginSuccessfulCallback loginSuccessfulCallback, XboxAuthService xboxAuthService, ServerTypeImplementation serverTypeImplementation,
		    PlayerProfile currentProfile = null) : base(skyBox, loginSuccessfulCallback, serverTypeImplementation)
	    {
		    _xboxAuthService = xboxAuthService;
		    _currentProfile = currentProfile;
	    }

	    /// <inheritdoc />
	    protected override Task<MsaDeviceAuthConnectResponse> StartConnect()
	    {
		    return _xboxAuthService.StartDeviceAuthConnect();
	    }

	    /// <inheritdoc />
	    protected override async Task<LoginResponse> ProcessLogin(CancellationToken cancellationToken)
	    {
		    try
		    {
			    var result = await _xboxAuthService.DoDeviceCodeLogin(
				    GetService<ResourceManager>().DeviceID, ConnectResponse.DeviceCode, cancellationToken);

			    if (result.success)
			    {
				    var r = _xboxAuthService.DecodedChain.Chain.FirstOrDefault(
					    x => x.ExtraData != null && !string.IsNullOrWhiteSpace(x.ExtraData.XUID));

				    var profile = _currentProfile ?? new PlayerProfile();
				    profile.UUID = r.ExtraData.XUID;
				    profile.Username = r.ExtraData.DisplayName;
				    profile.AccessToken = result.token.AccessToken;
				    profile.ClientToken = null;
				    profile.PlayerName = r.ExtraData.DisplayName;
				    profile.RefreshToken = result.token.RefreshToken;
				    profile.ExpiryTime = result.token.ExpiryTime;

				    profile.Authenticated = true;

				    return new LoginResponse(profile, true);
			    }
		    }
		    catch (Exception ex)
		    {
			    Log.Warn(ex, $"Unknown Auth issue.");
		    }
		    
		    return new LoginResponse(null, false);
	    }
    }

    public class LoginResponse
    {
	    public LoginResponse(PlayerProfile profile, bool success, string error = null)
	    {
		    Success = success;
		    Profile = profile;
		    Error = error;
	    }

	    public PlayerProfile Profile { get; }
	    public bool Success { get; }
	    public string Error { get; }
	    
	    
    }
}
