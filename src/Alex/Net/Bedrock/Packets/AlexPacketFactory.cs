using System;
using MiNET.Net;
using MiNET.Net.RakNet;
using NLog;

namespace Alex.Net.Bedrock.Packets
{
	public class AlexPacketFactory : ICustomPacketFactory
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AlexPacketFactory));

		public Packet Create(byte messageId, ReadOnlyMemory<byte> buffer, string ns)
		{
			if (ns == "raknet")
				return CreateRakNet(messageId, buffer);

			return CreatePe(messageId, buffer);
		}


		private Packet CreateRakNet(byte messageId, ReadOnlyMemory<byte> buffer)
		{
			Packet packet = null;

			switch (messageId)
			{
				//Hack to spawn on the Hive
				//case 0x84:
				//    packet = Datagram.CreateObject();
				//    break;
				case 16:
					packet = ConnectionRequestAccepted.CreateObject();

					break;
			}

			try
			{
				packet?.Decode(buffer);
			}
			catch (System.ArgumentOutOfRangeException ex)
			{
				Log.Warn(ex, $"RakNet processing: {ex.ToString()}");
			}

			return packet;
		}

		private Packet CreatePe(byte messageId, ReadOnlyMemory<byte> buffer)
		{
			Packet packet = null;

			try
			{
				switch (messageId)
				{
					case 0x97:
						packet = new McpeUpdateGm();

						break;
					// case 0x8a:
					//     packet = McpeEmote.CreateObject();
					//     break;
					// case 0x9e:
					//    packet = McpeAnimateEntity.CreateObject();
					//   break;
				}


				//packet?.Decode(buffer);
			}
			catch (Exception ex)
			{
				//  if (messageId != 0x31)
				{
					Log.Error(ex, $"Processing error.");
				}
			}

			return packet;
		}
	}
}