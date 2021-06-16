using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Alex.Common.Services;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Bedrock.Sound;
using FmodAudio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using MiNET.Sounds;
using MiNET.Utils;
using Newtonsoft.Json;
using NLog;
using Sound = FmodAudio.Sound;

namespace Alex.Audio
{
	/// <summary>
	///		Placeholder for the future FMOD based audio engine.
	/// </summary>
	public class AudioEngine
	{

		private static readonly Logger            Log = LogManager.GetCurrentClassLogger(typeof(AudioEngine));
		private                 IStorageSystem    StorageSystem { get; }
		private                 string            StoragePath   { get; }

		private   ConcurrentDictionary<string, SoundInfo> _sounds = new ConcurrentDictionary<string, SoundInfo>();
		private IReadOnlyDictionary<string, string> _soundMapping;
		protected FmodSystem                              FmodSystem;
		private   bool                                    Supported { get; }

		//private double GlobalVolume  { get; set; } = 1d;
		//private double MusicVolume   { get; set; } = 1d;
		//private double SoundFxVolume { get; set; } = 1d;
		//private double AmbientVolume { get; set; } = 1d;
		
		private IOptionsProvider OptionsProvider { get; }
		public AudioEngine(IStorageSystem storageSystem, IOptionsProvider optionsProvider)
		{
			StorageSystem = storageSystem;
			StoragePath = Path.Combine("assets", "bedrock");
			OptionsProvider = optionsProvider;

			try
			{
				FmodSystem = Fmod.CreateSystem();// new FmodSystem();
				
				FmodSystem.Output = OutputType.Autodetect;
				FmodSystem.Init(32, InitFlags._3D_RightHanded);
				FmodSystem.Set3DSettings(1f, 1f, 1f);
				
				Supported = true;
			}
			catch(Exception ex)
			{
				Log.Warn(ex, $"Failed to init audio engine. FMod is required.");
				Supported = false;
			}

			Dictionary<string, SoundMapping> soundMappings;
			string soundJson = ResourceManager.ReadStringResource("Alex.Resources.sounds.json");
			soundMappings = JsonConvert.DeserializeObject<Dictionary<string, SoundMapping>>(soundJson);

			Dictionary<string, string> mapped = new Dictionary<string, string>();
			//Dictionary<string, string> 
			foreach (var mapping in soundMappings)
			{
				mapped.TryAdd(mapping.Key, mapping.Value.PlaysoundMapping);

				if (!string.IsNullOrWhiteSpace(mapping.Value.BedrockMapping))
				{
					mapped.TryAdd(mapping.Value.BedrockMapping, mapping.Value.PlaysoundMapping);
				}
			}

			_soundMapping = mapped;
			/*	GlobalVolume = optionsProvider.AlexOptions.SoundOptions.GlobalVolume;
				optionsProvider.AlexOptions.SoundOptions.GlobalVolume.Bind(
					(value, newValue) =>
					{
						GlobalVolume = newValue;
					});
				
				MusicVolume = optionsProvider.AlexOptions.SoundOptions.MusicVolume;
				optionsProvider.AlexOptions.SoundOptions.MusicVolume.Bind(
					(value, newValue) =>
					{
						MusicVolume = newValue;
					});
				
				SoundFxVolume = optionsProvider.AlexOptions.SoundOptions.SoundEffectsVolume;
				optionsProvider.AlexOptions.SoundOptions.SoundEffectsVolume.Bind(
					(value, newValue) =>
					{
						SoundFxVolume = newValue;
					});
	
				AmbientVolume = optionsProvider.AlexOptions.SoundOptions.AmbientVolume;
	
				optionsProvider.AlexOptions.SoundOptions.AmbientVolume.Bind(
					(value, newValue) =>
					{
						AmbientVolume = newValue;
					});*/
		}

