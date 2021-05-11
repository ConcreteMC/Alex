using System;
using System.Collections.Concurrent;
using System.Linq;
using Alex.Gamestates.InGame;
using MiNET.Net;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock.Resources
{
	public class ResourcePackManager : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourcePackManager));

		private BedrockClient _client;
		private ConcurrentDictionary<string, ResourcePackEntry> _resourcePackEntries;
		private ResourceManager _resourceManager;

		public ResourcePackManager(BedrockClient client, ResourceManager resourceManager)
		{
			_client = client;
			_resourcePackEntries = new ConcurrentDictionary<string, ResourcePackEntry>();
			_resourceManager = resourceManager;
		}

		/// <summary>
		///		Are we waiting on resources?
		/// </summary>
		public bool WaitingOnResources => _resourcePackEntries.Any(x => !x.Value.IsComplete);

		/// <summary>
		///		The progress so far (value between 0 & 1)
		/// </summary>
		public float Progress => (1f / ExpectedDataSize) * TotalDataReceived;

		/// <summary>
		///		The expected amount of data for us to receive (in bytes)
		/// </summary>
		public long ExpectedDataSize => _resourcePackEntries.Sum(x => x.Value.ExpectedSize);

		/// <summary>
		///		The amount of data we have received so far (in bytes)
		/// </summary>
		public long TotalDataReceived => _resourcePackEntries.Sum(x => x.Value.TotalReceived);
		//	private ResourcePackIds _resourcePackIds;
		//	private ResourcePackIdVersions _resourcePackIdVersions;
		//	private ResourcePackIdVersions _texturePackIdVersions;

		public void HandleMcpeResourcePackStack(McpeResourcePackStack message)
		{
			Log.Info(
				$"Received ResourcePackStack/ (ForcedToAccept={message.mustAccept} Gameversion={message.gameVersion} Behaviorpacks={message.behaviorpackidversions.Count} Resourcepacks={message.resourcepackidversions.Count})");

			McpeResourcePackClientResponse response = McpeResourcePackClientResponse.CreateObject();
			response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed;
			//	response.resourcepackids = resourcePackIds;

			_client.SendPacket(response);
		}

		public void HandleMcpeResourcePacksInfo(McpeResourcePacksInfo message)
		{
			McpeResourcePackClientResponse response = McpeResourcePackClientResponse.CreateObject();
			ResourcePackIds resourcePackIds = new ResourcePackIds();

			foreach (var packInfo in message.texturepacks)
			{
				var entry = new TexturePackEntry(packInfo);
				if (_resourcePackEntries.TryAdd(entry.UUID, entry))
				{
					resourcePackIds.Add(entry.Identifier);
				}
			}

			foreach (var packInfo in message.behahaviorpackinfos)
			{
				var entry = new BehaviorPackEntry(packInfo);
				if (_resourcePackEntries.TryAdd(entry.UUID, entry))
				{
					resourcePackIds.Add(entry.Identifier);
				}
			}

			response.resourcepackids = resourcePackIds;

			//_resourcePackIds = resourcePackIds;

			if (resourcePackIds.Count > 0)
			{
				response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.SendPacks;
				//Log.Info($"Received resourcepack info, requesting data for {message.texturepacks.Count} texture packs & {message.behahaviorpackinfos.Count} behavior packs.");
			}
			else
			{
				response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.HaveAllPacks;

				//Log.Info($"Received resourcepack info, marking as HaveAllPacks");
			}

			_client.SendPacket(response);
		}

		public void HandleMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
		{
			var split = message.packageId.Split('_');

			if (!_resourcePackEntries.TryGetValue(split[0], out var packEntry))
			{
				Log.Warn($"Unknown resourcepack download: {message.packageId}");

				McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
				response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed;
				_client.SendPacket(response);

				return;
			}
			
			packEntry.SetDataInfo((ResourcePackType)message.packType, message.hash, message.chunkCount, message.maxChunkSize, message.compressedPackageSize, message.packageId);
			CheckCompletion(packEntry);
			//McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
			//	response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed;
			//_client.SendPacket(response);
		}

		private void CheckCompletion(ResourcePackEntry entry)
		{
			if (entry.IsComplete)
			{
				McpeResourcePackClientResponse response = McpeResourcePackClientResponse.CreateObject();
				response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed;
				response.resourcepackids = new ResourcePackIds() {entry.Identifier};
				_client.SendPacket(response);

				//_client.WorldProvider.Alex.Resources.
				Log.Info($"Completed pack, Identifier={entry.Identifier} (Received: {PlayingState.GetBytesReadable(entry.TotalReceived)}, Expected: {PlayingState.GetBytesReadable(entry.ExpectedSize)})");
				//TODO: Load the newly received resourcepack iinto the resourcemanager.

				if (entry is TexturePackEntry tpe)
				{
					//	Log.Info($"Texturepack contains {tpe.ResourcePack.Textures.Count} textures");
				}
				//_resourceManager
			}
			else
			{
				McpeResourcePackChunkRequest request = McpeResourcePackChunkRequest.CreateObject();
				request.chunkIndex = entry.ExpectedIndex;
				request.packageId = entry.PackageId;

				Log.Info(
					$"Requesting resource pack chunk, index={entry.ExpectedIndex}/{entry.ChunkCount} packageId={request.packageId} (Received: {PlayingState.GetBytesReadable(entry.TotalReceived)}, Expected: {PlayingState.GetBytesReadable(entry.ExpectedSize)})");

				_client.SendPacket(request);
			}
		}

		public void HandleMcpeResourcePackChunkData(McpeResourcePackChunkData message)
		{
			var split = message.packageId.Split('_');

			if (!_resourcePackEntries.TryGetValue(split[0], out var packEntry))
			{
				Log.Warn($"Unknown pack: {message.packageId}");

				return;
			}

			if (packEntry.SetChunkData(message.chunkIndex, message.payload)) { }

			//	Log.Info($"Received resourcepack chunk {message.chunkIndex + 1}/{packEntry.ChunkCount}");

			CheckCompletion(packEntry);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			var entries = _resourcePackEntries.ToArray();
			_resourcePackEntries.Clear();

			foreach (var entry in entries)
			{
				entry.Value?.Dispose();
			}
		}
	}
}