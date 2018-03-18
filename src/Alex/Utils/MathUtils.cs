using System;

namespace Alex.Utils
{
	public static class MathUtils
	{
		public static float ToRadians(float deg)
		{
			return (float)(Math.PI* deg / 180F);
			return (float)(deg * (Math.PI / 180F));
		}

		public static float RadianToDegree(float angle)
		{
			return (float)(angle * (180.0f / Math.PI));
		}

		public static uint Hash(uint value)
		{
			unchecked
			{
				value ^= value >> 16;
				value *= 0x85ebca6b;
				value ^= value >> 13;
				value *= 0xc2b2ae35;
				value ^= value >> 16;
			}

			return value;
		}
	}
}
