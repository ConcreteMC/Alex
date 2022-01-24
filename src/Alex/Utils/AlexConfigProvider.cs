using System;
using System.Collections.Concurrent;
using MiNET.Utils;

namespace Alex.Utils;

public class AlexConfigProvider : ConfigProvider
{
	public static AlexConfigProvider Instance { get; set; } = new AlexConfigProvider();

	private ConcurrentDictionary<string, string> KeyValues { get; } = new ConcurrentDictionary<string, string>();

	public AlexConfigProvider()
	{
		Set("PluginDisabled", "true");
		Set("UseEncryption", "false");
		Set("EnableCommands", "true");
		Set("EnableBlockTicking", "true");
		Set("EnableChunkTicking", "true");
		Set("GameRule.DoMobspawning", "true");
		Set("GameRule.DoDaylightcycle", "true");
		Set("ForceXBLAuthentication", "false");
		Set("Save.Enabled", "true");
		Set("Save.Interval", "5");
		Set("FastThreads", "10");
		Set("CalculateLights", "false");
	}

	public void Set(string key, string value)
	{
		KeyValues.AddOrUpdate(key.ToLower(), value, (s, s1) => value);
	}

	/// <inheritdoc />
	protected override void OnInitialize() { }

	/// <inheritdoc />
	public override string ReadString(string property)
	{
		if (KeyValues.TryGetValue(property, out var value))
		{
			return value;
		}

		return null;
	}
}