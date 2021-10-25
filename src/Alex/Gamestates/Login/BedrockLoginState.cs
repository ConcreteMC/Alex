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
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockLoginState));
	    public BedrockLoginState(GuiPanoramaSkyBox skyBox, LoginSuccessfulCallback loginSuccessfulCallback, XboxAuthService xboxAuthService, ServerTypeImplementation serverTypeImplementation) : base(skyBox, loginSuccessfulCallback, serverTypeImplementation)
        {
	        _xboxAuthService = xboxAuthService;
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

				    var profile = new PlayerProfile(
					    r.ExtraData.XUID, r.ExtraData.DisplayName, r.ExtraData.DisplayName, result.token.AccessToken,
					    null, result.token.RefreshToken, result.token.ExpiryTime);

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
