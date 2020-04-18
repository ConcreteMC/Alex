using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Services;
using Alex.API.Utils;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Alex
{
	public class ProfileManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ProfileManager));
		private Dictionary<string, SavedProfile> Profiles { get; }
		//public SavedProfile ActiveProfile { get; private set; } = null;
		public SavedProfile LastUsedProfile { get; private set; } = null;
		//private Alex Alex { get; }
		//private IStorageSystem Storage { get; }
		//private IPlayerProfileService ProfileService { get; }
		private IServiceProvider ServiceProvider { get; }
		public ProfileManager(IServiceProvider serviceProvider/*Alex alex, IStorageSystem storage, IPlayerProfileService playerProfileService*/)
		{
			//Alex = alex;
			//Storage = storage;
			Profiles = new Dictionary<string, SavedProfile>();
			ServiceProvider = serviceProvider;
			//ProfileService = playerProfileService;
		}

		private const string StatusMessage = "Loading profiles...";
		private const string ProfilesFile = "profiles";
		public void LoadProfiles(IProgressReceiver progressReceiver)
		{
			IPlayerProfileService profileService = ServiceProvider.GetRequiredService<IPlayerProfileService>();
			IStorageSystem storage = ServiceProvider.GetRequiredService<IStorageSystem>();
			
			progressReceiver.UpdateProgress(0, StatusMessage);
			if (storage.TryReadJson(ProfilesFile, out ProfilesFileFormat saveFile))
			//if (File.Exists(ProfilesFile))
			{
				progressReceiver.UpdateProgress(50, StatusMessage);

			//	ProfilesFileFormat saveFile = null;
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
							profile.Profile.IsBedrock = profile.Type == ProfileType.Bedrock;
							if (profile.Profile.Uuid.Equals(saveFile.SelectedProfile))
							{
								progressReceiver.UpdateProgress(90, StatusMessage);
								LastUsedProfile = profile;
								profileService.TryAuthenticateAsync(profile.Profile);
								//profileService.CurrentProfile = profile;
								break;
							}
						}
					}

					progressReceiver.UpdateProgress(99, StatusMessage);
					foreach (var profile in profiles)
					{
						Profiles.Add(profile.Profile.Uuid, profile);
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
			IPlayerProfileService profileService = ServiceProvider.GetRequiredService<IPlayerProfileService>();
			IStorageSystem storage = ServiceProvider.GetRequiredService<IStorageSystem>();
			
			//IPlayerProfileService profileService = Alex.Services.GetService<IPlayerProfileService>();
			storage.TryWriteJson(ProfilesFile, new ProfilesFileFormat()
			{
				Profiles = Profiles.Values.ToArray(),
				SelectedProfile = profileService?.CurrentProfile?.Uuid ?? string.Empty
			});
		}

		public void CreateOrUpdateProfile(ProfileType type, PlayerProfile profile, bool setActive = false)
		{
			IPlayerProfileService profileService = ServiceProvider.GetRequiredService<IPlayerProfileService>();
			var alex = ServiceProvider.GetRequiredService<Alex>();
			
			if (profile.Skin?.Texture == null)
			{
				profile.Skin = new Skin();

				if (alex.Resources.ResourcePack.TryGetBitmap("entity/alex", out var rawTexture))
				{
					profile.Skin.Texture = TextureUtils.BitmapToTexture2D(alex.GraphicsDevice, rawTexture);
					profile.Skin.Slim = true;
				}
			}
			SavedProfile savedProfile;
			if (Profiles.TryGetValue(profile.Uuid, out savedProfile))
			{
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
				//ActiveProfile = savedProfile;
				profileService?.Force(profile);
				//_playerProfileService.Force(profile);
			}

			alex.UIThreadQueue.Enqueue(SaveProfiles);
		}

		public PlayerProfile[] GetBedrockProfiles()
		{
			return Profiles.Values.Where(x => x.Type == ProfileType.Bedrock).Select(selector => selector.Profile).ToArray();
		}

		public PlayerProfile[] GetJavaProfiles()
		{
			return Profiles.Values.Where(x => x.Type == ProfileType.Java).Select(selector => selector.Profile).ToArray();
		}

		private class ProfilesFileFormat
		{
			public int Version = 1;
			public string SelectedProfile = string.Empty;
			public SavedProfile[] Profiles = new SavedProfile[0];
		}

		public class SavedProfile
		{
			public ProfileType Type;
			public PlayerProfile Profile;
		}

		public enum ProfileType
		{
			Java = 0,
			Bedrock = 1
		}
	}
}
