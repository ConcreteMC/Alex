using System.Collections.Generic;
using System.Text;
using Alex.Interfaces;

namespace Alex.Networking.Java.Models
{
	public static class ChatParser
	{
		/// <summary>
		/// The main function to convert text from MC 1.6+ JSON to MC 1.5.2 formatted text
		/// </summary>
		/// <param name="json">JSON serialized text</param>
		/// <param name="links">Optional container for links from JSON serialized text</param>
		/// <returns>Returns the translated text</returns>
		public static string ParseText(string json)
		{
			var parsed = Json.ParseJson(json);

			if (parsed == null)
				return null;
			
			return JSONData2String(parsed);
		}

		/// <summary>
		/// Format text using a specific formatting rule.
		/// Example : * %s %s + ["ORelio", "is doing something"] = * ORelio is doing something
		/// </summary>
		/// <param name="rulename">Name of the rule, chosen by the server</param>
		/// <param name="using_data">Data to be used in the rule</param>
		/// <returns>Returns the formatted text according to the given data</returns>
		private static string TranslateString(string rulename, List<string> using_data)
		{
			//	if (!RulesInitialized) { InitRules(); RulesInitialized = true; }
			//if (TranslationRules.ContainsKey(rulename))
			{
				int using_idx = 0;
				string rule = Language?.GetString(rulename) ?? rulename; //[rulename];
				StringBuilder result = new StringBuilder();

				for (int i = 0; i < rule.Length; i++)
				{
					if (rule[i] == '%' && i + 1 < rule.Length)
					{
						//Using string or int with %s or %d
						if (rule[i + 1] == 's' || rule[i + 1] == 'd')
						{
							if (using_data.Count > using_idx)
							{
								result.Append(using_data[using_idx]);
								using_idx++;
								i += 1;

								continue;
							}
						}

						//Using specified string or int with %1$s, %2$s...
						else if (char.IsDigit(rule[i + 1]) && i + 3 < rule.Length && rule[i + 2] == '$'
						         && (rule[i + 3] == 's' || rule[i + 3] == 'd'))
						{
							int specified_idx = rule[i + 1] - '1';

							if (using_data.Count > specified_idx)
							{
								result.Append(using_data[specified_idx]);
								using_idx++;
								i += 3;

								continue;
							}
						}
					}

					result.Append(rule[i]);
				}

				return result.ToString();
			}
			//else return "[" + rulename + "] " + String.Join(" ", using_data);
		}

		/// <summary>
		/// Use a JSON Object to build the corresponding string
		/// </summary>
		/// <param name="data">JSON object to convert</param>
		/// <param name="colorcode">Allow parent color code to affect child elements (set to "" for function init)</param>
		/// <param name="links">Container for links from JSON serialized text</param>
		/// <returns>returns the Minecraft-formatted string</returns>
		private static string JSONData2String(Json.JSONData data, string colorcode = "")
		{
			string extra_result = "";

			switch (data.Type)
			{
				case Json.JSONData.DataType.Object:
					if (data.Properties.ContainsKey("color"))
					{
						colorcode = TextColor.Color2tag(JSONData2String(data.Properties["color"], ""));
					}

					/*if (data.Properties.ContainsKey("clickEvent"))
					{
						Utils.Json.JSONData clickEvent = data.Properties["clickEvent"];
						if (clickEvent.Properties.ContainsKey("action")
							&& clickEvent.Properties.ContainsKey("value")
							&& clickEvent.Properties["action"].StringValue == "open_url"
							&& !String.IsNullOrEmpty(clickEvent.Properties["value"].StringValue))
						{
							links.Add(clickEvent.Properties["value"].StringValue);
						}
					}*/
					if (data.Properties.ContainsKey("extra"))
					{
						Json.JSONData[] extras = data.Properties["extra"].DataArray.ToArray();

						foreach (Json.JSONData item in extras)
							extra_result = extra_result + JSONData2String(item, colorcode) + "§r";
					}

					if (data.Properties.ContainsKey("text"))
					{
						return colorcode + JSONData2String(data.Properties["text"], colorcode) + extra_result;
					}
					else if (data.Properties.ContainsKey("translate"))
					{
						List<string> usingData = new List<string>();

						if (data.Properties.ContainsKey("using") && !data.Properties.ContainsKey("with"))
							data.Properties["with"] = data.Properties["using"];

						if (data.Properties.ContainsKey("with"))
						{
							Json.JSONData[] array = data.Properties["with"].DataArray.ToArray();

							for (int i = 0; i < array.Length; i++)
							{
								usingData.Add(JSONData2String(array[i], colorcode));
							}
						}

						return colorcode + TranslateString(JSONData2String(data.Properties["translate"]), usingData)
						                 + extra_result;
					}
					else return extra_result;

				case Json.JSONData.DataType.Array:
					string result = "";

					foreach (Json.JSONData item in data.DataArray)
					{
						result += JSONData2String(item, colorcode);
					}

					return result;

				case Json.JSONData.DataType.String:
					return colorcode + data.StringValue;
			}

			return "";
		}

		public static ITranslationProvider Language { get; set; }
	}
}