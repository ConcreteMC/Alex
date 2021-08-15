using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Services;
using Alex.Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Alex
{
	public class ProfileManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ProfileManager));
		private Dictionary<string, SavedProfile> Profiles { get; }
		public SavedProfile LastUsedProfile { get; private set; } = null;
		private IServiceProvider ServiceProvider { get; }
		
		public PlayerProfile CurrentProfile { get; private set; }
		public ProfileManager(IServiceProvider serviceProvider)
		{
			Profiles = new Dictionary<string, SavedProfile>();
			ServiceProvider = serviceProvider;
		}

		private const string StatusMessage = "Loading profiles...";
		private const string ProfilesFile = "profiles";
		public void LoadProfiles(IProgressReceiver progressReceiver)
		{
			IStorageSystem storage = ServiceProvider.GetRequiredService<IStorageSystem>();
			
			progressReceiver.UpdateProgress(0, StatusMessage);
			if (storage.TryReadJson(ProfilesFile, out ProfilesFileFormat saveFile))
			{
				progressReceiver.UpdateProgress(50, StatusMessage);

				SavedProfile[] profiles = null;
				

				progressReceiver.UpdateProgress(50, StatusMessage);

				if (saveFile != null)
				{
					profiles = saveFile.Profiles;

					if (!string.IsNullOrWhiteSpace(saveFile.SelectedProfile))
					{
						progressReceiver.UpdateProgress(75, StatusMessage);

						foreach (var profile in profiles)
						{
							//profile.Profile.Type = profile.Type;// == ProfileType.Bedrock;
							if (profile.Profile.Uuid.Equals(saveFile.SelectedProfile))
							{
								progressReceiver.UpdateProgress(90, StatusMessage);
								LastUsedProfile = profile;
								//profileService.TryAuthenticateAsync(profile.Profile);
								//profileService.CurrentProfile = profile;
								break;
							}
						}
					}

					progressReceiver.UpdateProgress(99, StatusMessage);
					foreach (var profile in profiles)
					{
						Profiles.TryAdd(profile.Profile.Uuid, profile);
					}
				}
				else
				{
					Log.Warn($"Profiles file not found.");
				}
			}
			else
			{
				storage.TryWriteJson(ProfilesFile, new ProfilesFileFormat());
			//	File.WriteAllText(ProfilesFile, JsonConvert.SerializeObject(new ProfilesFileFormat(), Formatting.Indented));
			}

			progressReceiver.UpdateProgress(100, StatusMessage);
		}

		public void SaveProfiles()
		{
			IStorageSystem storage = ServiceProvider.GetRequiredService<IStorageSystem>();
			
			storage.TryWriteJson(ProfilesFile, new ProfilesFileFormat()
			{
				Profiles = Profiles.Values.ToArray(),
				SelectedProfile = CurrentProfile?.Uuid ?? string.Empty
			});
		}

		public void CreateOrUpdateProfile(string type, PlayerProfile profile, bool setActive = false)
		{
			var alex = ServiceProvider.GetRequiredService<Alex>();
			
			if (profile.Skin?.Texture == null)
			{
				profile.Skin = new Skin();

				if (alex.Resources.TryGetBitmap("entity/alex", out var rawTexture))
				{
					profile.Skin.Texture = TextureUtils.BitmapToTexture2D(this, alex.GraphicsDevice, rawTexture);
					profile.Skin.Slim = true;
				}
			}
			SavedProfile savedProfile;
			if (Profiles.TryGetValue(profile.Uuid, out savedProfile))
			{
				savedProfile.Type = type;
				savedProfile.Profile = profile;
				Profiles[profile.Uuid] = savedProfile;
			}
			else
			{
				savedProfile = new SavedProfile();
				savedProfile.Type = type;
				savedProfile.Profile = profile;
				Profiles.Add(profile.Uuid, savedProfile);
			}

			if (setActive)
			{
				CurrentProfile = profile;
			}

			alex.UiTaskManager.Enqueue(SaveProfiles);
		}

		public PlayerProfile[] GetProfiles(string type)
		{
			return Profiles.Values.Where(x => x.Type == type).Select(selector => selector.Profile).ToArray();
		}

		private class ProfilesFileFormat
		{
			public int Version = 1;
			public string SelectedProfile = string.Empty;
			public SavedProfile[] Profiles = new SavedProfile[0];
		}

		public class SavedProfile
		{
			public string Type;
			public PlayerProfile Profile;
		}
	}
}
