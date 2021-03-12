using System;

namespace Alex.Net.Bedrock
{
    public class SlidingWindow
    {
        public static readonly long CcMaximumThreshold = 2000;
        public static readonly long CcAdditionalVariance = 30;
        public static readonly long CcSyn = 10;
        
        private int MtuSize { get; }

        public double Cwnd { get; set; }
        public double SsThresh { get; set; }
        public double EstimatedRtt { get; set; } = -1;
        public double LastRtt { get; set; } = -1;
        public double DeviationRtt { get; set; } = -1;
        public long OldestUnsentAck { get; set; }
        public long NextCongestionControlBlock { get; set; }
        public bool BackoffThisBlock { get; set; }

        public SlidingWindow(int mtuSize)
        {
            MtuSize = mtuSize;
            Cwnd = mtuSize;
        }

        public int GetRetransmissionBandwidth(int unAckedBytes)
        {
            return unAckedBytes;
        }

        public int GetTransmissionBandwidth(int unAckedBytes)
        {
            if (unAckedBytes <= Cwnd)
            {
                return (int) (Cwnd - unAckedBytes);
            }
            else
            {
                return 0;
            }
        }

        public void OnPacketReceived(long curTime)
        {
            if (OldestUnsentAck == 0)
            {
                OldestUnsentAck = curTime;
            }
        }

        public void OnResend(long curSequenceIndex)
        {
            if (!BackoffThisBlock && Cwnd > MtuSize * 2)
            {
                SsThresh = Cwnd / 2;

                if (SsThresh < MtuSize)
                {
                    SsThresh = MtuSize;
                }

                Cwnd = MtuSize;

                NextCongestionControlBlock = curSequenceIndex;
                BackoffThisBlock = true;
            }
        }

        public void OnNak()
        {
            if (!BackoffThisBlock)
            {
                SsThresh = Cwnd / 2D;
            }
        }

        public void OnAck(long rtt, long sequenceIndex, long curSequenceIndex)
        {
            LastRtt = rtt;

            if (EstimatedRtt == -1)
            {
                EstimatedRtt = rtt;
                DeviationRtt = rtt;
            }
            else
            {
                double difference = rtt - EstimatedRtt;
                EstimatedRtt += 0.5D * difference;
                DeviationRtt += 0.5 * (Math.Abs(difference) - DeviationRtt);
            }

            bool isNewCongestionControlPeriod = sequenceIndex > NextCongestionControlBlock;

            if (isNewCongestionControlPeriod)
            {
                BackoffThisBlock = false;
                NextCongestionControlBlock = curSequenceIndex;
            }

            if (IsInSlowStart())
            {
                Cwnd += MtuSize;

                if (Cwnd > SsThresh && SsThresh != 0)
                {
                    Cwnd = SsThresh + MtuSize * MtuSize / Cwnd;
                }
            }
            else if (isNewCongestionControlPeriod)
            {
                Cwnd += MtuSize * MtuSize / Cwnd;
            }
        }

        public bool IsInSlowStart()
        {
            return Cwnd <= SsThresh || SsThresh == 0;
        }

        public void OnSendAck()
        {
            OldestUnsentAck = 0;
        }

        public long GetRtoForRetransmission()
        {
            if (EstimatedRtt == -1)
            {
                return CcMaximumThreshold;
            }

            long threshold = (long) ((2.0D * EstimatedRtt + 4.0D * DeviationRtt) + CcAdditionalVariance);

            return threshold > CcMaximumThreshold ? CcMaximumThreshold : threshold;
        }

        public double GetRtt()
        {
            return EstimatedRtt;
        }

        public bool ShouldSendAcks(long curTime)
        {
            long rto = GetSenderRtoForAck();

            return rto == -1 || curTime >= OldestUnsentAck + CcSyn;
        }

        public long GetSenderRtoForAck()
        {
            if (LastRtt == -1)
            {
                return -1;
            }
            else
            {
                return (long) (LastRtt + CcSyn);
            }
        }
    }
}