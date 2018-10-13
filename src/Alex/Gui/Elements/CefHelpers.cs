using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace Alex.Gui.Elements
{
	public class ProxyConfig
	{
		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool InternetQueryOption(IntPtr hInternet, uint dwOption, IntPtr lpBuffer, ref int lpdwBufferLength);

		private const uint InternetOptionProxy = 38;

		public static InternetProxyInfo GetProxyInformation()
		{
			var bufferLength = 0;
			InternetQueryOption(IntPtr.Zero, InternetOptionProxy, IntPtr.Zero, ref bufferLength);
			var buffer = IntPtr.Zero;

			try
			{
				buffer = Marshal.AllocHGlobal(bufferLength);

				if (InternetQueryOption(IntPtr.Zero, InternetOptionProxy, buffer, ref bufferLength))
				{
					var ipi = (InternetProxyInfo)Marshal.PtrToStructure(buffer, typeof(InternetProxyInfo));
					return ipi;
				}
				{
					throw new Win32Exception();
				}
			}
			finally
			{
				if (buffer != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(buffer);
				}
			}
		}
	}

	public struct InternetProxyInfo
	{
		public InternetOpenType AccessType;
		public string ProxyAddress;
		public string ProxyBypass;
	}

	public enum InternetOpenType
	{
		PreConfig = 0,
		Direct = 1,
		Proxy = 3,
		PreConfigWithNoAutoProxy = 4
	}

	public class BrowserProcessHandler : IBrowserProcessHandler
	{
		/// <summary>
		/// The maximum number of milliseconds we're willing to wait between calls to OnScheduleMessagePumpWork().
		/// </summary>
		protected const int MaxTimerDelay = 1000 / 30;  // 30fps

		void IBrowserProcessHandler.OnContextInitialized()
		{
			//The Request Context has been initialized, you can now set preferences, like proxy server settings
			var cookieManager = Cef.GetGlobalCookieManager();
			cookieManager.SetStoragePath("cookies", true);
			cookieManager.SetSupportedSchemes(new string[] { "custom" });
			if (cookieManager.SetCookie("custom://cefsharp/index.html", new Cookie
			{
				Name = "CefSharpTestCookie",
				Value = "ILikeCookies",
				Expires = DateTime.Now.AddDays(1)
			}))
			{
				cookieManager.VisitUrlCookiesAsync("custom://cefsharp/index.html", false).ContinueWith(previous =>
				{
					if (previous.Status == TaskStatus.RanToCompletion)
					{
						var cookies = previous.Result;

						foreach (var cookie in cookies)
						{
							Debug.WriteLine("CookieName:" + cookie.Name);
						}
					}
					else
					{
						Debug.WriteLine("No Cookies found");
					}
				});
			}

			//Dispose of context when finished - preferable not to keep a reference if possible.
			using (var context = Cef.GetGlobalRequestContext())
			{
				string errorMessage;
				//You can set most preferences using a `.` notation rather than having to create a complex set of dictionaries.
				//The default is true, you can change to false to disable
				context.SetPreference("webkit.webprefs.plugins_enabled", true, out errorMessage);
			}
		}

		void IBrowserProcessHandler.OnScheduleMessagePumpWork(long delay)
		{
			//If the delay is greater than the Maximum then use MaxTimerDelay
			//instead - we do this to achieve a minimum number of FPS
			if (delay > MaxTimerDelay)
			{
				delay = MaxTimerDelay;
			}
			OnScheduleMessagePumpWork((int)delay);
		}

		protected virtual void OnScheduleMessagePumpWork(int delay)
		{
			//TODO: Schedule work on the UI thread - call Cef.DoMessageLoopWork
		}

		public virtual void Dispose()
		{

		}
	}
}
