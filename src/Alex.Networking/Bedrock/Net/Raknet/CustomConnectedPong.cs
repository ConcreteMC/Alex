using System;
using MiNET.Net;

namespace Alex.Networking.Bedrock.Net.Raknet
{
	public class CustomConnectedPong : ConnectedPong
	{
		public static bool CanPing { get; set; } = true;
		public static long Latency { get; set; } = 0;
		public static long LastSentPing { get; set; } = 0;
		
		public CustomConnectedPong()
		{
			
		}

		/// <inheritdoc />
		protected override void DecodePacket()
		{
			try
			{
				base.DecodePacket();

				Latency = (DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerMillisecond) - sendpingtime;
			}
			catch
			{
				CanPing = false;
			}
		}
	}
}