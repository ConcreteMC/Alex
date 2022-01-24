using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Alex.ResourcePackLib.Json.Models.Items
{
	[Flags]
	public enum DisplayPosition
	{
		Undefined = 0,

		FirstPerson = 1,
		ThirdPerson = 2,
		LeftHand = 4,
		RightHand = 8,


		[JsonEnumValue("thirdperson_righthand")]
		ThirdPersonRightHand = ThirdPerson | RightHand,

		[JsonEnumValue("thirdperson_lefthand")]
		ThirdPersonLeftHand = ThirdPerson | LeftHand,

		[JsonEnumValue("firstperson_righthand")]
		FirstPersonRightHand = FirstPerson | RightHand,

		[JsonEnumValue("firstperson_lefthand")]
		FirstPersonLeftHand = FirstPerson | LeftHand,

		[JsonEnumValue("gui")] Gui = 16,
		[JsonEnumValue("head")] Head = 32,
		[JsonEnumValue("ground")] Ground = 64,
		[JsonEnumValue("fixed")] Fixed = 128
	}

	public enum ItemDisplayType
	{
		Undefined = 0,
		HandHeld = 1,
		Generated = 2,
		Fixed = 3
	}

	public class JsonEnumValueAttribute : Attribute
	{
		public string Value { get; set; }

		public JsonEnumValueAttribute(string value)
		{
			Value = value;
		}

		public JsonEnumValueAttribute() : this(null) { }
	}

	public static class DisplayPositionHelper
	{
		private static readonly IReadOnlyDictionary<string, DisplayPosition> _lookupCache;
		private static readonly IReadOnlyDictionary<DisplayPosition, string> _reverseLookupCache;

		static DisplayPositionHelper()
		{
			var type = typeof(DisplayPosition);
			var dict = new Dictionary<string, DisplayPosition>(StringComparer.OrdinalIgnoreCase);

			foreach (var value in Enum.GetValues<DisplayPosition>())
			{
				var i = type.GetField(value.ToString());
				var attr = i?.GetCustomAttribute<JsonEnumValueAttribute>();

				if (attr != null)
				{
					dict.Add(attr.Value, value);
				}
			}

			_lookupCache = new ReadOnlyDictionary<string, DisplayPosition>(dict);

			_reverseLookupCache =
				new ReadOnlyDictionary<DisplayPosition, string>(dict.ToDictionary(ks => ks.Value, vs => vs.Key));
		}

		public static DisplayPosition ToDisplayPosition(string stringValue)
		{
			if (string.IsNullOrWhiteSpace(stringValue))
				throw new ArgumentNullException(nameof(stringValue));

			if (_lookupCache.TryGetValue(stringValue.ToLowerInvariant(), out var displayPosition))
				return displayPosition;

			throw new ArgumentOutOfRangeException(nameof(stringValue));
		}

		public static string ToString(this DisplayPosition displayPosition)
		{
			if (_reverseLookupCache.TryGetValue(displayPosition, out var str))
				return str;

			throw new ArgumentOutOfRangeException(nameof(displayPosition));
		}

		public static bool TryGetString(this DisplayPosition displayPosition, out string result)
		{
			if (_reverseLookupCache.TryGetValue(displayPosition, out result))
				return true;

			return false;
		}
	}
}