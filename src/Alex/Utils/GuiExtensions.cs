using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Gui.Elements;
using Chromely.CefGlue.Winapi;
using Chromely.CefGlue.Winapi.ChromeHost;
using Chromely.Core;
using Chromely.Core.Helpers;
using WinApi.Windows;

namespace Alex.Utils
{
	public static class GuiExtensions
	{
		public static void SetDefaultLinkHandler(this GuiTextElement element)
		{
			element.OnLinkClicked += (sender, e) =>
			{
				new Task(() =>
				{
					ChromelyConfiguration config = ChromelyConfiguration
						.Create()
						.WithAppArgs(new string[0])
						.WithHostSize(Alex.Instance.Window.ClientBounds.Width, Alex.Instance.Window.ClientBounds.Height)
						// The single process should only be used for debugging purpose.
						// For production, this should not be needed when the app is published 

						// Alternate approach for multi-process, is to add a subprocess application
						//.WithCustomSetting(CefSettingKeys.BrowserSubprocessPath, path_to_sunprocess)
#if RELEASE
					.WithCustomSetting(CefSettingKeys.SingleProcess, false)
#else
						.WithCustomSetting(CefSettingKeys.SingleProcess, true)
#endif
						.WithStartUrl(element.Text);


					var factory = WinapiHostFactory.Init();
					using (var window = factory.CreateWindow(() => new CefGlueBrowserHost(config),
						"Alex - External Website", constructionParams: new FrameWindowConstructionParams()))
					{
						window.SetSize(config.HostWidth, config.HostHeight);
						window.CenterToScreen();
						window.Show();

						new EventLoop().Run(window);
					}
				}).Start();
			};
		}
	}
}
