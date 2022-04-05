using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Alex.ResourcePackLib
{

	public static class StreamExtensions
	{
		public static IEnumerable<IEnumerable<T>> Batch<T>( 
			this IEnumerable<T> source, int batchSize) 
		{ 
			using (var enumerator = source.GetEnumerator()) 
				while (enumerator.MoveNext()) 
					yield return YieldBatchElements(enumerator, batchSize - 1); 
		} 

		private static IEnumerable<T> YieldBatchElements<T>( 
			IEnumerator<T> source, int batchSize) 
		{ 
			yield return source.Current; 
			for (int i = 0; i < batchSize && source.MoveNext(); i++) 
				yield return source.Current; 
		}

		public static unsafe ReadOnlySpan<byte> ReadToEnd(this Stream stream, int bufferSize = 256)
		{
			var ptr = Marshal.AllocHGlobal(bufferSize);

			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					Span<byte> buffer = new Span<byte>(ptr.ToPointer(), bufferSize);
					int read;

					do
					{
						read = stream.Read(buffer);
						ms.Write(buffer.Slice(0, read));
					} while (read > 0);

					return ms.ToArray();
				}
			}
			finally
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		public static Span<byte> ReadToSpan(this Stream stream, long length)
		{
			Span<byte> buffer = new Span<byte>(new byte[length]);
			stream.Read(buffer);

			return buffer;
		}
	}
}