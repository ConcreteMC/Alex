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
        Undefined = 0b0000,

        FirstPerson = 0b0001,
        ThirdPerson = 0b0010,
        LeftHand = 0b0100,
        RightHand = 0b1000,


        [JsonEnumValue("thirdperson_righthand")]
        ThirdPersonRightHand = ThirdPerson | RightHand,

        [JsonEnumValue("thirdperson_lefthand")]
        ThirdPersonLeftHand = ThirdPerson | LeftHand,

        [JsonEnumValue("firstperson_righthand")]
        FirstPersonRightHand = FirstPerson | RightHand,

        [JsonEnumValue("firstperson_lefthand")]
        FirstPersonLeftHand = FirstPerson | LeftHand,

        [JsonEnumValue("gui")] Gui = 0b00010000,
        [JsonEnumValue("head")] Head = 0b00110000,
        [JsonEnumValue("ground")] Ground = 0b01010000,
        [JsonEnumValue("fixed")] Fixed = 0b10010000
    }

    public class JsonEnumValueAttribute : Attribute
    {
        public string Value { get; set; }

        public JsonEnumValueAttribute(string value)
        {
            Value = value;
        }

        public JsonEnumValueAttribute() : this(null)
        {
        }
    }

    public static class DisplayPositionHelper
    {
        private static readonly IReadOnlyDictionary<string, DisplayPosition> _lookupCache;
        private static readonly IReadOnlyDictionary<DisplayPosition, string> _reverseLookupCache;

        static DisplayPositionHelper()
        {
            var type = typeof(DisplayPosition);
            var dict = new Dictionary<string, DisplayPosition>();

            foreach (var JsonEnumValue in Enum.GetNames(type))
            {
                var i = type.GetField(JsonEnumValue);
                var attr = i?.GetCustomAttribute<JsonEnumValueAttribute>();
                if (attr != null)
                {
                    dict.Add(attr.Value, (DisplayPosition) i.GetRawConstantValue());
                }
            }

            _lookupCache = new ReadOnlyDictionary<string, DisplayPosition>(dict);
            _reverseLookupCache =
                new ReadOnlyDictionary<DisplayPosition, string>(dict.ToDictionary(ks => ks.Value, vs => vs.Key));
        }

        public static DisplayPosition ToDisplayPosition(string string_value)
        {
            if(string.IsNullOrWhiteSpace(string_value))
                throw new ArgumentNullException(nameof(string_value));
            
            if (_lookupCache.TryGetValue(string_value.ToLowerInvariant(), out var displayPosition))
                return displayPosition;
            
            throw new ArgumentOutOfRangeException(nameof(string_value));
        }

        public static string ToString(this DisplayPosition displayPosition)
        {
            if (_reverseLookupCache.TryGetValue(displayPosition, out var str))
                return str;
            
            throw new ArgumentOutOfRangeException(nameof(displayPosition));
        }
    }
}