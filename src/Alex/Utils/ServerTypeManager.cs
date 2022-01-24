using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Alex.Common;
using Alex.Common.Data.Servers;
using Alex.Common.Services;
using Alex.Gamestates.Multiplayer;
using Alex.Gui;
using Alex.Net;
using Alex.Utils.Auth;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Extensions.DependencyInjection;
using MojangAPI.Model;
using SimpleInjector;
using PlayerProfile = Alex.Common.Services.PlayerProfile;

namespace Alex.Utils
{
	public class ServerTypeManager
	{
		private Dictionary<string, ServerTypeImplementation> ServerTypes { get; }

		public ServerTypeManager()
		{
			ServerTypes = new Dictionary<string, ServerTypeImplementation>();
		}

		public bool TryRegister<T>(string id, T serverType) where T : ServerTypeImplementation
		{
			if (ServerTypes.TryAdd(id, serverType))
			{
				serverType.Id = id;

				return true;
			}

			return false;
		}

		public bool TryGet<T>(string id, out T serverTypeImplementation) where T : ServerTypeImplementation
		{
			if (ServerTypes.TryGetValue(id, out var impl) && impl is T implementation)
			{
				serverTypeImplementation = implementation;

				return true;
			}

			serverTypeImplementation = null;

			return false;
		}

		public bool TryGet(string id, out ServerTypeImplementation serverTypeImplementation)
		{
			return ServerTypes.TryGetValue(id, out serverTypeImplementation);
		}

		public IEnumerable<ServerTypeImplementation> GetAll()
		{
			return ServerTypes.Values.ToArray();
		}
	}

	public abstract class ServerTypeImplementation<TQueryProvider> : ServerTypeImplementation
		where TQueryProvider : IServerQueryProvider
	{
		/// <inheritdoc />
		protected ServerTypeImplementation(Container container, string displayName, string typeIdentifier) : base(
			container, container.GetRequiredService<TQueryProvider>(), displayName, typeIdentifier) { }
	}

	public abstract class ServerTypeImplementation
	{
		public IServerQueryProvider QueryProvider { get; }
		public string DisplayName { get; set; }
		internal string Id { get; set; }

		public ushort DefaultPort { get; protected set; } = 25565;
		public int ProtocolVersion { get; protected set; } = 0;

		public string TypeIdentifier { get; }

		public IListStorageProvider<SavedServerEntry> ServerStorageProvider { get; }
		public IListStorageProvider<PlayerProfile> ProfileProvider { get; }

		protected ServerTypeImplementation(Container container,
			IServerQueryProvider queryProvider,
			string displayName,
			string typeIdentifier)
		{
			var storage = container.GetRequiredService<IStorageSystem>();
			storage = storage.Open("storage", typeIdentifier);

			ServerStorageProvider = new SavedServerDataProvider(storage, "servers");
			ProfileProvider = new ProfileManager(storage.Open());

			DisplayName = displayName;
			QueryProvider = queryProvider;
			TypeIdentifier = typeIdentifier;
		}

		public virtual Task<ProfileUpdateResult> UpdateProfile(PlayerProfile session)
		{
			throw new NotImplementedException();
		}

		public virtual bool TryGetWorldProvider(ServerConnectionDetails connectionDetails,
			PlayerProfile playerProfile,
			out WorldProvider worldProvider,
			out NetworkProvider networkProvider)
		{
			worldProvider = null;
			networkProvider = null;

			return false;
		}

		public virtual Task Authenticate(GuiPanoramaSkyBox skyBox,
			UserSelectionState.ProfileSelected callBack,
			PlayerProfile profile)
		{
			return Task.CompletedTask;
		}

		public abstract Task<PlayerProfile> VerifySession(PlayerProfile profile);

		public SavedServerEntry[] SponsoredServers { get; protected set; } = new SavedServerEntry[0];
	}
}