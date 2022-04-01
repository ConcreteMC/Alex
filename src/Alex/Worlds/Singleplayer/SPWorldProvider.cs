using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Alex.Common;
using Alex.Common.Data.Options;
using Alex.Common.Items;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Entities;
using Alex.Interfaces.Net;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Models;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using NLog;
using BlockFace = Alex.Interfaces.BlockFace;
using Player = Alex.Entities.Player;

namespace Alex.Worlds.Singleplayer
{
	internal class DebugNetworkProvider : NetworkProvider
	{
		public override bool IsConnected { get; } = true;

		protected override ConnectionInfo GetConnectionInfo()
		{
			return new ConnectionInfo(DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		}

		/// <inheritdoc />
		public override void PlayerOnGroundChanged(Player player, bool onGround) { }

		/// <inheritdoc />
		public override void EntityFell(long entityId, float distance, bool inVoid) { }

		public override void EntityAction(int entityId, EntityAction action) { }

		/// <inheritdoc />
		public override void PlayerAnimate(PlayerAnimations animation) { }

		public override void BlockPlaced(BlockCoordinates position,
			BlockFace face,
			int hand,
			int slot,
			Vector3 cursorPosition,
			Entity p) { }

		public override void PlayerDigging(DiggingStatus status,
			BlockCoordinates position,
			BlockFace face,
			Vector3 cursorPosition) { }

		public override void EntityInteraction(Entity player,
			Entity target,
			ItemUseOnEntityAction action,
			int hand,
			int slot,
			Vector3 cursorPosition) { }

		public override void WorldInteraction(Entity player,
			BlockCoordinates position,
			BlockFace face,
			int hand,
			int slot,
			Vector3 cursorPosition) { }

		public override void UseItem(Item item,
			int hand,
			ItemUseAction useAction,
			BlockCoordinates position,
			BlockFace face,
			Vector3 cursorPosition) { }

		public override void HeldItemChanged(Item item, short slot) { }

		/// <inheritdoc />
		public override void DropItem(BlockCoordinates position, BlockFace face, Item item, bool dropFullStack) { }

		public override void Close() { }

		/// <inheritdoc />
		public override void SendChatMessage(ChatObject message) { }

		/// <inheritdoc />
		public override void RequestRenderDistance(int oldValue, int newValue) { }
	}

	public class SPWorldProvider : WorldProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		private readonly IWorldGenerator _generator;
		private readonly List<ChunkCoordinates> _loadedChunks = new List<ChunkCoordinates>();

		private ChunkCoordinates PreviousChunkCoordinates { get; set; } =
			new ChunkCoordinates(int.MaxValue, int.MaxValue);

		private Alex Alex { get; }
		public NetworkProvider Network { get; } = new DebugNetworkProvider();

		private CancellationTokenSource ThreadCancellationTokenSource;

		private IOptionsProvider OptionsProvider { get; }
		private AlexOptions Options => OptionsProvider.AlexOptions;

		public SPWorldProvider(Alex alex, IWorldGenerator worldGenerator)
		{
			Alex = alex;
			OptionsProvider = Alex.ServiceContainer.GetRequiredService<IOptionsProvider>();

			_generator = worldGenerator;

			ThreadCancellationTokenSource = new CancellationTokenSource();
		}

		private IEnumerable<ChunkCoordinates> GenerateChunks(ChunkCoordinates center, int renderDistance)
		{
			var oldChunks = _loadedChunks.ToArray();

			List<ChunkCoordinates> newChunkCoordinates = new List<ChunkCoordinates>();

			int minZ = Math.Min(center.Z - renderDistance, center.Z + renderDistance);
			int maxZ = Math.Max(center.Z - renderDistance, center.Z + renderDistance);

			int minX = Math.Min(center.X - renderDistance, center.X + renderDistance);
			int maxX = Math.Max(center.X - renderDistance, center.X + renderDistance);

			//List<Task<ChunkColumn>> generatorTasks = new List<Task<ChunkColumn>>();
			for (int x = minX; x <= maxX; x++)
			for (int z = minZ; z <= maxZ; z++)
			{
				var cc = new ChunkCoordinates(x, z);
				newChunkCoordinates.Add(cc);

				var chunk = _generator.GenerateChunkColumn(cc);

				if (chunk == null) continue;

				base.World.ChunkManager.AddChunk(chunk, new ChunkCoordinates(chunk.X, chunk.Z), false);
				LoadEntities(chunk);

				yield return cc;
			}

			//Task.WaitAll(generatorTasks.ToArray());

			foreach (var chunk in oldChunks)
			{
				if (!newChunkCoordinates.Contains(chunk) && chunk != center)
				{
					//World.UnloadChunk(chunk);
					World.ChunkManager.RemoveChunk(chunk, false);
					_loadedChunks.Remove(chunk);
				}
			}
		}

