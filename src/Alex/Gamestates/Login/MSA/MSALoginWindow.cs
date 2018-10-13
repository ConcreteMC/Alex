using System;
using System.Collections.Generic;
using System.Text;
using CefSharp;
using Chromely.CefGlue.Winapi;
using Chromely.CefGlue.Winapi.ChromeHost;
using Chromely.Core;
using Chromely.Core.Helpers;
using WinApi.Windows;

namespace Alex.Gamestates.Login.MSA
{
	public class MSALoginWindow
	{
		public MSALoginWindow()
		{
			ChromelyConfiguration config = ChromelyConfiguration
				.Create()
				.WithAppArgs(new string[0])
				.WithHostSize(480, 640)
#if RELEASE
					.WithCustomSetting(CefSettingKeys.SingleProcess, false)
#else
				.WithCustomSetting(CefSettingKeys.SingleProcess, true)
#endif
				.WithStartUrl("");

			var factory = WinapiHostFactory.Init();
			using (var window = factory.CreateWindow(() => new CefGlueBrowserHost(config),
				"Microsoft Account Sign-In", constructionParams: new FrameWindowConstructionParams()))
			{
				window.SetSize(config.HostWidth, config.HostHeight);
				window.CenterToScreen();
				window.Show();
				
				new EventLoop().Run(window);
			}
		}
	}
}
