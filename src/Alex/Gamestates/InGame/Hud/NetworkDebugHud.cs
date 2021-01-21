using System;
using System.Text;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Net;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.InGame.Hud
{
    public class NetworkDebugHud : GuiScreen
    {
        private NetworkProvider            NetworkProvider    { get; }
        private GuiAutoUpdatingTextElement NetworkInfoElement { get; }
        private GuiTextElement             WarningElement     { get; }

        private bool _advanced = true;

        public bool Advanced
        {
            get
            {
                return _advanced;
            }
            set
            {
                _advanced = value;
                NetworkInfoElement.IsVisible = value;
            }
        }

        public NetworkDebugHud(NetworkProvider networkProvider)
        {
            NetworkProvider = networkProvider;
            Anchor = Alignment.Fill;
            
            WarningElement = new GuiTextElement
            {
                IsVisible = false,
                TextColor = TextColor.Red,
                Text = "",
                Anchor = Alignment.TopCenter,
                Scale = 1f,
                FontStyle = FontStyle.DropShadow,
                BackgroundOverlay = Color.Black * 0.5f
            };

            AddChild(WarningElement);
            
            NetworkInfoElement = new GuiAutoUpdatingTextElement(GetNetworkInfo, true);
            NetworkInfoElement.Interval = TimeSpan.FromMilliseconds(500);
            
            NetworkInfoElement.BackgroundOverlay = Color.Black * 0.5f;
            
            NetworkInfoElement.Anchor = Alignment.BottomRight;
            NetworkInfoElement.TextOpacity = 0.75f;
            NetworkInfoElement.TextColor = TextColor.Red;
            NetworkInfoElement.Scale = 1f;
            AddChild(NetworkInfoElement);
        }
        
        private string   _lastString = String.Empty;
        private DateTime _nextUpdate = DateTime.UtcNow;
        private bool     _state      = false;

        private string GetNetworkInfo()
        {
            var info = NetworkProvider.GetConnectionInfo();

            //double dataOut = (double) (info.BytesOut * 8L) / 1000000.0;
            //double dataIn  = (double) (info.BytesIn * 8L) / 1000000.0;
            double        dataOut = (double) (info.BytesOut) / 1000.0;
            double        dataIn  = (double) (info.BytesIn) / 1000.0;
            
            StringBuilder sb      = new StringBuilder();
            sb.Append($"Latency: {info.Latency}ms");
            sb.Append($" | Pkt in/out(#/s): {info.PacketsIn}/{info.PacketsOut}");

         //   if (info.Ack.HasValue && info.AckSent.HasValue)
            {
                sb.Append($" | Ack in/out(#/s): {info.Ack}/{info.AckSent}");
            }

            /* sb.Append($" | NACK's: {info.Nack}");
             sb.Append($" | Resends: {info.Resends}");
             sb.Append($" | Fails: {info.Fails}");*/
            sb.Append($" | THR in/out(Kbps): {dataIn:F2}/{dataOut:F2}");

            // WarningElement.IsVisible = info.State != ConnectionInfo.NetworkState.Ok;

            if (info.State != ConnectionInfo.NetworkState.Ok)
            {
                WarningElement.IsVisible = true;

                switch (info.State)
                {
                    case ConnectionInfo.NetworkState.OutOfOrder:
                        WarningElement.Text = "Warning: Datagram out of order!";

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
                    WarningElement.TextColor = TextColor.Yellow;
                }
                else
                {
                    WarningElement.TextColor = TextColor.Red;
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