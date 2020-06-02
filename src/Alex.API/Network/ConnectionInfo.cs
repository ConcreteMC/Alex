using System;

namespace Alex.API.Network
{
    public class ConnectionInfo
    {
        public DateTime ConnectionOpenedTime { get; }
        public long Latency { get; set; }
        public long Nack { get; set; }
        public long Ack { get; set; }
        public long AckSent { get; set; }
        public long Fails { get; set; }
        public long Resends { get; set; }
        
        public long BytesIn { get; set; }
        public long BytesOut { get; set; }
        
        public ConnectionInfo(DateTime connectionOpenedTime, long latency, long nack, long ack, long acksSent, long fails, long resends, long bytesIn, long bytesOut)
        {
            ConnectionOpenedTime = connectionOpenedTime;
            Latency = latency;
            Nack = nack;
            Ack = ack;
            AckSent = acksSent;
            Fails = fails;
            Resends = resends;
            BytesIn = bytesIn;
            BytesOut = bytesOut;
        }
    }
}