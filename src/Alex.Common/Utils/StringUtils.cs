using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Alex.Common.Utils;

public static class StringUtils
{
	private static readonly Regex _wordSplitRegex = new Regex(
		@"[A-Z]{2,}(?=[A-Z][a-z]+[0-9]*|\b)|[A-Z]?[a-z]+[0-9]*|[A-Z]|[0-9]+", RegexOptions.Compiled);

	public static string ToCamelCase(this string str)
	{
		return new string(
			CultureInfo.InvariantCulture.TextInfo
			   .ToTitleCase(string.Join(" ", _wordSplitRegex.Matches(str)).ToLowerInvariant()).Replace(@" ", "")
			   .Select((x, i) => i == 0 ? char.ToLowerInvariant(x) : x).ToArray());
	}

	public static string ToLowerSnakeCase(this string str)
	{
		return string.Join("_", _wordSplitRegex.Matches(str)).ToLowerInvariant();
	}

	public static string ToLowerKebabCase(this string str)
	{
		return string.Join("-", _wordSplitRegex.Matches(str)).ToLowerInvariant();
	}
}