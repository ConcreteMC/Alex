using System;
using System.Threading.Tasks;
using Alex.API.Entities;
using Alex.API.Gui.Elements;
using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.Worlds
{
	public abstract class WorldProvider : IDisposable
	{
		public delegate void ProgressReport(LoadingState state, int percentage);

		protected World  WorldReceiver  { get; set; }
		public    ITitleComponent TitleComponent { get; set; }
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

		public void Init(World worldReceiver, out LevelInfo info)
		{
			WorldReceiver = worldReceiver;

			Initiate(out info);
		}

		public abstract Task Load(ProgressReport progressReport);

		public virtual void Dispose()
		{

		}
	}
}