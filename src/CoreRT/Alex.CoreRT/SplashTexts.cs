using System;

namespace Alex
{
	internal static class SplashTexts
	{
		private static readonly string[] Texts =
		{
			"Inspired by ThinkOfDeath",
			"Weird shit!",
			"Uhm, random?",
			"This %RANDOM% is a random number!",
			"Has a menu!",
			"Created in C#",
			"O.M.G Why are you reading this?!",
            "This project is messy af...  Don't look at me!",
            "Ehhhh hi?"
		};

		private static readonly Random Random = new Random();

		public static string GetSplashText()
		{
			return Texts[Random.Next(Texts.Length - 1)].Replace("%RANDOM%", Random.Next().ToString());
		}
	}
}
