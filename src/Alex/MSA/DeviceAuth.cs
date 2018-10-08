using System;
using System.Collections.Generic;
using System.Text;
using Alex.Utils;

namespace Alex.MSA
{
	public class DeviceAuth
	{
		private static FastRandom RND = new FastRandom();

		public string Username;
		public string Password;
		public string Puid;

		public DeviceAuth()
		{

		}

		public void Randomize()
		{
			Username = GenerateRandom("abcdefghijklmnopqrstuvwxyz", 18);
			Password = GenerateRandom(
				"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+[]{}/?;:'\\\",.<>`~", 16);
		}

		private string GenerateRandom(string allowedChars, int length)
		{
			char[] chars = new char[length];
			for (int i = 0; i < chars.Length; i++)
			{
				chars[i] = allowedChars[RND.Next(0, allowedChars.Length - 1)];
			}

			return new string(chars);
		}
	}
}
