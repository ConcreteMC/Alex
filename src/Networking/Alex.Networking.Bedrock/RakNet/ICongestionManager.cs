namespace Alex.Networking.Bedrock.RakNet
{
	public interface ICongestionManager
	{
		/// <summary>
		///      Max bytes on wire
		/// </summary>
		double CongestionWindow { get; }

		/// <summary>
		///      Threshold between slow start and congestion avoidance
		/// </summary>
		double SlowStartThreshold { get; }

		/// <summary>
		///     The estimated Round Trip Time
		/// </summary>
		double EstimatedRtt { get; }

		/// <summary>
		///     The last known Round Trip Time
		/// </summary>
		long LastRtt { get; }

		/// <summary>
		///     The amount the Round Trip Time is allowed to deviate
		/// </summary>
		double DeviationRtt { get; }

		/// <summary>
		/// When we get an ack, if oldestUnsentAck==0, set it to the current time
		/// When we send out acks, set oldestUnsentAck to 0
		/// </summary>
		long OldestUnsentAck { get; }

		long NextCongestionControlBlock { get; }
		bool BackoffThisBlock { get; }
		bool IsContinuousSend { get; }

		int GetRetransmissionBandwidth(long currentTime, int unAckedBytes);

		int GetTransmissionBandwidth(int unAckedBytes, bool isContinuousSend);

		bool OnPacketReceived(long currentTime,
			long datagramSequenceNumber,
			bool isContinuousSend,
			int sizeInBytes,
			out long skippedMessageCount);

		void OnResend(long currentTime, long nextActionTime);

		/// <summary>
		///     Call when you get a NAK, with the sequence number of the lost message
		///     Affects the congestion control
		/// </summary>
		void OnNak(long currentTime, long nakSequenceNumber);

		long GetAndIncrementNextDatagramSequenceNumber();

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
		void OnAck(long currentTime, long rtt, long sequenceIndex, bool isContinuousSend);

		bool IsInSlowStart();

		/// <summary>
		/// Call when we send a NACK
		/// Also updates SND, the period between sends, since data is written out
		/// </summary>
		void OnSendNack();

		/// <summary>
		/// Call when we send an ack, to write B and AS if needed
		/// B and AS are only written once per SYN, to prevent slow calculations
		/// Also updates SND, the period between sends, since data is written out
		/// Be sure to call OnSendAckGetBAndAS() before calling OnSendAck(), since whether you write it or not affects \a numBytes
		/// </summary>
		void OnSendAck();

		/// <summary>
		/// Retransmission time out for the sender
		/// If the time difference between when a message was last transmitted, and the current time is greater than RTO then packet is eligible for retransmission, pending congestion control
		/// RTO = (RTT + 4 * RTTVar) + SYN
		/// If we have been continuously sending for the last RTO, and no ACK or NAK at all, SND*=2;
		/// This is per message, which is different from UDT, but RakNet supports packetloss with continuing data where UDT is only RELIABLE_ORDERED
		/// Minimum value is 100 milliseconds
		/// </summary>
		/// <returns></returns>
		long GetRtoForRetransmission();

		double GetRtt();

		/// <summary>
		///     Acks do not have to be sent immediately. Instead, they can be buffered up such that groups of acks are sent at a time
		///     This reduces overall bandwidth usage
		///     How long they can be buffered depends on the retransmit time of the sender
		///     Should call once per update tick, and send if needed
		/// </summary>
		/// <param name="curTime"></param>
		/// <returns></returns>
		bool ShouldSendAcks(long curTime);

		long GetSenderRtoForAck();

		void OnGotPacketPair(int sequenceNumber);
	}
}