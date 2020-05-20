using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Network;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.GameStates.Playing
{
    public class NetworkScreen : GuiScreen
    {
        private INetworkProvider NetworkProvider { get; }
        private GuiAutoUpdatingTextElement NetworkInfoElement { get; }
        public NetworkScreen(INetworkProvider networkProvider)
        {
            NetworkProvider = networkProvider;
            Anchor = Alignment.Fill;
            
            NetworkInfoElement = new GuiAutoUpdatingTextElement(GetNetworkInfo, true);
            NetworkInfoElement.BackgroundOverlay = Color.Black * 0.5f;
            
            NetworkInfoElement.Anchor = Alignment.BottomRight;
            NetworkInfoElement.TextOpacity = 0.5f;
            NetworkInfoElement.TextColor = TextColor.Red;
            NetworkInfoElement.Scale = 0.5f;
            AddChild(NetworkInfoElement);
        }
        
        private string GetNetworkInfo()
        {
            var info = NetworkProvider.GetConnectionInfo();
            
            StringBuilder sb = new StringBuilder();
            sb.Append($"Latency: {info.Latency}");
            sb.Append($" | Ack: {info.AckSent} sent / {info.Ack} received");
            sb.Append($" | NACK's: {info.Nack}");
            sb.Append($" | Resends: {info.Resends}");
            sb.Append($" | Fails: {info.Fails}");
            
            return sb.ToString();
        }
    }
}