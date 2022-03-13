using System.IO;
using System.IO.Compression;
using System.Linq;
using Alex.Common.Utils;
using Alex.Networking.Java.Util;
using Alex.ResourcePackLib.IO.Abstract;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SicStream;
using CipherStream = Org.BouncyCastle.Crypto.IO.CipherStream;

namespace Alex.ResourcePackLib
{
	internal static class ZipExtensions
	{
		public static bool IsFile(this ZipArchiveEntry entry)
		{
			return !entry.FullName.EndsWith("/");
		}

		public static bool IsDirectory(this ZipArchiveEntry entry)
		{
			return entry.FullName.EndsWith("/");
		}

		public static string ReadAsString(this IFile entry)
		{
			using (TextReader reader = new StreamReader(entry.Open()))
			{
				return reader.ReadToEnd();
			}
		}

		public static Stream OpenEncoded(this IFile entry, IResourceEncryptionProvider cryptoProvider)
		{
			if (cryptoProvider == null)
				return entry.Open();

			if (cryptoProvider.TryOpen(entry, out var cryptoStream))
			{
				return cryptoStream;
			}
			
			//TODO: Decrypt.
			return entry.Open();
		}

		public static Stream OpenEncoded(this IFile entry, byte[] key)
		{
			return entry.Open().OpenEncoded(key);
		}
		
		public static Stream OpenEncoded(this Stream stream, byte[] key)
		{
			IBufferedCipher decryptor = CipherUtilities.GetCipher("AES/CFB8/NoPadding");
			decryptor.Init(false, new ParametersWithIV(new KeyParameter(key), key.Take(16).ToArray()));

			IBufferedCipher encryptor = CipherUtilities.GetCipher("AES/CFB8/NoPadding"); 
			encryptor.Init(true, new ParametersWithIV(new KeyParameter(key), key.Take(16).ToArray()));
			
			/*var encryptor = new StreamingSicBlockCipher(new SicBlockCipher(new AesEngine()));
			var decryptor = new StreamingSicBlockCipher(new SicBlockCipher(new AesEngine()));
			decryptor.Init(false, new ParametersWithIV(new KeyParameter(secret), secret.Take(12).Concat(new byte[] {0, 0, 0, 2}).ToArray()));
			encryptor.Init(true, new ParametersWithIV(new KeyParameter(secret), secret.Take(12).Concat(new byte[] {0, 0, 0, 2}).ToArray()));*/
			
			var cipherStream = new CipherStream(stream, decryptor, encryptor);
			return cipherStream;
		}

		public static string ReadAsEncodedString(this IFile entry, IResourceEncryptionProvider cryptoProvider)
		{
			using (TextReader reader = new StreamReader(entry.OpenEncoded(cryptoProvider)))
			{
				return reader.ReadToEnd();
			}
		}
	}
}