using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Alex.Common.Utils;
using Alex.Net.Bedrock;
using Alex.Utils.Caching;
using MiNET.Net;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock.Resources
{
	public class ResourcePackManager : IDisposable, IProgressReceiver
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourcePackManager));

		private BedrockClient _client;
		private ConcurrentDictionary<string, ResourcePackEntry> _resourcePackEntries;
		private ResourceManager _resourceManager;
		private ResourcePackCache _resourcePackCache;

		public ResourcePackManager(BedrockClient client,
			ResourceManager resourceManager,
			ResourcePackCache resourcePackCache)
		{
			_resourcePackCache = resourcePackCache;
			_client = client;
			_resourcePackEntries = new ConcurrentDictionary<string, ResourcePackEntry>();
			_resourceManager = resourceManager;
		}

		//public bool Loading { get; private set; } = false;
		public EventHandler<ResourceStatusChangedEventArgs> StatusChanged = null;

		private ResourceManagerStatus _status = ResourceManagerStatus.Initialized;

		public ResourceManagerStatus Status
		{
			get
			{
				return _status;
			}
			private set
			{
				StatusChanged?.Invoke(this, new ResourceStatusChangedEventArgs(value));
				_status = value;
			}
		}

		/// <summary>
		///		Are we waiting on resources?
		/// </summary>
		public bool WaitingOnResources => AcceptServerResources && _resourcePackEntries.Any(x => !x.Value.IsComplete);

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

		private static bool AcceptServerResources =>
			Alex.Instance.Options.AlexOptions.MiscelaneousOptions.LoadServerResources.Value;

		public int LoadingProgress { get; private set; } = 0;

		/// <summary>
		///		If true, client will try to download & decrypt server resources. Otherwise we will ignore encrypted packs.
		/// </summary>
		public static bool AcceptEncrypted { get; set; } = true;

		public void HandleMcpeResourcePackStack(McpeResourcePackStack message)
		{
			Log.Info($"Received ResourcePackStack/ (ForcedToAccept={message.mustAccept} Gameversion={message.gameVersion} Behaviorpacks={message.behaviorpackidversions.Count} Resourcepacks={message.resourcepackidversions.Count})");

			McpeResourcePackClientResponse response = McpeResourcePackClientResponse.CreateObject();
			response.responseStatus = (byte)McpeResourcePackClientResponse.ResponseStatus.Completed;
			//	response.resourcepackids = resourcePackIds;

			_client.SendPacket(response);
		}

		private bool RequestMissing(bool all = false)
		{
			var missing = _resourcePackEntries.Where(x => !x.Value.IsComplete).ToArray();

			if (missing.Length == 0)
				return false;

			McpeResourcePackClientResponse response = McpeResourcePackClientResponse.CreateObject();
			response.responseStatus = (byte)McpeResourcePackClientResponse.ResponseStatus.SendPacks;
			response.resourcepackids = new ResourcePackIds()
			{
				
			};

			if (all)
			{
				response.resourcepackids.AddRange(missing.Select(x => x.Value.Identifier));
			}
			else
			{
				response.resourcepackids.Add(missing.FirstOrDefault().Value.Identifier);
			}
			
			_client.SendPacket(response);
			
			return true;
		}

		private void SendHaveAllPacks()
		{
			McpeResourcePackClientResponse response = McpeResourcePackClientResponse.CreateObject();
			response.responseStatus = (byte)McpeResourcePackClientResponse.ResponseStatus.HaveAllPacks;
			response.resourcepackids = new ResourcePackIds();
			
			foreach(var entry in _resourcePackEntries)
				response.resourcepackids.Add(entry.Value.Identifier);
			
			_client.SendPacket(response);
		}
		
		public void HandleMcpeResourcePacksInfo(McpeResourcePacksInfo message)
		{
			//McpeResourcePackClientResponse response = McpeResourcePackClientResponse.CreateObject();
			//ResourcePackIds resourcePackIds = new ResourcePackIds();

			foreach (var packInfo in message.texturepacks)
			{
				if (!AcceptEncrypted && !string.IsNullOrWhiteSpace(packInfo.ContentKey))
				{
					Log.Info($"Skipping encrypted resourcepack: {packInfo.ContentIdentity}");

					continue;
				}

				var entry = new TexturePackEntry(packInfo);

				if (_resourcePackEntries.TryAdd(entry.UUID, entry))
				{
					//resourcePackIds.Add(entry.Identifier);
				}
			}

			foreach (var packInfo in message.behahaviorpackinfos)
			{
				if (!AcceptEncrypted && !string.IsNullOrWhiteSpace(packInfo.ContentKey))
				{
					Log.Info($"Skipping encrypted resourcepack: {packInfo.ContentIdentity}");

					continue;
				}

				var entry = new BehaviorPackEntry(packInfo);

				if (_resourcePackEntries.TryAdd(entry.UUID, entry))
				{
					//resourcePackIds.Add(entry.Identifier);
				}
			}

			//response.resourcepackids = resourcePackIds;

			//_resourcePackIds = resourcePackIds;

			if (AcceptServerResources && RequestMissing())
			{
			//	response.responseStatus = (byte)McpeResourcePackClientResponse.ResponseStatus.SendPacks;
				Status = ResourceManagerStatus.Downloading;
				Log.Info($"Received resourcepack info, requesting data for {message.texturepacks.Count} texture packs & {message.behahaviorpackinfos.Count} behavior packs.");
			}
			else
			{
				SendHaveAllPacks();
			
				Status = ResourceManagerStatus.Ready;
				Log.Info($"Received resourcepack info, marking as HaveAllPacks");
			}

			//_client.SendPacket(response);
		}

		public void HandleMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
		{
			var split = message.packageId.Split('_');

			if (!_resourcePackEntries.TryGetValue(split[0], out var packEntry))
			{
				Log.Warn($"Unknown resourcepack download: {message.packageId}");

				McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
				response.responseStatus = (byte)McpeResourcePackClientResponse.ResponseStatus.Completed;
				_client.SendPacket(response);

				return;
			}

			packEntry.SetDataInfo(
				(ResourcePackType)message.packType, message.hash, message.chunkCount, message.maxChunkSize,
				message.compressedPackageSize, message.packageId);

			if (_resourcePackCache.TryGet(packEntry.Identifier, out byte[] data))
			{
				if ((ulong)data.Length == message.compressedPackageSize)
				{
					packEntry.SetData(data);
				}
				else
				{
					_resourcePackCache.Remove(packEntry.Identifier);
				}
			}

			CheckCompletion(packEntry);
			//McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
			//	response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed;
			//_client.SendPacket(response);
		}

		private void CheckCompletion(ResourcePackEntry entry)
		{
			if (entry.IsComplete)
			{
				Log.Info(
					$"Completed pack, Identifier={entry.Identifier} (Received: {FormattingUtils.GetBytesReadable(entry.TotalReceived)}, Expected: {FormattingUtils.GetBytesReadable(entry.ExpectedSize)})");

				if (!WaitingOnResources && AcceptServerResources) //We got all packs.
				{
					McpeResourcePackClientResponse response = McpeResourcePackClientResponse.CreateObject();
					response.responseStatus = (byte)McpeResourcePackClientResponse.ResponseStatus.Completed;
					response.resourcepackids = new ResourcePackIds()
					{
						
					};

					/*if (d.Value != null)
					{
						response.resourcepackids.Add(d.Value.Identifier);
					}*/
					response.resourcepackids.AddRange(_resourcePackEntries.Select(x => x.Value.Identifier));
					
					_client.SendPacket(response);
					
					Log.Info($"All packs received, loading...");
					
					LoadingProgress = 0;
					Status = ResourceManagerStatus.Loading;

					ThreadPool.QueueUserWorkItem(
						_ =>
						{
							_resourceManager.ReloadBedrockResources(this);
							Status = ResourceManagerStatus.Ready;
							LoadingProgress = 0;
						});
				}
				else
				{
					RequestMissing();
				}
			}
			else
			{
				McpeResourcePackChunkRequest request = McpeResourcePackChunkRequest.CreateObject();
				request.chunkIndex = entry.ExpectedIndex;
				request.packageId = entry.PackageId;

				Log.Info(
					$"Requesting resource pack chunk, index={(entry.ExpectedIndex + 1)}/{entry.ChunkCount} packageId={request.packageId}");

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
			
			if (packEntry.SetChunkData(message.chunkIndex, message.payload, out byte[] completedData))
			{
				_resourcePackCache.TryStore(packEntry.Identifier, completedData);
			}
			
			Log.Info($"Received resourcepack chunk {message.chunkIndex + 1}/{packEntry.ChunkCount} (Received: {FormattingUtils.GetBytesReadable(packEntry.TotalReceived)}, Expected: {FormattingUtils.GetBytesReadable(packEntry.ExpectedSize)})");
			CheckCompletion(packEntry);
			

			//CheckCompletion(packEntry);
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

			//if (anyComplete)
			//	_resourceManager.ReloadBedrockResources(null);
		}

		public class ResourceStatusChangedEventArgs : EventArgs
		{
			public ResourceManagerStatus Status { get; }

			public ResourceStatusChangedEventArgs(ResourceManagerStatus status)
			{
				Status = status;
			}
		}

		public enum ResourceManagerStatus
		{
			Initialized,
			Downloading,
			Loading,
			Ready
		}

		/// <inheritdoc />
		public void UpdateProgress(int percentage, string statusMessage)
		{
			LoadingProgress = percentage;
		}

		/// <inheritdoc />
		public void UpdateProgress(int percentage, string statusMessage, string sub)
		{
			LoadingProgress = percentage;
		}
	}
}