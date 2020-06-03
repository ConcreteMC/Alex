using System;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Net;
using Alex.Networking.Bedrock.Net.Raknet;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates.InGame.Hud
{
    public class NetworkDebugHud : GuiScreen
    {
        private NetworkProvider NetworkProvider { get; }
        private GuiAutoUpdatingTextElement NetworkInfoElement { get; }
        public NetworkDebugHud(NetworkProvider networkProvider)
        {
            NetworkProvider = networkProvider;
            Anchor = Alignment.Fill;
            
            NetworkInfoElement = new GuiAutoUpdatingTextElement(GetNetworkInfo, true);
            NetworkInfoElement.BackgroundOverlay = Color.Black * 0.5f;
            
            NetworkInfoElement.Anchor = Alignment.BottomRight;
            NetworkInfoElement.TextOpacity = 0.75f;
            NetworkInfoElement.TextColor = TextColor.Red;
            NetworkInfoElement.Scale = 1f;
            AddChild(NetworkInfoElement);
        }
        
        private string _lastString = String.Empty;
        private DateTime _nextUpdate = DateTime.UtcNow;
        private string GetNetworkInfo()
        {
            if (DateTime.UtcNow >= _nextUpdate)
            {
                var info = NetworkProvider.GetConnectionInfo();

                double dataOut = (double) (info.BytesOut * 8L) / 1000000.0;
                double dataIn = (double) (info.BytesIn * 8L) / 1000000.0;
                
                StringBuilder sb = new StringBuilder();
                sb.Append($"Latency: {CustomConnectedPong.Latency}ms");
                sb.Append($" | Ack in/out(#/s): {info.Ack}/{info.AckSent}");
               /* sb.Append($" | NACK's: {info.Nack}");
                sb.Append($" | Resends: {info.Resends}");
                sb.Append($" | Fails: {info.Fails}");*/
                sb.Append($" | THR in/out(Mbps): {dataIn:F}/{dataOut:F}");

                _lastString = sb.ToString();
               // _nextUpdate = DateTime.UtcNow.AddSeconds(0.5);
            }

            return _lastString;
        }
    }
}