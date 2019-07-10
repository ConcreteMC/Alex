using System;
using System.Linq;
using System.Text;
using Alex.API.Graphics.Typography;

namespace Alex.API.Utils
{
	public static class StringExtensions
	{
		public static string StripIllegalCharacters(this string input, IFont font)
		{
			if (input == null) return "null";
			return input.ToArray()
				.Where(i => !font.Characters.Contains(i))
				.Aggregate(input, (current, i) => current.Replace(i.ToString(), ""));
		}

		public static string StripColors(this string input)
		{
			if (input == null)
				throw new ArgumentNullException("input");
			if (input.IndexOf('§') == -1)
			{
				return input;
			}
			else
			{
				StringBuilder output = new StringBuilder(input.Length);
				for (int i = 0; i < input.Length; i++)
				{
					if (input[i] == '§')
					{
						if (i == input.Length - 1)
						{
							break;
						}
						else if (input[i + 1] == '§')
						{
							output.Append('§');
						}
						i++;
					}
					else
					{
						output.Append(input[i]);
					}
				}
				return output.ToString();
			}
		}
	}
}
