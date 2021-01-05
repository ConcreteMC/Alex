using System;
using MiNET.Net;

namespace Alex.Net.Bedrock.Raknet
{
	public class CustomConnectedPong : ConnectedPong
	{
		public CustomConnectedPong()
		{
			
		}

		/// <inheritdoc />
		protected override void DecodePacket()
		{
			try
			{
				base.DecodePacket();

			//	Latency = (DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerMillisecond) - sendpingtime;
			}
			catch
			{
				//CanPing = false;
			}
		}
	}
}