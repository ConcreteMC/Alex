using System;
using System.Diagnostics;
using MiNET.Net;
using MiNET.Net.RakNet;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
    public class MessageHandler : BedrockMessageHandlerBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        private readonly McpeClientMessageDispatcher _messageDispatcher;

        public Action ConnectionAction { get; set; }
        public Action<string, bool> DisconnectedAction { get; set; }
        
        public MessageHandler(RakSession session, IMcpeClientMessageHandler handler) : base(session)
        {
            _messageDispatcher = new McpeClientMessageDispatcher(handler);
        }

        public override void Connected()
        {
            ConnectionAction?.Invoke();
        }

        public override void Disconnect(string reason, bool sendDisconnect = true)
        {
            DisconnectedAction?.Invoke(reason, sendDisconnect);
        }

        public override Packet OnSendCustomPacket(Packet message)
        {
            return message;
        }

        public override void HandleCustomPacket(Packet message)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                _messageDispatcher.HandlePacket(message);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, $"Packet handling error: {message}");
            }
            finally
            {
                sw.Stop();

                if (sw.ElapsedMilliseconds > 250)
                {
                    Log.Warn($"Packet handling took longer than expected! Time elapsed: {sw.ElapsedMilliseconds}ms (Packet={message})");
                }
            }
        }
    }
}