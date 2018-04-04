using System;
using System.Security.Cryptography;
using Alex.Networking.Java.Util.Encryption;

namespace Alex.Networking.Java.Util
{
	public class EncryptionHolder
	{
		private RSACryptoServiceProvider Rsa { get; }
		private RSAParameters _privateKey;
		private RSAParameters _publicKey;

		public byte[] PublicKey { get; }

		public EncryptionHolder()
		{
			CspParameters csp = new CspParameters
			{
				KeyContainerName = "SharpMC",
				ProviderType = 1,
				KeyNumber = 1
			};


			Rsa = new RSACryptoServiceProvider(1024, csp);

			_privateKey = Rsa.ExportParameters(true);
			_publicKey = Rsa.ExportParameters(false);

			PublicKey = AsnKeyBuilder.PublicKeyToX509(_publicKey).GetBytes();
		}

		public byte[] Decrypt(byte[] data)
		{
			return RsaDecrypt(data, _privateKey, false);
		}

		public static byte[] RsaEncrypt(byte[] dataToEncrypt, RSAParameters rsaKeyInfo, bool doOaepPadding)
		{
			try
			{
				byte[] encryptedData;
				//Create a new instance of RSACryptoServiceProvider.
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
				{

					//Import the RSA Key information. This only needs
					//toinclude the public key information.
					rsa.ImportParameters(rsaKeyInfo);

					//Encrypt the passed byte array and specify OAEP padding.  
					//OAEP padding is only available on Microsoft Windows XP or
					//later.  
					encryptedData = rsa.Encrypt(dataToEncrypt, doOaepPadding);
				}
				return encryptedData;
			}
			//Catch and display a CryptographicException  
			//to the console.
			catch (CryptographicException e)
			{
				Console.WriteLine(e.Message);

				return null;
			}

		}

		public static byte[] RsaDecrypt(byte[] dataToDecrypt, RSAParameters rsaKeyInfo, bool doOaepPadding)
		{
			try
			{
				byte[] decryptedData;
				//Create a new instance of RSACryptoServiceProvider.
				using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
				{
					//Import the RSA Key information. This needs
					//to include the private key information.
					rsa.ImportParameters(rsaKeyInfo);

					//Decrypt the passed byte array and specify OAEP padding.  
					//OAEP padding is only available on Microsoft Windows XP or
					//later.  
					decryptedData = rsa.Decrypt(dataToDecrypt, doOaepPadding);
				}
				return decryptedData;
			}
			//Catch and display a CryptographicException  
			//to the console.
			catch (CryptographicException e)
			{
				Console.WriteLine(e.ToString());

				return null;
			}

		}
	}
}
