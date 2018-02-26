using System;

namespace Alex.CoreRT.Utils
{
	public static class MathUtils
	{
		public static float ToRadians(float deg)
		{
			return (float)(Math.PI* deg / 180F);
			return (float)(deg * (Math.PI / 180F));
		}
	}
}
