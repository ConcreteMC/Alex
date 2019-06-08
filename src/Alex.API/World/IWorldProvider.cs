using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alex.API.Blocks.State;
using Alex.API.Data;
using Alex.API.Entities;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Microsoft.Xna.Framework;

namespace Alex.API.World
{
	public abstract class WorldProvider : IDisposable
	{
		public delegate void ProgressReport(LoadingState state, int percentage);

		protected IWorldReceiver WorldReceiver { get; set; }
		protected IChatReceiver ChatReceiver { get; set; }
		public ITitleComponent TitleComponent { get; set; }
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

		public void SpawnEntity(long entityId, IEntity entity)
		{
			WorldReceiver.SpawnEntity(entityId, entity);
		}

		public void DespawnEntity(long entityId)
		{
			WorldReceiver.DespawnEntity(entityId);
		}

		public abstract Vector3 GetSpawnPoint();

		protected abstract void Initiate(out LevelInfo info, out IChatProvider chatProvider);

		public void Init(IWorldReceiver worldReceiver, IChatReceiver chat, out LevelInfo info, out IChatProvider chatProvider)
		{
			WorldReceiver = worldReceiver;
			ChatReceiver = chat;

			Initiate(out info, out chatProvider);
		}

		public abstract Task Load(ProgressReport progressReport);

		public virtual void Dispose()
		{

		}
	}

	public interface IWorldReceiver
	{
		IEntity GetPlayerEntity();

		IChunkColumn GetChunkColumn(int x, int z);
		void ChunkReceived(IChunkColumn chunkColumn, int x, int z, bool update);
		void ChunkUnload(int x, int z);
		void ChunkUpdate(IChunkColumn chunkColumn, ScheduleType type = ScheduleType.Lighting);

        void SpawnEntity(long entityId, IEntity entity);
		void DespawnEntity(long entityId);

		void UpdatePlayerPosition(PlayerLocation location);
		void UpdateEntityPosition(long entityId, PlayerLocation position, bool relative = false, bool updateLook = false, bool updatePitch = false);
		void UpdateEntityLook(long entityId, float yaw, float pitch, bool onGround);
        bool TryGetEntity(long entityId, out IEntity entity);
		
		void SetTime(long worldTime);
		void SetRain(bool raining);

		void SetBlockState(BlockCoordinates coordinates, IBlockState blockState);

		void AddPlayerListItem(PlayerListItem item);
		void RemovePlayerListItem(UUID item);
	};
}
