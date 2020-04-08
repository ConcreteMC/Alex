using System;
using MiNET.Net;
using MiNET.Net.RakNet;
using NLog;

namespace Alex.Worlds.Bedrock
{
    public class SafeMessageHandler : BedrockClientMessageHandler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SafeMessageHandler));
        
        public SafeMessageHandler(RakSession session, IMcpeClientMessageHandler handler) : base(session, handler)
        {
            
        }

        public override void HandleCustomPacket(Packet message)
        {
            try
            {
                base.HandleCustomPacket(message);
            }
            catch (Exception ex)
            {
                Log.Error($"Custom handler: {ex.ToString()}");
            }
        }
    }
}