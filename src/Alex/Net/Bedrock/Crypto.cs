using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;

namespace Alex.Net.Bedrock
{
	public class CryptoContext
	{
		public bool UseEncryption;

		//public RijndaelManaged Algorithm { get; set; }

		//public IBufferedCipher Decryptor { get; set; }
		//public MemoryStream InputStream { get; set; }
		public IBufferedCipher Decryptor { get; set; }

		//public IBufferedCipher Encryptor { get; set; }
		//public MemoryStream OutputStream { get; set; }
		public IBufferedCipher Encryptor { get; set; }

		public long SendCounter = -1;

		public AsymmetricCipherKeyPair ClientKey { get; set; }
		public byte[]                  Key       { get; set; }

		public byte[] Decrypt(byte[] payload)
		{
			return Decryptor.ProcessBytes(payload);
		}

		public byte[] Encrypt(byte[] wrapperPayload)
		{
			return Encryptor.ProcessBytes(wrapperPayload);
		}
	}
}