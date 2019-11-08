using System;
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
		public ITitleComponent TitleComponent { get; set; }
        protected WorldProvider()
		{
			
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

		IChunkColumn GetChunkColumn(int x, int z);
		
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
