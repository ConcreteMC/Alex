using System;
using System.Collections.Generic;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;

namespace Alex.Net.Bedrock.Raknet
{
	public class CustomNak : Packet<CustomNak>
	{
		public List<Tuple<int, int>> ranges = new List<Tuple<int, int>>();
		public List<int>             naks   = new List<int>();
		public CustomNak()
		{
			Id = 0xa0;
		}
		
		protected override void DecodePacket()
		{
			base.DecodePacket();

			if (Id != 0xa0) throw new Exception("Not NAK");
			ranges.Clear();

			short count = ReadShort(true);
			for (int i = 0; i < count; i++)
			{
				var onlyOneSequence = ReadByte();
				if (onlyOneSequence == 0)
				{
					int start = ReadLittle().IntValue();
					int end   = ReadLittle().IntValue();
					if (end - start > 512) end = start + 512;

					var range = new Tuple<int, int>(start, end);
					ranges.Add(range);
				}
				else
				{
					int seqNo = ReadLittle().IntValue();
					var range = new Tuple<int, int>(seqNo, seqNo);
					ranges.Add(range);
				}
			}
		}

		/// <inheritdoc />
		protected override void EncodePacket()
		{
			base.EncodePacket();

			if (ranges.Count == 0 && naks.Count > 0)
			{
				ranges = Acks.Slize(naks);
			}
			
			Write((short) ranges.Count, true);

			foreach (var range in ranges)
			{
				if (range.Item1 == range.Item2)
				{
					Write((byte) 1);
					Write(new Int24(range.Item1));
				}
				else
				{
					Write((byte) 0);
					Write(new Int24(range.Item1));
					Write(new Int24(range.Item2));
				}
			}
		}
	}
}