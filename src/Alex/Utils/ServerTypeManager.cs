using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Alex.Common;
using Alex.Common.Data.Servers;
using Alex.Common.Services;
using Alex.Gui;
using Alex.Net;
using Alex.Utils.Auth;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Java;
using MojangAPI.Model;
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

	public class ServerTypeImplementation
	{
		public delegate void AuthenticationCallback(PlayerProfile profile);

		public IServerQueryProvider QueryProvider { get; }
		public string DisplayName { get; set; }
		internal string Id { get; set; }

		public ushort DefaultPort { get; protected set; } = 25565;
		public int ProtocolVersion { get; protected set; } = 0;
		
		public string TypeIdentifier { get; }
		public ServerTypeImplementation(IServerQueryProvider queryProvider, string displayName, string typeIdentifier)
		{
			DisplayName = displayName;
			QueryProvider = queryProvider;
			TypeIdentifier = typeIdentifier;
		}

		public virtual Task<ProfileUpdateResult> UpdateProfile(PlayerProfile session)
		{
			throw new NotImplementedException();
		}

		public virtual bool TryGetWorldProvider(ServerConnectionDetails connectionDetails, PlayerProfile playerProfile, out WorldProvider worldProvider, out NetworkProvider networkProvider)
		{
			worldProvider = null;
			networkProvider = null;
			return false;
		}

		public virtual Task<bool> VerifyAuthentication(PlayerProfile profile)
		{
			return Task.FromResult(false);
		}

		public virtual Task Authenticate(GuiPanoramaSkyBox skyBox, AuthenticationCallback callBack)
		{
			return Task.CompletedTask;
		}

		public SavedServerEntry[] SponsoredServers { get; protected set; } = new SavedServerEntry[0];
	}
}