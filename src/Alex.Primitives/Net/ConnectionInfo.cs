using System;

namespace Alex.Interfaces.Net
{
	public class ConnectionInfo
	{
		public DateTime ConnectionOpenedTime { get; }
		public long Latency;

		public long Nak;
		public long NakSent;

		public long Ack;
		public long AckSent;

		public long Fails;
		public long Resends;

		public long BytesIn;
		public long BytesOut;

		public long PacketsIn;
		public long PacketsOut;

		public long PacketLoss
		{
			get
			{
				var packets = PacketsIn + PacketsOut;

				if (packets == 0)
					return 0;

				return (100 / packets) * (Nak + NakSent);
			}
		}

		public NetworkState State { get; }

		public ConnectionInfo(DateTime connectionOpenedTime,
			long latency,
			long nack,
			long ack,
			long acksSent,
			long fails,
			long resends,
			long bytesIn,
			long bytesOut,
			long packetsIn,
			long packetsOut,
			NetworkState state = NetworkState.Ok)
		{
			ConnectionOpenedTime = connectionOpenedTime;
			Latency = latency;
			Nak = nack;
			Ack = ack;
			AckSent = acksSent;
			Fails = fails;
			Resends = resends;
			BytesIn = bytesIn;
			BytesOut = bytesOut;
			PacketsIn = packetsIn;
			PacketsOut = packetsOut;
			State = state;
		}

		public ConnectionInfo() { }

		public enum NetworkState
		{
			Ok,
			OutOfOrder,
			Slow,
			HighPing,
			PacketLoss
		}
	}
}