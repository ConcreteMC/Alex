using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Alex.Common.Services;
using Alex.Common.Utils;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Alex
{
	public class ProfileManager : IListStorageProvider<PlayerProfile>
	{
		public IStorageSystem StorageSystem { get; }
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ProfileManager));
		
		private ProfilesFileFormat CurrentFile { get; set; }
		public ProfileManager(IStorageSystem storageSystem)
		{
			StorageSystem = storageSystem;
			CurrentFile = new ProfilesFileFormat();
			
			Load();
		}

		private const string ProfilesFile = "profiles";
		private object _loadingLock = new object();

		public void CreateOrUpdateProfile(PlayerProfile profile, bool setActive = false)
		{
			lock (_loadingLock)
			{
				var matchingUuid = CurrentFile.Profiles.FirstOrDefault(x => x.UUID.Equals(profile.UUID));

				if (matchingUuid == default)
				{
					CurrentFile.Profiles.Add(profile);
				}
				else
				{
					var index = CurrentFile.Profiles.IndexOf(matchingUuid);


					if (index != -1)
					{
						CurrentFile.Profiles[index] = profile;
					}
				}

				if (setActive)
				{
					CurrentFile.SelectedProfile = profile.UUID;
				}
			}

			Save();
		}

		private class ProfilesFileFormat
		{
			public int Version = 1;
			public string SelectedProfile = string.Empty;
			public ObservableCollection<PlayerProfile> Profiles = new ObservableCollection<PlayerProfile>();
		}

		/// <inheritdoc />
		public IReadOnlyCollection<PlayerProfile> Data => CurrentFile.Profiles;

		/// <inheritdoc />
		public void Load()
		{
			lock (_loadingLock)
			{
				IStorageSystem storage = StorageSystem;

				if (storage.TryReadJson(ProfilesFile, out ProfilesFileFormat saveFile))
				{
					CurrentFile = saveFile;
				}
				else
				{
					storage.TryWriteJson(ProfilesFile, CurrentFile);
				}
			}
		}

		/// <inheritdoc />
		public void Save()
		{
			StorageSystem.TryWriteJson(ProfilesFile, CurrentFile);
		}
		
		private int GetIndexOf(PlayerProfile entry)
		{
			var newEntry = CurrentFile.Profiles.FirstOrDefault(x => x.UUID.Equals(entry.UUID));
			return CurrentFile.Profiles.IndexOf(newEntry);
		}

		/// <inheritdoc />
		public bool MoveUp(PlayerProfile entry)
		{
			lock (_loadingLock)
			{
				var currentIndex = GetIndexOf(entry);

				if (currentIndex == -1 || currentIndex == 0)
					return false;

				CurrentFile.Profiles.Move(currentIndex, currentIndex - 1);

				return true;
			}
		}

		/// <inheritdoc />
		public bool MoveDown(PlayerProfile entry)
		{
			lock (_loadingLock)
			{
				var currentIndex = GetIndexOf(entry);

				if (currentIndex == -1 || currentIndex == CurrentFile.Profiles.Count - 1)
					return false;

				CurrentFile.Profiles.Move(currentIndex, currentIndex + 1);

				return true;
			}
		}

		/// <inheritdoc />
		public void MoveEntry(int index, PlayerProfile entry)
		{
			lock (_loadingLock)
			{
				var oldIndex = GetIndexOf(entry);

				if (oldIndex == -1)
					return;

				CurrentFile.Profiles.Move(oldIndex, index);
			}
		}

		/// <inheritdoc />
		public void AddEntry(PlayerProfile entry)
		{
			CreateOrUpdateProfile(entry, false);
		}

		/// <inheritdoc />
		public bool RemoveEntry(PlayerProfile entry)
		{
			lock (_loadingLock)
			{
				var found = CurrentFile.Profiles.FirstOrDefault(x => x.UUID.Equals(entry.UUID));

				if (found != default)
				{
					CurrentFile.Profiles.Remove(found);

					if (CurrentFile.SelectedProfile == entry.UUID)
					{
						CurrentFile.SelectedProfile = CurrentFile.Profiles.FirstOrDefault()?.UUID;
					}

					return true;
				}

				return false;
			}
		}
	}
}