		private void LoadEntities(ChunkColumn chunk)
		{
			/*var column = (ChunkColumn)chunk;
			if (column.Entities != null)
			{
				foreach (var nbt in column.Entities)
				{
					var eId = Interlocked.Increment(ref _spEntityIdCounter);
					if (EntityFactory.TryLoadEntity(nbt, eId, out Entity entity))
					{
						World.SpawnEntity(eId, entity);
					}
				}
			}*/
		}

		protected override void Initiate()
		{
			/*lock (genLock)
			{
				while (_preGeneratedChunks.TryDequeue(out ChunkColumn chunk))
				{
					if (chunk != null)
					{
						base.LoadChunk(chunk, chunk.X, chunk.Z, false);
						LoadEntities(chunk);
					}
				}

				_preGeneratedChunks = null;
			}*/

			World.Player.CanFly = true;
			World.Player.IsFlying = true;
			//world.Player.Controller.IsFreeCam = true;

			World.UpdatePlayerPosition(new PlayerLocation(GetSpawnPoint()));

			if (ItemFactory.TryGetItem("minecraft:diamond_sword", out var sword))
				World.Player.Inventory.SetSlot(World.Player.Inventory.HotbarOffset, sword, false);

			if (ItemFactory.TryGetItem("minecraft:grass_block", out var grass))
				World.Player.Inventory.SetSlot(World.Player.Inventory.HotbarOffset + 1, grass, false);
		}

		public override Vector3 GetSpawnPoint()
		{
			return _generator.GetSpawnPoint();
		}

		public override LoadResult Load(ProgressReport progressReport)
		{
			//	ChunkManager.DoMultiPartCalculations = false;


			int t = Options.VideoOptions.RenderDistance;
			double radiusSquared = Math.Pow(t, 2);

			var target = radiusSquared * 3;
			int count = 0;

			var pp = GetSpawnPoint();
			var center = new ChunkCoordinates(new PlayerLocation(pp.X, 0, pp.Z));

			Stopwatch sw = Stopwatch.StartNew();

			foreach (var cc in GenerateChunks(center, t))
			{
				count++;

				//base.World.ChunkManager.AddChunk(chunk, new ChunkCoordinates(chunk.X, chunk.Z), false);

				progressReport(LoadingState.LoadingChunks, (int)Math.Floor((count / target) * 100));
			}

			var loaded = sw.Elapsed;

			sw.Stop();

			Log.Info(
				$"Chunk pre-loading took {sw.Elapsed.TotalMilliseconds}ms (Loading: {loaded}ms Initializing: {(sw.Elapsed - loaded).TotalMilliseconds}ms)");

			PreviousChunkCoordinates = new ChunkCoordinates(new PlayerLocation(pp.X, pp.Y, pp.Z));

			World.Player.IsSpawned = true;

			//	UpdateThread = new Thread(RunThread) {IsBackground = true};

			//UpdateThread.Start();

			return LoadResult.Done;
		}

		public override void Dispose()
		{
			//ChunkManager.DoMultiPartCalculations = true;

			ThreadCancellationTokenSource?.Cancel();
			base.Dispose();
		}

		/// <inheritdoc />
		public override void OnTick()
		{
			if (!World.Player.IsSpawned)
			{
				return;
			}

			/*var e = base.WorldReceiver?.Player;
			if (e != null)
			{
				pp = e.KnownPosition;
			}*/
			//var pp = base.WorldReceiver.Player;
			ChunkCoordinates currentCoordinates = new ChunkCoordinates(World.Player.KnownPosition);

			if (PreviousChunkCoordinates != currentCoordinates)
			{
				PreviousChunkCoordinates = currentCoordinates;

				GenerateChunks(currentCoordinates, OptionsProvider.AlexOptions.VideoOptions.RenderDistance);

				//World.ChunkManager.FlagPrioritization();
			}
		}
	}
}