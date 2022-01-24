using System.IO;

namespace Alex.Networking.Java.Framework
{
	public interface IPacket<in TStream> where TStream : Stream
	{
		void Encode(TStream stream);

		void Decode(TStream stream);
	}
}