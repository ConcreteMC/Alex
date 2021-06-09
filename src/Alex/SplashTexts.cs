using System;
using Alex.Common.Utils;
using MiNET.Utils;

namespace Alex
{
	internal static class SplashTexts
	{
		private static readonly string[] Texts =
		{
			"Inspired by ThinkOfDeath",
			"Weird shit!",
			"Uhm, random?",
			$"This {TextColor.Underline}%RANDOM%{TextColor.Reset} is a random number!",
			"Has a menu!",
			"Created in C#",
			"O.M.G Why are you reading this?!",
            "This project is messy af...  Don't look at me!",
            "Ehhhh hi?",
			"Such compatibility, wow.",
			"Who like minecwaf?!",
			$"Thanks {TextColor.Rainbow("TruDan")}!",
			"3D Transformations are hard...",
			$"Powered by {TextColor.Rainbow("MiNET")}",
			$"Wow... This logo {TextColor.Bold}SUCKS{TextColor.Reset} :O",
			$"GUI powered by {TextColor.Gold}https://github.com/TruDan/RocketUI"
		};
		
		public static string GetSplashText()
		{
			return Texts[FastRandom.Instance.Next(Texts.Length - 1)].Replace("%RANDOM%", FastRandom.Instance.Next().ToString());
		}
	}
}
