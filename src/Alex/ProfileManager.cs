using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Alex.API.Services;
using Alex.Services;
using Alex.Utils;
using Newtonsoft.Json;
using NLog;

namespace Alex
{
	public class ProfileManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ProfileManager));
		private Dictionary<string, SavedProfile> Profiles { get; }
		//public SavedProfile ActiveProfile { get; private set; } = null;
		public SavedProfile LastUsedProfile { get; private set; } = null;
		private Alex Alex { get; }
		private IStorageSystem Storage { get; }
		public ProfileManager(Alex alex, IStorageSystem storage)
		{
			Alex = alex;
			Storage = storage;
			Profiles = new Dictionary<string, SavedProfile>();
		}

		private const string StatusMessage = "Loading profiles...";
		private const string ProfilesFile = "profiles";
		public void LoadProfiles(IProgressReceiver progressReceiver)
		{
			IPlayerProfileService profileService = Alex.Services.GetService<IPlayerProfileService>();
			
			progressReceiver.UpdateProgress(0, StatusMessage);
			if (Storage.TryRead(ProfilesFile, out ProfilesFileFormat saveFile))
			//if (File.Exists(ProfilesFile))
			{
				progressReceiver.UpdateProgress(50, StatusMessage);

			//	ProfilesFileFormat saveFile = null;
				SavedProfile[] profiles = null;
				try
				{
					//string contents = File.ReadAllText(ProfilesFile);
					//saveFile = JsonConvert.DeserializeObject<ProfilesFileFormat>(contents, new Texture2DJsonConverter(Alex.GraphicsDevice));
				}
				catch
				{
					Log.Warn($"Correct profiles savefile!");
				}

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
			}
			else
			{
				Storage.TryWrite(ProfilesFile, new ProfilesFileFormat());
			//	File.WriteAllText(ProfilesFile, JsonConvert.SerializeObject(new ProfilesFileFormat(), Formatting.Indented));
			}

			progressReceiver.UpdateProgress(100, StatusMessage);
		}

		public void SaveProfiles()
		{
			IPlayerProfileService profileService = Alex.Services.GetService<IPlayerProfileService>();
			Storage.TryWrite(ProfilesFile, new ProfilesFileFormat()
			{
				Profiles = Profiles.Values.ToArray(),
				SelectedProfile = profileService?.CurrentProfile?.Uuid ?? string.Empty
			});
		}

		public void CreateOrUpdateProfile(ProfileType type, PlayerProfile profile, bool setActive = false)
		{
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
			}

			Alex.UIThreadQueue.Enqueue(SaveProfiles);
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
