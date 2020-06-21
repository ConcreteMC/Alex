using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Alex.API.Utils
{
    public static class StreamExtensions
    {
        public static Memory<byte> ReadToMemory(this Stream stream, long length)
        {
            Memory<byte> buffer = new Memory<byte>(new byte[length]);
            stream.Read(buffer.Span);

            return buffer;
        }
        
        public static ReadOnlySpan<byte> ReadToSpan(this Stream stream, long length)
        {
            Span<byte> buffer = new Span<byte>(new byte[length]);
            stream.Read(buffer);

            return buffer;
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
    }
}