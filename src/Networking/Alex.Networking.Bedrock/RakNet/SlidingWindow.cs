using System;
using System.Threading;
using NLog;

namespace Alex.Networking.Bedrock.RakNet
{
	public class SlidingWindow : ICongestionManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SlidingWindow));
		private const long UNSET_TIME_US = -1;
		public static readonly long CcMaximumThreshold = 2000;
		public static readonly long CcAdditionalVariance = 30;
		public static readonly long CcSyn = 10;

		private int MtuSize { get; }

		private double _cwnd;

		/// <summary>
		///      Max bytes on wire
		/// </summary>
		public double CongestionWindow
		{
			get => _cwnd;
			private set
			{
				_cwnd = value;
			}
		}

		/// <summary>
		///      Threshold between slow start and congestion avoidance
		/// </summary>
		public double SlowStartThreshold { get; private set; } = 0;

		/// <summary>
		///     The estimated Round Trip Time
		/// </summary>
		public double EstimatedRtt { get; private set; } = -1;

		/// <summary>
		///     The last known Round Trip Time
		/// </summary>
		public long LastRtt { get; private set; } = -1;

		/// <summary>
		///     The amount the Round Trip Time is allowed to deviate
		/// </summary>
		public double DeviationRtt { get; private set; } = -1;

		/// <summary>
		/// When we get an ack, if oldestUnsentAck==0, set it to the current time
		/// When we send out acks, set oldestUnsentAck to 0
		/// </summary>
		public long OldestUnsentAck { get; private set; } = 0;

		/// <summary>
		///     Every outgoing datagram is assigned a sequence number, which increments by 1 every assignment
		/// </summary>
		private long _nextDatagramSequenceNumber = 0;

		/// <summary>
		/// Track which datagram sequence numbers have arrived.
		/// If a sequence number is skipped, send a NAK for all skipped messages
		/// </summary>
		private long _expectedNextSequenceNumber = 0;

		public long NextCongestionControlBlock { get; private set; }
		public bool BackoffThisBlock { get; private set; }
		public bool IsContinuousSend { get; private set; } = false;

		public SlidingWindow(int mtuSize)
		{
			MtuSize = mtuSize;
			CongestionWindow = mtuSize;
		}

		public int GetRetransmissionBandwidth(long currentTime, int unAckedBytes)
		{
			if (currentTime - _lastPacketReceived >= GetRtoForRetransmission())
				return 0;

			return unAckedBytes;
		}

		public int GetTransmissionBandwidth(int unAckedBytes, bool isContinuousSend)
		{
			IsContinuousSend = isContinuousSend;

			if (unAckedBytes <= CongestionWindow)
			{
				return (int)(CongestionWindow - unAckedBytes);
			}
			else
			{
				return 0;
			}
		}

		private long _lastPacketReceived = 0;

		public bool OnPacketReceived(long currentTime,
			long datagramSequenceNumber,
			bool isContinuousSend,
			int sizeInBytes,
			out long skippedMessageCount)
		{
			if (OldestUnsentAck == 0)
				OldestUnsentAck = currentTime;

			_lastPacketReceived = currentTime;

			var expected = Interlocked.CompareExchange(
				ref _expectedNextSequenceNumber, datagramSequenceNumber + 1, datagramSequenceNumber);

			if (expected == datagramSequenceNumber)
			{
				skippedMessageCount = 0;
			}
			else if (GreaterThan(datagramSequenceNumber, expected))
			{
				skippedMessageCount = datagramSequenceNumber - expected;

				// Sanity check, just use timeout resend if this was really valid
				if (skippedMessageCount > 1000)
				{
					// During testing, the nat punchthrough server got 51200 on the first packet. I have no idea where this comes from, but has happened twice
					if (skippedMessageCount > 50000)
						return false;

					skippedMessageCount = 1000;
				}

				Interlocked.Exchange(ref _expectedNextSequenceNumber, datagramSequenceNumber + 1);
			}
			else
			{
				skippedMessageCount = 0;
			}

			return true;
		}

		public void OnResend(long currentTime, long nextActionTime)
		{
			if (IsContinuousSend && !BackoffThisBlock && CongestionWindow > MtuSize * 2)
			{
				SlowStartThreshold = CongestionWindow / 2;

				if (SlowStartThreshold < MtuSize)
				{
					SlowStartThreshold = MtuSize;
				}

				CongestionWindow = MtuSize;

				NextCongestionControlBlock = Interlocked.Read(ref _nextDatagramSequenceNumber);
				BackoffThisBlock = true;

				Log.Info($"(Resend) Enter slow start. SlowStartThreshold={SlowStartThreshold:F2} Cwnd={CongestionWindow:F2}");
			}
		}

		/// <summary>
		///     Call when you get a NAK, with the sequence number of the lost message
		///     Affects the congestion control
		/// </summary>
		public void OnNak(long currentTime, long nakSequenceNumber)
		{
			if (IsContinuousSend && !BackoffThisBlock)
			{
				SlowStartThreshold = CongestionWindow / 2D;

				Log.Info($"Set congestion avoidance. SlowStartThreshold={SlowStartThreshold:F2} Cwnd={CongestionWindow:F2}");
			}
		}

		public long GetAndIncrementNextDatagramSequenceNumber()
		{
			return Interlocked.Increment(ref _nextDatagramSequenceNumber);
		}

		/// <summary>
		/// Call this when an ACK arrives.
		/// hasBAndAS are possibly written with the ack, see OnSendAck()
		/// B and AS are used in the calculations in UpdateWindowSizeAndAckOnAckPerSyn
		/// B and AS are updated at most once per SYN 
		/// </summary>
		/// <param name="currentTime"></param>
		/// <param name="rtt"></param>
		/// <param name="sequenceIndex"></param>
		/// <param name="isContinuousSend"></param>
		public void OnAck(long currentTime, long rtt, long sequenceIndex, bool isContinuousSend)
		{
			if (OldestUnsentAck == 0)
			{
				OldestUnsentAck = currentTime;
			}

			LastRtt = rtt;

			if (EstimatedRtt == UNSET_TIME_US)
			{
				EstimatedRtt = rtt;
				DeviationRtt = rtt;
			}
			else
			{
				double difference = rtt - EstimatedRtt;
				EstimatedRtt += 0.05D * difference;
				DeviationRtt += 0.05d * (Math.Abs(difference) - DeviationRtt);
			}

			IsContinuousSend = isContinuousSend;

			if (!isContinuousSend)
				return;

			bool isNewCongestionControlPeriod = GreaterThan(sequenceIndex, NextCongestionControlBlock);

			if (isNewCongestionControlPeriod)
			{
				BackoffThisBlock = false;
				//speedUpThisBlock = false;
				NextCongestionControlBlock = Interlocked.Read(ref _nextDatagramSequenceNumber);
			}

			if (IsInSlowStart())
			{
				CongestionWindow += MtuSize;

				if (CongestionWindow > SlowStartThreshold && SlowStartThreshold != 0)
					CongestionWindow = SlowStartThreshold + MtuSize * MtuSize / CongestionWindow;

				Log.Info($"Slow start increase... SlowStartThreshold={SlowStartThreshold:F2} Cwnd={CongestionWindow:F2}");
			}
			else if (isNewCongestionControlPeriod)
			{
				CongestionWindow += MtuSize * MtuSize / CongestionWindow;

				Log.Info($"Congestion avoidance increase... SlowStartThreshold={SlowStartThreshold:F2} Cwnd={CongestionWindow:F2}");
			}
		}

		private bool GreaterThan(long a, long b)
		{
			var halfSpan = 0.5;

			return b != a && b - a < halfSpan;
		}

		public bool IsInSlowStart()
		{
			return CongestionWindow <= SlowStartThreshold || SlowStartThreshold == 0;
		}

		/// <summary>
		/// Call when we send a NACK
		/// Also updates SND, the period between sends, since data is written out
		/// </summary>
		public void OnSendNack() { }

		/// <summary>
		/// Call when we send an ack, to write B and AS if needed
		/// B and AS are only written once per SYN, to prevent slow calculations
		/// Also updates SND, the period between sends, since data is written out
		/// Be sure to call OnSendAckGetBAndAS() before calling OnSendAck(), since whether you write it or not affects \a numBytes
		/// </summary>
		public void OnSendAck()
		{
			OldestUnsentAck = 0;
		}

		/// <summary>
		/// Retransmission time out for the sender
		/// If the time difference between when a message was last transmitted, and the current time is greater than RTO then packet is eligible for retransmission, pending congestion control
		/// RTO = (RTT + 4 * RTTVar) + SYN
		/// If we have been continuously sending for the last RTO, and no ACK or NAK at all, SND*=2;
		/// This is per message, which is different from UDT, but RakNet supports packetloss with continuing data where UDT is only RELIABLE_ORDERED
		/// Minimum value is 100 milliseconds
		/// </summary>
		/// <returns></returns>
		public long GetRtoForRetransmission()
		{
			if (EstimatedRtt == UNSET_TIME_US)
			{
				return CcMaximumThreshold;
			}

			long threshold = (long)((2.0 * EstimatedRtt + 4.0 * DeviationRtt) + CcAdditionalVariance);

			return (threshold > CcMaximumThreshold ? CcMaximumThreshold : threshold);
		}

		public double GetRtt()
		{
			if (LastRtt == UNSET_TIME_US)
				return 0;

			return LastRtt;
		}

		/// <summary>
		///     Acks do not have to be sent immediately. Instead, they can be buffered up such that groups of acks are sent at a time
		///     This reduces overall bandwidth usage
		///     How long they can be buffered depends on the retransmit time of the sender
		///     Should call once per update tick, and send if needed
		/// </summary>
		/// <param name="curTime"></param>
		/// <returns></returns>
		public bool ShouldSendAcks(long curTime)
		{
			long rto = GetSenderRtoForAck();

			if (rto == UNSET_TIME_US)
				return true;

			return curTime >= OldestUnsentAck + CcSyn;
		}

		public long GetSenderRtoForAck()
		{
			if (LastRtt == UNSET_TIME_US)
			{
				return UNSET_TIME_US;
			}
			else
			{
				return (long)(LastRtt + CcSyn);
			}
		}

		public void OnGotPacketPair(int sequenceNumber) { }
	}
}