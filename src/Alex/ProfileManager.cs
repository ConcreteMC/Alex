using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NLog;

namespace Alex
{
	public class ProfileManager
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ProfileManager));
		private List<SavedProfile> Profiles { get; }
		public SavedProfile ActiveProfile { get; private set; } = null;
		public ProfileManager()
		{
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
					saveFile = JsonConvert.DeserializeObject<ProfilesFileFormat>(contents);
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
							if (profile.UUID.Equals(saveFile.SelectedProfile))
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

		public void CreateProfile(ProfileType type, string accessToken, string username, string rawUsername,
			string uuid, string clientToken, bool setActive = false)
		{
			SavedProfile profile = new SavedProfile();
			profile.Type = type;
			profile.AccessToken = accessToken;
			profile.Username = username;
			profile.RawUsername = rawUsername;
			profile.UUID = uuid;
			profile.ClientToken = clientToken;

			Profiles.Add(profile);
			if (setActive) ActiveProfile = profile;
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

			public string AccessToken;
			public string Username;
			public string RawUsername;
			public string UUID;
			public string ClientToken;
		}

		public enum ProfileType
		{
			Java = 0,
			Bedrock = 1
		}
	}
}
