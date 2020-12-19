using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.ResourcePackLib;
using NLog;

namespace Alex.Audio
{
	/// <summary>
	///		Placeholder for the future FMOD based audio engine.
	/// </summary>
	public unsafe class AudioEngine
	{
		[DllImport("libdl.so.2")]
		private static extern IntPtr dlopen(string filename, int flags);
		
		private static readonly Logger            Log = LogManager.GetCurrentClassLogger(typeof(AudioEngine));
		private                 IStorageSystem    StorageSystem { get; }
		private                 string            StoragePath   { get; }

		public AudioEngine(IStorageSystem storageSystem)
		{
			StorageSystem = storageSystem;
			StoragePath = Path.Combine("assets");
			
		}

		public void Initialize(McResourcePack resourcePack)
		{
			if (resourcePack.SoundDefinitions == null)
				return;

			foreach (var sound in resourcePack.SoundDefinitions)
			{
				foreach (var element in sound.Value.Sounds)
				{
					string path        = StoragePath;
					string elementPath = null;

					if (element.SoundClass != null)
					{
						elementPath = $"minecraft/sounds/{element.SoundClass.Name}.ogg";
						path = Path.Combine(path, elementPath);
					}
					else if (element.Path != null)
					{
						elementPath = $"minecraft/sounds/{element.Path}.ogg";
						path = Path.Combine(path, elementPath);
					}

					if (!StorageSystem.Exists(path) && elementPath != null)
					{
						if (!StorageSystem.TryCreateDirectory(Path.GetDirectoryName(path))) { }

						try
						{
						/*	using (var stream = resourcePack.GetStream(elementPath))
							{
								var data = stream.ReadToEnd();

								if (StorageSystem.TryWriteBytes(path, data.ToArray()))
								{
									Log.Info($"Saved {elementPath}...");
								}
							}*/
						}
						catch (FileNotFoundException)
						{
							Log.Warn($"File not found: {elementPath}");
						}
					}
				}
			}
		}

	}
}