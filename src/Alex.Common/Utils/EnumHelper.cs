using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Alex.Common.Utils;

public static class EnumHelper
{
	/// <summary>
	///   Gets a the value of a <see cref="T:System.ComponentModel.DescriptionAttribute" />
	///   associated with a particular enumeration value.
	/// </summary>
	/// <typeparam name="T">The enumeration type.</typeparam>
	/// <param name="source">The enumeration value.</param>
	/// <returns>The string value stored in the value's description attribute.</returns>
	/// <footer><a href="https://www.google.com/search?q=Accord.ExtensionMethods.GetDescription">`ExtensionMethods.GetDescription` on google.com</a></footer>
	public static T ParseUsingEnumMember<T>(string value,
		StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		var enumType = typeof(T);

		foreach (var name in Enum.GetNames(enumType))
		{
			var enumMemberAttribute =
				((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true))
			   .Single();

			if (string.Equals(enumMemberAttribute.Value, value, comparison)) return (T)Enum.Parse(enumType, name);
		}

		//throw exception or whatever handling you want or
		return default(T);
	}

	public static bool TryParseUsingEnumMember<T>(string value, out T enumValue) where T : Enum
	{
		enumValue = default;

		try
		{
			enumValue = ParseUsingEnumMember<T>(value);

			return true;
		}
		catch
		{
			return false;
		}
	}
}