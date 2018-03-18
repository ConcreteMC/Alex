using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alex.API.Entities;
using Microsoft.Xna.Framework;

namespace Alex.API.World
{
	public abstract class WorldProvider : IDisposable
	{
		public delegate void ProgressReport(LoadingState state, int percentage);

		private IWorldReceiver WorldReceiver { get; set; }
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

		protected Vector3 GetPlayerPosition()
		{
			return WorldReceiver.RequestPlayerPosition();
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

		protected abstract void Initiate();

		public void Init(IWorldReceiver worldReceiver)
		{
			WorldReceiver = worldReceiver;

			Initiate();
		}

		public abstract Task Load(ProgressReport progressReport);

		public virtual void Dispose()
		{

		}
	}

	public interface IWorldReceiver
	{
		Vector3 RequestPlayerPosition();

		void ChunkReceived(IChunkColumn chunkColumn, int x, int z, bool update);
		void ChunkUnload(int x, int z);

		void SpawnEntity(long entityId, IEntity entity);
		void DespawnEntity(long entityId);
	}
}
