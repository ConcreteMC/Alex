using MiNET.Net.RakNet;

namespace Alex.Networking.Bedrock.RakNet
{
	public class InternalDatagramHeader : DatagramHeader
	{
		private static bool GetBit(byte b, int bitNumber)
		{
			return ((b >> bitNumber) & 1) != 0;
		}

		public InternalDatagramHeader(byte header)
		{
			IsValid = GetBit(header, 7);
			IsAck = GetBit(header, 6);

			if (IsValid)
			{
				if (IsAck)
				{
					IsNak = false;
					IsPacketPair = false;
					HasBAndAs = GetBit(header, 5);
				}
				else
				{
					IsNak = GetBit(header, 5);

					if (IsNak)
					{
						IsPacketPair = false;
					}
					else
					{
						IsPacketPair = GetBit(header, 4);
						IsContinuousSend = GetBit(header, 3);
						NeedsBAndAs = GetBit(header, 2);
					}
				}
			}
		}

		public InternalDatagramHeader() : this(0) { }

		/// <inheritdoc />
		public override string ToString()
		{
			return
				$"{{IsContinuousSend={IsContinuousSend}, IsPacketPair={IsPacketPair}, NeedsBAndAs={NeedsBAndAs}, IsACK={IsAck}, IsNAK={IsNak}, IsValid={IsValid}}}";
		}
	}
}