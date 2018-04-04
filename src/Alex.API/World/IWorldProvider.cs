using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alex.API.Entities;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.World
{
	public abstract class WorldProvider : IDisposable
	{
		public delegate void ProgressReport(LoadingState state, int percentage);

		protected IWorldReceiver WorldReceiver { get; set; }
		protected WorldProvider()
		{
			
		}

		protected void LoadChunk(IChunkColumn chunk, int x, int z, bool update)
		{
			WorldReceiver.ChunkReceived(chunk, x, z, update);
		}

		protected void UnloadChunk(int x, int z)
		{
			WorldReceiver.ChunkUnload(x, z);
		}

		protected void SpawnEntity(long entityId, IEntity entity)
		{
			WorldReceiver.SpawnEntity(entityId, entity);
		}

		protected void DespawnEntity(long entityId)
		{
			WorldReceiver.DespawnEntity(entityId);
		}

		public abstract Vector3 GetSpawnPoint();

		protected abstract void Initiate(out LevelInfo info);

		public void Init(IWorldReceiver worldReceiver, out LevelInfo info)
		{
			WorldReceiver = worldReceiver;

			Initiate(out info);
		}

		public abstract Task Load(ProgressReport progressReport);

		public virtual void Dispose()
		{

		}
	}

	public interface IWorldReceiver
	{
		IEntity GetPlayerEntity();

		void ChunkReceived(IChunkColumn chunkColumn, int x, int z, bool update);
		void ChunkUnload(int x, int z);

		void SpawnEntity(long entityId, IEntity entity);
		void DespawnEntity(long entityId);

		void UpdatePlayerPosition(PlayerLocation location);
		void UpdateEntityPosition(long entityId, PlayerLocation position, bool relative = false);

		void SetTime(long worldTime);
	}
}
