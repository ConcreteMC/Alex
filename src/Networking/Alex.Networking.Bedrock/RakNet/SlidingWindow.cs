using System;
using System.Threading;
using NLog;

namespace Alex.Networking.Bedrock.RakNet
{
    public class SlidingWindow
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SlidingWindow));
        private const long UNSET_TIME_US = -1;
        public static readonly long CcMaximumThreshold = 2000;
        public static readonly long CcAdditionalVariance = 30;
        public static readonly long CcSyn = 10;
        
        private int MtuSize { get; }

        /// <summary>
        ///      Max bytes on wire
        /// </summary>
        public double Cwnd
        {
            get => _cwnd;
            private set
            {
                _cwnd = Math.Clamp(value, 0, MtuSize);
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
        private long  _nextDatagramSequenceNumber = 0;

        /// <summary>
        /// Track which datagram sequence numbers have arrived.
        /// If a sequence number is skipped, send a NAK for all skipped messages
        /// </summary>
        private long _expectedNextSequenceNumber = 0;

        private double _cwnd;

        public long NextCongestionControlBlock { get; private set; }
        public bool BackoffThisBlock { get; private set; }
        public bool IsContinuousSend { get; private set; } = false;

        public SlidingWindow(int mtuSize)
        {
            MtuSize = mtuSize;
            Cwnd = mtuSize;
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
            
            if (unAckedBytes <= Cwnd)
            {
                return (int) (Cwnd - unAckedBytes);
            }
            else
            {
                return 0;
            }
        }

        private long _lastPacketReceived = 0;
        public bool OnPacketReceived(long currentTime, long datagramSequenceNumber, bool isContinuousSend, int sizeInBytes, out long skippedMessageCount)
        {
            _lastPacketReceived = currentTime;
            if (datagramSequenceNumber == _expectedNextSequenceNumber)
             {
                 skippedMessageCount=0;
                 _expectedNextSequenceNumber=datagramSequenceNumber+1;
             }
             else if (datagramSequenceNumber > _expectedNextSequenceNumber)
             {
                 skippedMessageCount = datagramSequenceNumber - _expectedNextSequenceNumber;
                 // Sanity check, just use timeout resend if this was really valid
                 if (skippedMessageCount > 1000)
                 {
                     // During testing, the nat punchthrough server got 51200 on the first packet. I have no idea where this comes from, but has happened twice
                     if (skippedMessageCount > 50000)
                         return false;
                     
                     skippedMessageCount=1000;
                 }
                 _expectedNextSequenceNumber = datagramSequenceNumber + 1;
             }
             else
             {
                 skippedMessageCount = 0;
             }
 
             return true;
        }

        public void OnResend(long currentTime, long nextActionTime)
        {
            if (IsContinuousSend && !BackoffThisBlock && Cwnd > MtuSize * 2)
            {
                SlowStartThreshold = Cwnd / 2;

                if (SlowStartThreshold < MtuSize)
                {
                    SlowStartThreshold = MtuSize;
                }

                Cwnd = MtuSize;

                NextCongestionControlBlock = Interlocked.Read(ref _nextDatagramSequenceNumber);
                BackoffThisBlock = true;
                
                Log.Info($"(Resend) Enter slow start. Cwnd={Cwnd:F2}");
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
                SlowStartThreshold = Cwnd / 2D;
                
                Log.Info($"Set congestion avoidance. Cwnd={Cwnd:F2}, SlowStartThreshold={SlowStartThreshold:F2}, NAKSequenceNumber={nakSequenceNumber}");
            }
        }

        public long GetAndIncrementNextDatagramSequenceNumber()
        {
            return Interlocked.Increment(ref _nextDatagramSequenceNumber)  -1;
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

            if (EstimatedRtt < 0d)
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
            
            bool isNewCongestionControlPeriod = sequenceIndex > NextCongestionControlBlock;

            if (isNewCongestionControlPeriod)
            {
                BackoffThisBlock = false;
              //  speedUpThisBlock = false;
                NextCongestionControlBlock = Interlocked.Read(ref _nextDatagramSequenceNumber);
            }

            if (IsInSlowStart())
            {
                if (Cwnd < MtuSize)
                {
                    Cwnd += MtuSize;

                    if (Cwnd > SlowStartThreshold && SlowStartThreshold > 0)
                    {
                        Cwnd = SlowStartThreshold + MtuSize * MtuSize / Cwnd;
                    }

                    Log.Info($"Slow start increase... Cwnd={Cwnd:F2}");
                }
            }
            else if (isNewCongestionControlPeriod)
            {
                Cwnd += MtuSize * MtuSize / Cwnd;
                
                Log.Info($"Congestion avoidance increase... Cwnd={Cwnd:F2}");
            }
        }

        public bool IsInSlowStart()
        {
            return Cwnd <= SlowStartThreshold || SlowStartThreshold <= 0;
        }

        /// <summary>
        /// Call when we send a NACK
        /// Also updates SND, the period between sends, since data is written out
        /// </summary>
        public void OnSendNack()
        {
            
        }
        
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
            if (EstimatedRtt < 0d)
            {
                return CcMaximumThreshold;
            }

            long threshold = (long) ((2.0D * EstimatedRtt + 4.0D * DeviationRtt) + CcAdditionalVariance);

            return (threshold > CcMaximumThreshold ? CcMaximumThreshold : threshold);
        }

        public double GetRtt()
        {
            if (LastRtt == UNSET_TIME_US)
                return 0;

            return LastRtt;
            return EstimatedRtt;
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
                return (long) (LastRtt + CcSyn);
            }
        }

        public void OnGotPacketPair(int sequenceNumber)
        {
            
        }
    }
}