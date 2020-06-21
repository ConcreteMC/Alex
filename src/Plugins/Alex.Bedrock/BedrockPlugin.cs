using System;
using Alex.Plugins;
using MiNET.Net;
using NLog;

namespace Alex.Bedrock
{
	/*[PluginInfo]
	public class BedrockPlugin : Plugin
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockPlugin));
		
		public Alex Game { get; }
		internal XBLMSAService AuthenticationService { get; }
		public BedrockPlugin(Alex alex)
		{
			Game = alex;
			AuthenticationService = new XBLMSAService();
			PacketFactory.CustomPacketFactory = new AlexPacketFactory();
		}
		
		/// <inheritdoc />
		public override void Enabled()
		{
			if (Game.ServerTypeManager.TryRegister("bedrock", new BedrockServerType(this, AuthenticationService)))
			{
				Log.Info($"Registered bedrock implementation for version {McpeProtocolInfo.GameVersion}");
			}
			else
			{
				Log.Warn($"Could not register bedrock server type.");
			}
		}

		/// <inheritdoc />
		public override void Disabled()
		{
			
		}
	}*/
}