		public int Initialize(BedrockResourcePack resourcePack, IProgressReceiver progress)
		{
			if (!Supported)
				return 0;
			
			if (resourcePack.SoundDefinitions == null)
				return 0;

			int count = 0;
			int total = resourcePack.SoundDefinitions.SoundDefinitions.Count;
			foreach (var sound in resourcePack.SoundDefinitions.SoundDefinitions)
			{
				progress?.UpdateProgress(count, total, "Importing sound definitions...", sound.Key);
				List<WrappedSound> values = new List<WrappedSound>();
				foreach (var element in sound.Value.Sounds)
				{
					string path        = StoragePath;
					string elementPath = null;

					if (element.SoundClass != null)
					{
						elementPath = $"{element.SoundClass.Name}.fsb";
						path = Path.Combine(StoragePath, elementPath);
					}
					else if (element.Path != null)
					{
						elementPath = $"{element.Path}.fsb";
						path = Path.Combine(StoragePath, elementPath);
					}

					bool exists = StorageSystem.Exists(path);

					if (!exists)
					{
						if (element.SoundClass != null)
						{
							elementPath = $"{element.SoundClass.Name}.ogg";
							path = Path.Combine(StoragePath, elementPath);
						}
						else if (element.Path != null)
						{
							elementPath = $"{element.Path}.ogg";
							path = Path.Combine(StoragePath, elementPath);
						}
					}

					exists = StorageSystem.Exists(path);
					
					//Sound s = null;

					if (exists && Supported && elementPath != null)
					{
						if (StorageSystem.TryGetDirectory(StoragePath, out var directoryInfo))
						{
							string filePath = Path.Combine(directoryInfo.FullName, elementPath);
							if (!File.Exists(filePath))
								Log.Warn($"Invalid path: {filePath}");

							bool is3d = !(element.SoundClass != null && element.SoundClass.Is3D.HasValue && !element.SoundClass.Is3D.Value);

							var mode = is3d ? (Mode.CreateStream | Mode._3D) : Mode.CreateStream;
							
							Sound s = FmodSystem.CreateSound(filePath, mode);

							if (s.SubSoundCount > 0)
							{
								//Log.Info($"Subsounds: {s.Value.SubSoundCount}");
								s = s.GetSubSound(0);
								//Log.Info($"S: {s.Value.Name}");
							}
							
							s.Mode = mode;
	
							float volume = 1f;
							float pitch  = 1f;

							if (sound.Value.Pitch.HasValue)
								pitch = sound.Value.Pitch.Value;

							if (element.SoundClass != null)
							{
								if (element.SoundClass.Pitch.HasValue)
									pitch = element.SoundClass.Pitch.Value;
								
								if (element.SoundClass.Volume.HasValue)
									volume = element.SoundClass.Volume.Value;
							}
							
							float max = 16f;
							float min = 0.5f;
								
							if (sound.Value.MaxDistance.HasValue)
							{
								max = sound.Value.MaxDistance.Value;
							}
								
							if (sound.Value.MinDistance.HasValue)
							{
								min = sound.Value.MinDistance.Value;
							}

							min = MathF.Min(min, max);
							max = MathF.Max(min, max);
							s.Set3DMinMaxDistance(min, max);

							values.Add(new WrappedSound(s, pitch, volume, is3d));
						}
						//Log.Info($"Sound: {sound.Key}");
					}
				}

				if (values.Count > 0)
				{
					var soundInfo = new SoundInfo(sound.Key, SoundCategory.Effects, values.ToArray());
					_sounds.AddOrUpdate(sound.Key, s => soundInfo, (s, info) => soundInfo);
					//if (_sounds.TryAdd(sound.Key, ))
					{
						count++;
					}
				}
			}

			return count;
		}

		private Vector3 _lastPos = Vector3.Zero;
		public void Update(GameTime gameTime, Vector3 position, Vector3 forward)
		{
			if (!Supported) return;
			
			var vel = (position - _lastPos) *  1000f / (float) gameTime.ElapsedGameTime.TotalMilliseconds;
			
			FmodSystem.Set3DListenerAttributes(
				0, new System.Numerics.Vector3(position.X, position.Y, position.Z),
				System.Numerics.Vector3.Zero, //new System.Numerics.Vector3(vel.X, vel.Y, vel.Z), 
				-System.Numerics.Vector3.UnitZ, 
				System.Numerics.Vector3.UnitY);
			
			_lastPos = position;
			
			FmodSystem.Update();
		}
		
		private string GetName(Sounds sound)
		{
			return sound.ToString();
		}

		public bool PlaySound(Sounds sound, Vector3 position, float pitch, float volume)
		{
			return PlaySound(GetName(sound), position, pitch, volume);
		}

		public bool PlayJavaSound(string sound, Vector3 position, float pitch, float volume)
		{
			if (_soundMapping.TryGetValue(sound, out var mapped))
			{
				return PlaySound(mapped, position, pitch, volume);
			}

			return false;
		}
		
		public bool PlaySound(string sound, Vector3 position, float pitch, float volume)
		{
			if (!_sounds.TryGetValue(sound, out var soundInfo))
			{
				return false;
			}

			if (Supported && soundInfo.Sound != null)
			{
				var     selected    = soundInfo.Sound;
				Channel instance = FmodSystem.PlaySound(selected.Value, paused:true);

				if (selected.Is3D)
				{
					instance.Set3DAttributes(
						new System.Numerics.Vector3(position.X, position.Y, position.Z), default, default);
				}

				if (volume > 1f || volume < 0f)
				{
				//	Log.Warn($"Invalid volume: {volume}");
				}
				instance.Volume = volume * selected.Volume;

				switch (soundInfo.Category)
				{
					case SoundCategory.Ambient:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.AmbientVolume;
						break;

					case SoundCategory.Weather:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.WeatherVolume;
						break;

					case SoundCategory.Player:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.PlayerVolume;
						break;

					case SoundCategory.Block:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.BlocksVolume;
						break;

					case SoundCategory.Hostile:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.HostileVolume;
						break;

					case SoundCategory.Neutral:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.NeutralVolume;
						break;

					case SoundCategory.Record:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.RecordVolume;
						break;

					case SoundCategory.Bottle:
						
						break;

					case SoundCategory.Ui:
						instance.Volume = 1f;
						break;

					case SoundCategory.Music:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.MusicVolume;
						break;

					case SoundCategory.Effects:
						instance.Volume *= (float)OptionsProvider.AlexOptions.SoundOptions.SoundEffectsVolume;
						break;
				}
			
				
				instance.Volume *= (float)	OptionsProvider.AlexOptions.SoundOptions.GlobalVolume;
				
				instance.Pitch = pitch;
				instance.Paused = false;
			}

			return true;
		}
		
		public bool PlaySound(string sound, float pitch = 1f, float volume = 1f)
		{
			return PlaySound(sound, _lastPos, pitch, volume);
		}
	}
}