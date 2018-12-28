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
using MiNET.Client;
using MiNET.Net;
using MiNET.Utils;
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
		public ManualResetEventSlim ConnectionAcceptedWaitHandle { get; }
		public BedrockWorldProvider WorldProvider { get; }
		public EventHandler<BedrockMotd> OnMotdReceivedHandler;
		public BedrockMotd KnownMotd = new BedrockMotd(string.Empty);

		public BedrockClient(IPEndPoint endpoint, string username, DedicatedThreadPool threadPool, BedrockWorldProvider wp) : base(endpoint,
			username, threadPool)
		{
			WorldProvider = wp;
			ConnectionAcceptedWaitHandle = new ManualResetEventSlim(false);
			MessageDispatcher = new McpeClientMessageDispatcher(new BedrockClientPacketHandler(this));
			IsEmulator = true;
			CurrentLocation = new MiNET.Utils.PlayerLocation(0,0,0);
		}

		public override void OnConnectionRequestAccepted()
		{
			ConnectionAcceptedWaitHandle.Set();
			base.OnConnectionRequestAccepted();
		}

		public bool IgnoreUnConnectedPong = false;
		protected override void OnUnConnectedPong(UnconnectedPong packet, IPEndPoint senderEndpoint)
		{
			KnownMotd = new BedrockMotd(packet.serverName);
			OnMotdReceivedHandler?.Invoke(this, KnownMotd);
			if (IgnoreUnConnectedPong) return;

			base.OnUnConnectedPong(packet, senderEndpoint);
		}

		public bool IsConnected => base.HaveServer;
		public IWorldReceiver WorldReceiver { get; set; }

		void INetworkProvider.EntityAction(int entityId, EntityAction action)
		{
			
		}

		void INetworkProvider.SendChatMessage(string message)
		{
			SendChat(message);
		}

	    public void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
	    {
	        throw new NotImplementedException();
	    }

		public void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face)
		{
			throw new NotImplementedException();
		}

		public void UseItem(int hand)
		{
			throw new NotImplementedException();
		}

		public void HeldItemChanged(short slot)
		{
			throw new NotImplementedException();
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

		public void Dispose()
		{
			StopClient();
		}
	}
}
