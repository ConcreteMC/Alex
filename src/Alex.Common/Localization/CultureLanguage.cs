using System;
using System.Collections.Generic;
using Alex.Interfaces;

namespace Alex.Common.Localization
{
	public class CultureLanguage : ITranslationProvider
	{
		public string this[string key]
		{
			get { return GetString(key); }
		}

		private readonly Dictionary<string, string> _translations =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public bool Loaded { get; private set; } = false;

		public CultureLanguage() { }

		public void Load(IDictionary<string, string> translations)
		{
			//if (Loaded)
			//   return;

			foreach (var translation in translations)
			{
				if (_translations.ContainsKey(translation.Key)) continue;
				_translations[translation.Key] = translation.Value;
			}

			// Loaded = true;
		}

		public string GetString(string key)
		{
			if (_translations.TryGetValue(key, out var value))
			{
				return value;
			}

			return key; //$"[Translation={key}]";
		}

		private string _displayName = null;

		public string DisplayName
		{
			get
			{
				if (_displayName != null)
					return _displayName;

				string name = GetString("language.name");
				string region = GetString("language.region");

				if (!string.IsNullOrWhiteSpace(region))
				{
					return $"{name} ({region})";
				}

				return name;
			}
			set
			{
				_displayName = value;
			}
		}

		private string _name = null;

		public string Name
		{
			get
			{
				if (_name == null)
					return GetString("language.name");

				return _name;
			}
			set
			{
				_name = value;
			}
		}

		private string _region = null;

		public string Region
		{
			get
			{
				if (_region == null)
					return GetString("language.region");

				return _region;
			}
			set
			{
				_region = value;
			}
		}

		private string _code = null;

		public string Code
		{
			get
			{
				if (_code == null)
					return GetString("language.code");

				return _code;
			}
			set
			{
				_code = value;
			}
		}
	}
}