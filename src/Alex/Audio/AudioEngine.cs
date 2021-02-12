using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.ResourcePackLib;
using FmodAudio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using MiNET.Sounds;
using MiNET.Utils;
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
		protected FmodSystem                              FmodSystem;
		private   bool                                    Supported { get; }

		private double GlobalVolume  { get; set; } = 1f;
		private double MusicVolume   { get; set; } = 1f;
		private double SoundFxVolume { get; set; } = 1f;
		public AudioEngine(IStorageSystem storageSystem, IOptionsProvider optionsProvider)
		{
			StorageSystem = storageSystem;
			StoragePath = Path.Combine("assets", "bedrock");

			try
			{
				FmodSystem = Fmod.CreateSystem();// new FmodSystem();
				
				FmodSystem.Init(32, InitFlags._3D_RightHanded);
				FmodSystem.Set3DSettings(1f, 1f, 1f);
				
				Supported = true;
			}
			catch(Exception ex)
			{
				Log.Warn(ex, $"Failed to init audio engine. FMod is required.");
				Supported = false;
			}

			GlobalVolume = optionsProvider.AlexOptions.SoundOptions.GlobalVolume;
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
		}

		public void Initialize(BedrockResourcePack resourcePack)
		{
			if (resourcePack.SoundDefinitions == null)
				return;

			foreach (var sound in resourcePack.SoundDefinitions.SoundDefinitions)
			{
				List<WrappedSound> values = new List<WrappedSound>();
				foreach (var element in sound.Value.Sounds)
				{
					string path        = StoragePath;
					string elementPath = null;

					if (element.SoundClass != null)
					{
						elementPath = $"{element.SoundClass.Name}.fsb";
						path = Path.Combine(path, elementPath);
					}
					else if (element.Path != null)
					{
						elementPath = $"{element.Path}.fsb";
						path = Path.Combine(path, elementPath);
					}

					bool exists = StorageSystem.Exists(path);
					if (!exists && elementPath != null)
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

					//Sound s = null;

					if (exists && Supported && elementPath != null)
					{
						if (StorageSystem.TryGetDirectory(StoragePath, out var directoryInfo))
						{
							string filePath = Path.Combine(directoryInfo.FullName, elementPath);
							if (!File.Exists(filePath))
								Log.Warn($"Invalid path: {filePath}");
							
							Sound s = FmodSystem.CreateSound(filePath, Mode.CreateStream | Mode._3D | Mode._3D_HeadRelative);

							if (s.SubSoundCount > 0)
							{
								//Log.Info($"Subsounds: {s.Value.SubSoundCount}");
								s = s.GetSubSound(0);
								//Log.Info($"S: {s.Value.Name}");
							}
							
							s.Mode = Mode.CreateStream | Mode._3D | Mode._3D_HeadRelative;
	
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

							values.Add(new WrappedSound(s, pitch, volume));
						}
						//Log.Info($"Sound: {sound.Key}");
					}
				}

				if (values.Count > 0)
				{
					_sounds.TryAdd(sound.Key, new SoundInfo(sound.Key, SoundCategory.Effects, values.ToArray()));
				}
			}
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
		
		public bool PlaySound(string sound, Vector3 position, float pitch, float volume)
		{
			if (!_sounds.TryGetValue(sound, out var soundInfo))
			{
			/*	if (soundInfo != null)
				{
					Log.Warn($"Sound not found: {sound} ({soundInfo.Path})");
				}
				else
				{
					Log.Warn($"Sound not found: {sound}");
				}*/

				return false;
			}

			if (Supported && soundInfo.Sound != null)
			{
				var     selected    = soundInfo.Sound;
				Channel instance = FmodSystem.PlaySound(selected.Value, paused:true);
				instance.Set3DAttributes(new System.Numerics.Vector3(position.X, position.Y, position.Z),
					default, default);
				
		//		selected.Value.Get3DMinMaxDistance(out float min, out float max);
				
		//		instance.Set3DMinMaxDistance(min, max * selected.Volume);
				
				//instance.Volume = volume;

				if (soundInfo.Category == SoundCategory.Effects)
				{
					instance.Volume *= (float)SoundFxVolume;
				}
				else if (soundInfo.Category == SoundCategory.Music)
				{
					instance.Volume *= (float)MusicVolume;
				}
				
				instance.Volume *= (float)GlobalVolume;
				
				instance.Pitch = pitch;
				instance.Paused = false;
			}
			//MediaPlayer.Play(soundInfo.Song);

			return true;
			//MediaPlayer.Play();
		}
	}

	public class SoundInfo
	{
		private static FastRandom    _fastRandom = new FastRandom();
		public         string        Name     { get; set; }
		public         SoundCategory Category { get; set; }
		public WrappedSound Sound
		{
			get
			{
				if (_sounds == null || _sounds.Length == 0)
					return null;

				return _sounds[_fastRandom.Next() % _sounds.Length];
			}
		}
		
		private WrappedSound[] _sounds;
		public SoundInfo(string name, SoundCategory category, WrappedSound[] sounds)
		{
			Name = name;
			Category = category;
			_sounds = sounds;
		}
	}

	public class WrappedSound
	{
		public Sound Value  { get; }
		public float Pitch  { get; }
		public float Volume { get; }
		public WrappedSound(Sound sound, float pitch = 1f, float volume = 1f)
		{
			Value = sound;
			Pitch = pitch;
			Volume = volume;
		}
	}

	public enum SoundCategory
	{
		Music,
		Effects
	}
	
	public enum Sounds
	{
		
	}
}