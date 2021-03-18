using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.IO;
using EasyPipes;
using MiNET.Net;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class ResourcePackManager : IDisposable
	{
		private static readonly Logger   Log = LogManager.GetCurrentClassLogger(typeof(ResourcePackManager));
		
		private BedrockClient                                   _client;
		private ConcurrentDictionary<string, ResourcePackEntry> _resourcePackEntries;
		private ResourceManager _resourceManager;
		public ResourcePackManager(BedrockClient client, ResourceManager resourceManager)
		{
			_client = client;
			_resourcePackEntries = new ConcurrentDictionary<string, ResourcePackEntry>();
			_resourceManager = resourceManager;
		}

		private ResourcePackIds _resourcePackIds;
		public void HandleMcpeResourcePackStack(McpeResourcePackStack message)
		{
			//Log.Info(
			//	$"Received ResourcePackStack, sending final response. (ForcedToAccept={message.mustAccept} Gameversion={message.gameVersion} Behaviorpacks={message.behaviorpackidversions.Count} Resourcepacks={message.resourcepackidversions.Count})");

			McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
			response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed;
			response.resourcepackids = _resourcePackIds;
			_client.SendPacket(response);
		}
		
		public void HandleMcpeResourcePacksInfo(McpeResourcePacksInfo message)
		{
			McpeResourcePackClientResponse response        = new McpeResourcePackClientResponse();
			ResourcePackIds                resourcePackIds = new ResourcePackIds();
			foreach (var packInfo in message.texturepacks)
			{
				resourcePackIds.Add($"{packInfo.UUID}_{packInfo.Version}");
				_resourcePackEntries.TryAdd(packInfo.UUID, new TexturePackEntry(packInfo));
			}

			foreach (var packInfo in message.behahaviorpackinfos)
			{
				resourcePackIds.Add($"{packInfo.PackIdVersion.Id}_{packInfo.PackIdVersion.Version}");
				_resourcePackEntries.TryAdd(packInfo.PackIdVersion.Id, new BehaviorPackEntry(packInfo));
			}

			_resourcePackIds = resourcePackIds;
	        
			if (resourcePackIds.Count > 0)
			{
				response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.SendPacks;
				response.resourcepackids = resourcePackIds;
				
				Log.Info($"Received resourcepack info, requesting data.");
			}
			else
			{
				response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.HaveAllPacks;
				response.resourcepackids = resourcePackIds;
				
				
				Log.Info($"Received resourcepack info, marking as HaveAllPacks");
			}

			_client.SendPacket(response);
		}

		public void HandleMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
		{
			var split = message.packageId.Split('_');
			if (!_resourcePackEntries.TryGetValue(split[0], out var packEntry))
			{
				Log.Warn($"Unknown resourcepack download: {split[0]}");
				
				McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
				response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed; 
				_client.SendPacket(response);
				
				return;
			}
			
			packEntry.SetDataInfo(message.chunkCount, message.maxChunkSize, message.compressedPackageSize);
			CheckCompletion(packEntry);
			//McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
		//	response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed;
			//_client.SendPacket(response);
		}

		private void CheckCompletion(ResourcePackEntry entry)
		{
			if (entry.IsComplete)
			{
				McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
				response.responseStatus = (byte) McpeResourcePackClientResponse.ResponseStatus.Completed;
				response.resourcepackids = new ResourcePackIds() {{$"{entry.Identifier}_{entry.Version}"}};
				_client.SendPacket(response);
				
				//_client.WorldProvider.Alex.Resources.
				Log.Info($"Completed pack: {entry.Identifier} (Size: {entry.GetData().Length})");
				
				//TODO: Load the newly received resourcepack iinto the resourcemanager.
			}
			else
			{
				McpeResourcePackChunkRequest request = McpeResourcePackChunkRequest.CreateObject();
				request.chunkIndex = entry.ExpectedIndex;
				request.packageId = entry.Identifier;
					
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
			
			if (packEntry.SetChunkData(message.chunkIndex, message.payload))
			{
				
			}
			
			Log.Info($"Received resourcepack chunk {message.chunkIndex + 1}/{packEntry.ChunkCount}");
			
			CheckCompletion(packEntry);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			
		}
	}

	public class ResourcePackEntry
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourcePackEntry));
		 
		public string Identifier;
		public string Version;
		public ResourcePackEntry(string packUuid, string version) {
			Identifier = packUuid;
			Version = version;
		}

		private byte[] _completedData = null;
		public byte[] GetData()
		{
			return _completedData;
		}
		
		private byte[][] _chunks;
		private uint     _maxChunkSize;
		private ulong    _compressedPackageSize;

		public ulong ChunkCount { get; private set; } = 0;
		public void SetDataInfo(uint messageChunkCount, uint messageMaxChunkSize, ulong messageCompressedPackageSize)
		{
			var chunkCount = messageCompressedPackageSize / messageMaxChunkSize;

			if (messageCompressedPackageSize % messageMaxChunkSize != 0)
				chunkCount++;

			ChunkCount = chunkCount;
			
			_chunks = new byte[chunkCount][];
			_maxChunkSize = messageMaxChunkSize;
			_compressedPackageSize = messageCompressedPackageSize;

			for (int i = 0; i < _chunks.Length; i++)
			{
				_chunks[i] = null;
			}

			ExpectedIndex = 0;
		}

		public uint ExpectedIndex { get; set; } = 0;
		public bool SetChunkData(uint chunkIndex, byte[] chunkData)
		{
			if (chunkIndex != ExpectedIndex)
			{
				Log.Warn($"Received wrong chunk index, expected={ExpectedIndex} received={chunkIndex}");
				return false;
			}

			ExpectedIndex++;
			_chunks[chunkIndex] = chunkData;

			if (_chunks.All(x => x != null))
			{
				using (MemoryStream ms = new MemoryStream())
				{
					for (int i = 0; i < _chunks.Length; i++)
					{
						ms.Write(_chunks[i]);
					}

					ms.Position = 0;
					
					byte[] buffer = new byte[_compressedPackageSize];
					ms.Read(buffer, 0, buffer.Length);
					_completedData = buffer;
					//_completedData = ms.
					//_completedData = ms.Read().ToArray();

					OnComplete(_completedData);
				}

				IsComplete = true;
			}

			return true;
		}
		
		protected virtual void OnComplete(byte[] data){}

		public bool IsComplete { get; private set; } = false;
		
		public IEnumerable<uint> GetMissingChunks()
		{
			if (_chunks == null)
				yield break;
			for (uint i = 0; i < _chunks.Length; i++)
			{
				if (_chunks[i] == null)
					yield return i;
			}
		}
	}
	
	public class TexturePackEntry : ResourcePackEntry
	{
		private static readonly Logger     Log = LogManager.GetCurrentClassLogger(typeof(TexturePackEntry));
		
		private TexturePackInfo     Info         { get; }
		public  BedrockResourcePack ResourcePack { get; private set; }
		/// <inheritdoc />
		public TexturePackEntry(TexturePackInfo info) : base(info.UUID, info.Version)
		{
			Info = info;
		}

		/// <inheritdoc />
		protected override void OnComplete(byte[] data)
		{
			base.OnComplete(data);

		/*	using (MemoryStream ms = new MemoryStream(data))
			{
				using (ZipFileSystem zfs = new ZipFileSystem(ms, Info.ContentIdentity))
				{
					BedrockResourcePack brp = new BedrockResourcePack(zfs);
					ResourcePack = brp;
				}
			}*/
			//File.WriteAllBytes($"Texture-{Info.UUID}.zip", data);
		}
	}
	
	public class BehaviorPackEntry : ResourcePackEntry
	{
		private ResourcePackInfo Info { get; }
		/// <inheritdoc />
		public BehaviorPackEntry(ResourcePackInfo info) : base(info.PackIdVersion.Id, info.PackIdVersion.Version)
		{
			Info = info;
		}
		
		protected override void OnComplete(byte[] data)
		{
			base.OnComplete(data);
			//File.WriteAllBytes($"Behavior-{Info.PackIdVersion.Id}.zip", data);
		}
	}
}