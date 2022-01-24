using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.Utils;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock.Resources
{
	public class ResourcePackEntry : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ResourcePackEntry));

		public string PackageId;
		public string Identifier;
		public string UUID;
		public string Version;

		public ResourcePackType PackType { get; set; }

		public ResourcePackEntry(string packUuid, string version)
		{
			UUID = packUuid;
			Version = version;

			PackageId = Identifier = $"{UUID}_{Version}";
		}

		//private byte[] _completedData = null;

		//public byte[] GetData()
		//{
		//	return _completedData;
		//}

		private byte[][] _chunks;
		private uint _maxChunkSize;
		private ulong _compressedPackageSize;

		public ulong ChunkCount { get; private set; } = 0;
		//private byte[] _hash = null;

		public void SetDataInfo(ResourcePackType packType,
			byte[] hash,
			uint messageChunkCount,
			uint messageMaxChunkSize,
			ulong messageCompressedPackageSize,
			string packageId)
		{
			PackageId = packageId;
			PackType = packType;
			//_hash = hash;

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

			Log.Info($"Downloading resources... ID={Identifier}");
			//Log.Info($"Resourcepack data info, packType={packType}");
		}

		public uint ExpectedIndex { get; set; } = 0;

		public bool SetChunkData(uint chunkIndex, byte[] chunkData, out byte[] completedData)
		{
			completedData = null;

			if (IsComplete)
				return false;

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
						_chunks[i] = null;
					}

					ms.Position = 0;

					byte[] buffer = new byte[_compressedPackageSize];
					ms.Read(buffer, 0, buffer.Length);

					//_completedData = buffer;

					//_completedData = ms.
					//_completedData = ms.Read().ToArray();

					completedData = buffer;

					if (Alex.Instance.Options.AlexOptions.MiscelaneousOptions.LoadServerResources.Value)
					{
						OnComplete(buffer);
					}
				}

				IsComplete = true;
			}

			return true;
		}

		public void SetData(byte[] data)
		{
			OnComplete(data);
			IsComplete = true;
		}

		protected virtual void OnComplete(byte[] data) { }

		public bool IsComplete { get; private set; } = false;

		/// <summary>
		///		The total amount of data received so far
		/// </summary>
		public long TotalReceived => _chunks == null ? 0 : (long)_chunks.Where(x => x != null).Sum(x => x.LongLength);

		/// <summary>
		///		The expected amount of data to come in.
		/// </summary>
		public long ExpectedSize => (long)_compressedPackageSize;

		protected virtual void Dispose(bool disposing) { }

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ResourcePackEntry()
		{
			Dispose(false);
		}
	}
}