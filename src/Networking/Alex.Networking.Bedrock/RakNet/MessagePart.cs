using System;
using MiNET.Net;
using MiNET.Net.RakNet;
using NLog;

namespace Alex.Networking.Bedrock.RakNet
{
	public class MessagePart : Packet<MessagePart> // Replace this with stream
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public Memory<byte> Buffer { get; set; }
		public byte ContainedMessageId { get; set; }
		
		public override void Reset()
		{
			base.Reset();
			ReliabilityHeader.Reset();
			ContainedMessageId = 0;
			Buffer = null;
		}

		protected override void EncodePacket()
		{
			// DO NOT CALL base.EncodePackage();

			Memory<byte> encodedMessage = Buffer;

			if (encodedMessage.Length == 0) Log.Error("Bad size 0 in message part");
			//if(ReliabilityHeader.Reliability != Reliability.ReliableOrdered) Log.Warn($"Sending message with reliability={ReliabilityHeader.Reliability}");

			byte flags = (byte) (((byte) ReliabilityHeader.Reliability) << 5);
			Write((byte) (flags | (ReliabilityHeader.HasSplit ? 0b00010000 : 0x00)));
			Write((short) (encodedMessage.Length * 8), true); // bit length

			switch (ReliabilityHeader.Reliability)
			{
				case Reliability.Reliable:
				case Reliability.ReliableOrdered:
				case Reliability.ReliableSequenced:
				case Reliability.ReliableWithAckReceipt:
				case Reliability.ReliableOrderedWithAckReceipt:
					Write(ReliabilityHeader.ReliableMessageNumber);

					break;
			}

			//switch (ReliabilityHeader.Reliability)
			//{
			//	case Reliability.UnreliableSequenced:
			//	case Reliability.ReliableSequenced:
			//		ReliabilityHeader.SequencingIndex = WriteLittle();
			//		break;
			//}

			switch (ReliabilityHeader.Reliability)
			{
				case Reliability.UnreliableSequenced:
				case Reliability.ReliableOrdered:
				case Reliability.ReliableSequenced:
				case Reliability.ReliableOrderedWithAckReceipt:
					Write(ReliabilityHeader.OrderingIndex);
					Write(ReliabilityHeader.OrderingChannel);

					break;
			}

			if (ReliabilityHeader.HasSplit)
			{
				Write(ReliabilityHeader.PartCount, true);
				Write(ReliabilityHeader.PartId, true);
				Write(ReliabilityHeader.PartIndex, true);
			}

			// Message body

			Write(encodedMessage);
		}
	}
}