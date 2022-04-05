using System;
using System.Text;
using Alex.Common;
using Alex.Common.Utils;
using Alex.Interfaces;
using Alex.Interfaces.Net;
using Alex.Net;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Screens.Ingame
{
	public class NetworkDebugHud : Screen
	{
		private NetworkProvider NetworkProvider { get; }
		private AutoUpdatingTextElement NetworkInfoElement { get; }
		private TextElement WarningElement { get; }

		private bool _advanced = false;

		public bool Advanced
		{
			get
			{
				return _advanced;
			}
			set
			{
				_advanced = value;

				if (value)
				{
					AddChild(NetworkInfoElement);
				}
				else
				{
					RemoveChild(NetworkInfoElement);
				}
			}
		}

		public NetworkDebugHud(NetworkProvider networkProvider)
		{
			NetworkProvider = networkProvider;
			Anchor = Alignment.Fill;

			WarningElement = new TextElement
			{
				IsVisible = false,
				TextColor = (Color)TextColor.Red.ToXna(),
				Text = "",
				Anchor = Alignment.TopLeft,
				Scale = 1f,
				FontStyle = FontStyle.DropShadow,
				Background = Color.Transparent,
				BackgroundOverlay = Color.Black * 0.5f
			};

			AddChild(WarningElement);

			NetworkInfoElement = new AutoUpdatingTextElement(GetNetworkInfo, true)
			{
				Background = Color.Transparent,
				Interval = TimeSpan.FromMilliseconds(500),
				BackgroundOverlay = Color.Black * 0.5f,
				Anchor = Alignment.BottomRight,
				TextOpacity = 0.95f,
				TextColor = (Color)TextColor.Red.ToXna(),
				Scale = 1f,
				IsVisible = true,
				TextAlignment = TextAlignment.Right
			};

			// AddChild(NetworkInfoElement);
		}

		private string _lastString = String.Empty;
		private bool _state = false;

		private string GetNetworkInfo()
		{
			var info = NetworkProvider.ConnectionInfo;

			//double dataOut = (double) (info.BytesOut * 8L) / 1000000.0;
			//double dataIn  = (double) (info.BytesIn * 8L) / 1000000.0;
			double dataOut = (double)(info.BytesOut) / 1000.0;
			double dataIn = (double)(info.BytesIn) / 1000.0;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Latency: {info.Latency:00}ms");

			/*if (info.Nak >= 0 && info.NakSent >= 0)
			{
			    sb.AppendLine($"Pkt loss: {info.PacketLoss}%");
			}*/

			sb.AppendLine($"Pkt in/out(#/s): {info.PacketsIn:00}/{info.PacketsOut:00}");

			if (info.Ack >= 0)
			{
				sb.AppendLine($"Ack in/out(#/s): {info.Ack:00}/{info.AckSent:00}");
			}

			if (info.Nak >= 0)
			{
				sb.AppendLine($"Nak in/out(#/s): {info.Nak:00}/{info.NakSent:00}");
			}

			if (info.Resends >= 0 || info.Fails >= 0)
			{
				sb.AppendLine($"Resends/Fails: {info.Resends:00}/{info.Fails:00}");
			}

			sb.Append(
				$"THR in/out: {FormattingUtils.GetBytesReadable(info.BytesIn, 2)}ps/{FormattingUtils.GetBytesReadable(info.BytesOut, 2)}ps");

			;
			// WarningElement.IsVisible = info.State != ConnectionInfo.NetworkState.Ok;

			if (info.State != ConnectionInfo.NetworkState.Ok)
			{
				WarningElement.IsVisible = true;

				switch (info.State)
				{
					case ConnectionInfo.NetworkState.OutOfOrder:
						WarningElement.Text = "Warning: Network out of order!";

						break;

					case ConnectionInfo.NetworkState.Slow:
						WarningElement.Text = "Warning: Slow networking detected!";

						break;

					case ConnectionInfo.NetworkState.HighPing:
						WarningElement.Text = "Warning: High latency detected, this may cause lag.";

						break;

					case ConnectionInfo.NetworkState.PacketLoss:
						WarningElement.Text = "Warning: Packet loss detected!";

						break;
				}

				if (_state)
				{
					WarningElement.TextColor = (Color)TextColor.Yellow.ToXna();
				}
				else
				{
					WarningElement.TextColor = (Color)TextColor.Red.ToXna();
				}

				_state = !_state;
			}
			else
			{
				WarningElement.IsVisible = false;
			}

			_lastString = sb.ToString();
			// _nextUpdate = DateTime.UtcNow.AddSeconds(0.5);

			return _lastString;
		}
	}
}