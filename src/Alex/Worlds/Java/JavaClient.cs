using System;
using System.Net.Sockets;
using Alex.API;
using Alex.API.Entities;
using Alex.API.Items;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Play;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ConnectionState = Alex.Networking.Java.ConnectionState;
using Packet = Alex.Networking.Java.Packets.Packet;

namespace Alex.Worlds.Java
{
	public class JavaClient : NetConnection
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaClient));

		private IJavaProvider WorldReceiver { get; }
		public JavaClient(JavaWorldProvider javaWorldProvider, Socket socket) : base(Direction.ClientBound, socket, null, null)
		{
			MCPacketFactory.Load();

			WorldReceiver = javaWorldProvider;
		}

		protected override void HandlePacket(Packet packet)
		{
			if (packet == null) return;

			switch (ConnectionState)
			{
				case ConnectionState.Handshake:
					WorldReceiver.HandleHandshake(packet);
					break;
				case ConnectionState.Status:
					WorldReceiver.HandleStatus(packet);
					break;
				case ConnectionState.Login:
					WorldReceiver.HandleLogin(packet);
					break;
				case ConnectionState.Play:
					WorldReceiver.HandlePlay(packet);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
