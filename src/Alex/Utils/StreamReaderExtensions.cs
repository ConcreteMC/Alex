using System;
using System.IO;
using System.Reflection;

namespace Alex.Utils
{
	/// <summary>Useful <see cref="StreamReader"/> extentions.</summary>
	public static class StreamReaderExtentions
	{
		/// <summary>Gets the position within the <see cref="StreamReader.BaseStream"/> of the <see cref="StreamReader"/>.</summary>
		/// <remarks><para>This method is quite slow. It uses reflection to access private <see cref="StreamReader"/> fields. Don't use it too often.</para></remarks>
		/// <param name="streamReader">Source <see cref="StreamReader"/>.</param>
		/// <exception cref="ArgumentNullException">Occurs when passed <see cref="StreamReader"/> is null.</exception>
		/// <returns>The current position of this stream.</returns>
		public static long GetPosition(this StreamReader streamReader)
		{
			if (streamReader == null)
				throw new ArgumentNullException("streamReader");

			var charBuffer = (char[])streamReader.GetType().InvokeMember(
				"charBuffer",
				BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
				null, streamReader, null);

			var charPos = (int)streamReader.GetType().InvokeMember(
				"charPos",
				BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
				null, streamReader, null);

			var charLen = (int)streamReader.GetType().InvokeMember(
				"charLen",
				BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
				null, streamReader, null);

			var offsetLength = streamReader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);

			return streamReader.BaseStream.Position - offsetLength;
		}

		/// <summary>Sets the position within the <see cref="StreamReader.BaseStream"/> of the <see cref="StreamReader"/>.</summary>
		/// <remarks>
		/// <para><see cref="StreamReader.BaseStream"/> should be seekable.</para>
		/// <para>This method is quite slow. It uses reflection and flushes the charBuffer of the <see cref="StreamReader.BaseStream"/>. Don't use it too often.</para>
		/// </remarks>
		/// <param name="streamReader">Source <see cref="StreamReader"/>.</param>
		/// <param name="position">The point relative to origin from which to begin seeking.</param>
		/// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for origin, using a value of type <see cref="SeekOrigin"/>. </param>
		/// <exception cref="ArgumentNullException">Occurs when passed <see cref="StreamReader"/> is null.</exception>
		/// <exception cref="ArgumentException">Occurs when <see cref="StreamReader.BaseStream"/> is not seekable.</exception>
		/// <returns>The new position in the stream. This position can be different to the <see cref="position"/> because of the preamble.</returns>
		public static long Seek(this StreamReader streamReader, long position, SeekOrigin origin)
		{
			if (streamReader == null)
				throw new ArgumentNullException("streamReader");

			if (!streamReader.BaseStream.CanSeek)
				throw new ArgumentException("Underlying stream should be seekable.", "streamReader");

			var preamble = (byte[])streamReader.GetType().InvokeMember(
				"_preamble",
				BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
				null, streamReader, null);

			if (preamble.Length > 0 && position < preamble.Length) // preamble or BOM must be skipped
				position += preamble.Length;

			var newPosition = streamReader.BaseStream.Seek(position, origin); // seek
			streamReader.DiscardBufferedData(); // this updates the buffer

			return newPosition;
		}
	}
}