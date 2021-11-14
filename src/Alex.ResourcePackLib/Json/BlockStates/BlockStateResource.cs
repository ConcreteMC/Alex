using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.BlockStates
{
	public class BlockStateResource
	{
		[JsonIgnore]
		public string Name { get; set; }

		[JsonIgnore]
		public string Namespace { get; set; }

		/// <summary>
		/// Holds the names of all the variants of the block.
		/// </summary>
		public Dictionary<BlockVariantKey, BlockStateVariant> Variants { get; set; } = new Dictionary<BlockVariantKey, BlockStateVariant>();

		[JsonProperty("multipart")]
		public BlockStateMultiPart[] Parts { get; set; } = new BlockStateMultiPart[0];
	}

	[JsonConverter(typeof(BlockVariantKeyConverter)), TypeConverter(typeof(BlockVariantKeyTypeConverter))]
	public class BlockVariantKey : IEnumerable<KeyValuePair<string, string>>
	{
		private IReadOnlyDictionary<string, string> _dictionary;
		public BlockVariantKey() : this(null)
		{
			
		}

		public BlockVariantKey(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				_dictionary = new Dictionary<string, string>();
				return;
			}

			var matches = VariantParser.Matches(input).ToArray();
			var dictionary = new Dictionary<string, string>(matches.Length, StringComparer.OrdinalIgnoreCase);
			foreach (Match match in matches)
			{
				dictionary.Add(match.Groups["property"].Value, match.Groups["value"].Value);
			}

			_dictionary = dictionary;
		}

		public bool TryGetValue(string key, out string value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public string this[string key]
		{
			get => _dictionary[key];
		}
		
		private static readonly Regex VariantParser = new Regex(
			"(?'property'[^=,]*?)=(?'value'[^,]*)", RegexOptions.Compiled);

		/// <inheritdoc />
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			foreach (var kv in _dictionary)
				yield return kv;
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
