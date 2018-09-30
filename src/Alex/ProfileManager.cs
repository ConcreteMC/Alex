using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Alex.API.Services;
using Alex.Utils;
using Newtonsoft.Json;
using NLog;

namespace Alex
{
	public class ProfileManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ProfileManager));
		private List<SavedProfile> Profiles { get; }
		public SavedProfile ActiveProfile { get; private set; } = null;

		private Alex Alex { get; }
		public ProfileManager(Alex alex)
		{
			Alex = alex;
			Profiles = new List<SavedProfile>();
		}

		private const string StatusMessage = "Loading profiles...";
		private const string ProfilesFile = "profiles.json";
		public void LoadProfiles(IProgressReceiver progressReceiver)
		{
			progressReceiver.UpdateProgress(0, StatusMessage);
			if (File.Exists(ProfilesFile))
			{
				progressReceiver.UpdateProgress(50, StatusMessage);

				ProfilesFileFormat saveFile = null;
				SavedProfile[] profiles = null;
				try
				{
					string contents = File.ReadAllText(ProfilesFile);
					saveFile = JsonConvert.DeserializeObject<ProfilesFileFormat>(contents, new Texture2DJsonConverter(Alex.GraphicsDevice));
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
							if (profile.Profile.Uuid.Equals(saveFile.SelectedProfile))
							{
								progressReceiver.UpdateProgress(90, StatusMessage);
								ActiveProfile = profile;
								break;
							}
						}
					}

					progressReceiver.UpdateProgress(99, StatusMessage);
					Profiles.AddRange(profiles);
				}
			}
			else
			{
				File.WriteAllText(ProfilesFile, JsonConvert.SerializeObject(new ProfilesFileFormat(), Formatting.Indented));
			}

			progressReceiver.UpdateProgress(100, StatusMessage);
		}

		public void SaveProfiles()
		{
			File.WriteAllText(ProfilesFile, JsonConvert.SerializeObject(new ProfilesFileFormat()
			{
				Profiles = Profiles.ToArray(),
				SelectedProfile = ActiveProfile?.Profile.Uuid ?? string.Empty
			}, Formatting.Indented, new Texture2DJsonConverter(Alex.GraphicsDevice)));
		}

		public void CreateOrUpdateProfile(ProfileType type, PlayerProfile profile, bool setActive = false)
		{
			SavedProfile savedProfile = new SavedProfile();
			savedProfile.Type = type;
			savedProfile.Profile = profile;

			Profiles.Add(savedProfile);
			if (setActive)
			{
				ActiveProfile = savedProfile;
			}

			Alex.UIThreadQueue.Enqueue(SaveProfiles);
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
