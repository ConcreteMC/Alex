using System;

namespace Alex.Networking.Java.Util
{
	public static class EndianUtils
	{
		public static double NetworkToHostOrder(byte[] data)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(data);
			}

			return BitConverter.ToDouble(data, 0);
		}

		public static float NetworkToHostOrder(float network)
		{
			var bytes = BitConverter.GetBytes(network);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return BitConverter.ToSingle(bytes, 0);
		}

		public static ushort[] NetworkToHostOrder(ushort[] network)
		{
			if (BitConverter.IsLittleEndian)
				Array.Reverse(network);

			return network;
		}

		public static ushort NetworkToHostOrder(ushort network)
		{
			var net = BitConverter.GetBytes(network);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(net);

			return BitConverter.ToUInt16(net, 0);
		}

		public static ulong NetworkToHostOrder(ulong network)
		{
			var net = BitConverter.GetBytes(network);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(net);

			return BitConverter.ToUInt64(net, 0);
		}

		public static byte[] HostToNetworkOrder(double d)
		{
			var data = BitConverter.GetBytes(d);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);

			return data;
		}

		public static byte[] HostToNetworkOrder(float host)
		{
			var bytes = BitConverter.GetBytes(host);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return bytes;
		}

		public static byte[] HostToNetworkOrderLong(ulong host)
		{
			var bytes = BitConverter.GetBytes(host);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			return bytes;
		}
	}
}