using System;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Alex.Net;
using Microsoft.Xna.Framework;
using RocketUI;


namespace Alex.Gamestates.InGame.Hud
{
    public class NetworkDebugHud : Screen
    {
        private NetworkProvider            NetworkProvider    { get; }
        private AutoUpdatingTextElement NetworkInfoElement { get; }
        private TextElement             WarningElement     { get; }

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
                NetworkInfoElement.IsVisible = value;
            }
        }

        public NetworkDebugHud(NetworkProvider networkProvider)
        {
            NetworkProvider = networkProvider;
            Anchor = Alignment.Fill;
            
            WarningElement = new TextElement
            {
                IsVisible = false,
                TextColor = (Color) TextColor.Red,
                Text = "",
                Anchor = Alignment.TopLeft,
                Scale = 1f,
                FontStyle = FontStyle.DropShadow,
                BackgroundOverlay = Color.Black * 0.5f,
                Background = Color.Transparent
            };

            AddChild(WarningElement);

            NetworkInfoElement = new AutoUpdatingTextElement(GetNetworkInfo, true)
            {
                Background = Color.Transparent,
                Interval = TimeSpan.FromMilliseconds(500),
                BackgroundOverlay = Color.Black * 0.5f,
                Anchor = Alignment.BottomRight,
                TextOpacity = 0.75f,
                TextColor = (Color) TextColor.Red,
                Scale = 1f,
                IsVisible = false,
                TextAlignment = TextAlignment.Right
            };



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
            sb.AppendLine($"Latency: {info.Latency}ms");
            sb.AppendLine($"Pkt in/out(#/s): {info.PacketsIn:00}/{info.PacketsOut:00}");

             sb.AppendLine($"Ack in/out(#/s): {info.Ack:00}/{info.AckSent:00}");

              sb.AppendLine($"Nak in/out(#/s): {info.Nak:00}/{info.NakSent:00}");
             // sb.AppendLine($"Resends: {info.Resends:00}");
              //sb.AppendLine($"Fails: {info.Fails:00}");
            sb.Append($"THR in/out(Kbps): {dataIn:F2}/{dataOut:F2}");

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
                    WarningElement.TextColor = (Color) TextColor.Yellow;
                }
                else
                {
                    WarningElement.TextColor = (Color) TextColor.Red;
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