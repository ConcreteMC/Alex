using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Alex.API.Data;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.API.World;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Blocks;
using MiNET.Client;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;

namespace Alex.Worlds.Bedrock
{
	public class BedrockMotd
	{
		public string Edition;
		public string MOTD;
		public int MaxPlayers;
		public int Players;
		public int ProtocolVersion;
		public string ClientVersion;

		public BedrockMotd(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return;

			var split = raw.Split(';');
			int i = 0;
			Edition = split[i++];
			MOTD = split[i++];

			if (int.TryParse(split[i++], out int protocolVersion))
			{
				ProtocolVersion = protocolVersion;
			}
			
			ClientVersion = split[i++];

			if (int.TryParse(split[i++], out int players))
			{
				Players = players;
			}

			if (int.TryParse(split[i++], out int maxplayers))
			{
				MaxPlayers = maxplayers;
			}
		}
	}
	public class BedrockClient : MiNetClient, INetworkProvider, IChatProvider, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClient));
		
		public ManualResetEventSlim ConnectionAcceptedWaitHandle { get; }
		public BedrockWorldProvider WorldProvider { get; }
		public EventHandler<BedrockMotd> OnMotdReceivedHandler;
		public BedrockMotd KnownMotd = new BedrockMotd(string.Empty);

		public BedrockClient(Alex alex,IPEndPoint endpoint, string username, DedicatedThreadPool threadPool, BedrockWorldProvider wp) : base(endpoint,
			username, threadPool)
		{
			WorldProvider = wp;
			ConnectionAcceptedWaitHandle = new ManualResetEventSlim(false);
			MessageDispatcher = new McpeClientMessageDispatcher(new BedrockClientPacketHandler(this, alex));
			IsEmulator = true;
			CurrentLocation = new MiNET.Utils.PlayerLocation(0,0,0);

			base.ChunkRadius = alex.GameSettings.RenderDistance;
		}

		public override void OnConnectionRequestAccepted()
		{
			ConnectionAcceptedWaitHandle.Set();
			base.OnConnectionRequestAccepted();
		}

		public bool IgnoreUnConnectedPong = false;
		protected override void OnUnconnectedPong(UnconnectedPong packet, IPEndPoint senderEndpoint)
		{
			KnownMotd = new BedrockMotd(packet.serverName);
			OnMotdReceivedHandler?.Invoke(this, KnownMotd);
			if (IgnoreUnConnectedPong) return;

			base.OnUnconnectedPong(packet, senderEndpoint);
		}

		public bool IsConnected => base.HaveServer;
		public IWorldReceiver WorldReceiver { get; set; }

		void INetworkProvider.EntityAction(int entityId, EntityAction action)
		{
			PlayerAction translated;
			switch (action)
			{
				case EntityAction.StartSneaking:
					translated = PlayerAction.StartSneak;
					break;
				case EntityAction.StopSneaking:
					translated = PlayerAction.StopSneak;
					break;

				case EntityAction.StartSprinting:
					translated = PlayerAction.StartSprint;
					break;
				case EntityAction.StopSprinting:
					translated = PlayerAction.StopSprint;
					break;

				default:
					return;
			}
			
			SendPlayerAction(translated, null, null);
		}

		void INetworkProvider.SendChatMessage(string message)
		{
			SendChat(message);
		}

		public void SendPlayerAction(PlayerAction action, BlockCoordinates? coordinates, int? blockFace )
		{
			McpePlayerAction packet = McpePlayerAction.CreateObject();
			packet.actionId = (int) action;
			
			if (coordinates.HasValue)
				packet.coordinates = new MiNET.Utils.BlockCoordinates(coordinates.Value.X, 
					coordinates.Value.Y, coordinates.Value.Z);

			if (blockFace.HasValue)
				packet.face = blockFace.Value;
			
			SendPacket(packet);
		}
		
	    public void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face)
	    {
		    if (status == DiggingStatus.Started)
		    {
			    SendPlayerAction(PlayerAction.StartBreak, position, (int)face);
		    }
		    else if (status == DiggingStatus.Finished)
		    {
			    SendPlayerAction(PlayerAction.StopBreak, position, (int)face);
		    }
		    else if (status == DiggingStatus.Cancelled)
		    {
			    SendPlayerAction(PlayerAction.AbortBreak, position, (int)face);
		    }
	    }

	    public void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
	    {
		    Log.Warn("TODO: Implement Block Placement");
	    }

	    public void UseItem(int hand)
		{
			Log.Warn("TODO: Implement UseItem");
		}

		public void HeldItemChanged(short slot)
		{
			Log.Warn("TODO: Implement Held Item Changed");
		}

		public void Close()
		{
			base.StopClient();
		}

		void IChatProvider.Send(string message)
		{
			SendChat(message);
		}

		void IChatProvider.RequestTabComplete(string text, out int transactionId)
		{
			transactionId = 0;
		}

		public void ChunkReceived(ChunkColumn chunkColumn)
		{
			WorldProvider.ChunkReceived(chunkColumn);
		}

		public void RequestChunkRadius(int radius)
		{
			var packet = McpeRequestChunkRadius.CreateObject();
			packet.chunkRadius = radius;

			base.SendPacket(packet);
		}
		
		public void Dispose()
		{
			StopClient();
		}
	}
}
