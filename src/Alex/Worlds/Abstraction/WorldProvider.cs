using System;
using System.Threading.Tasks;
using Alex.API.Entities;
using Alex.API.Gui.Elements;
using Alex.API.World;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Abstraction
{
	public abstract class WorldProvider : IDisposable
	{
		public delegate void ProgressReport(LoadingState state, int percentage);

		protected World  World  { get; set; }
		public    ITitleComponent TitleComponent { get; set; }
		protected WorldProvider()
		{
			
		}

		public abstract Vector3 GetSpawnPoint();

		protected abstract void Initiate(out LevelInfo info);

		public void Init(World worldReceiver, out LevelInfo info)
		{
			World = worldReceiver;

			Initiate(out info);
		}

		public abstract Task Load(ProgressReport progressReport);

		public virtual void Dispose()
		{

		}
	}
}