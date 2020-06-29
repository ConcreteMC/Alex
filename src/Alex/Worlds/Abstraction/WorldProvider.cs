using System;
using System.Threading.Tasks;
using Alex.API.Entities;
using Alex.API.Gui.Elements;
using Alex.API.World;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Abstraction
{
	public abstract class WorldProvider : IDisposable
	{
		public delegate void ProgressReport(LoadingState state, int percentage);

		protected World  World  { get; set; }
		public    ITitleComponent TitleComponent { get; set; }
		public ScoreboardView ScoreboardView { get; set; }
		protected WorldProvider()
		{
			
		}

		public abstract Vector3 GetSpawnPoint();

		protected abstract void Initiate();

		public void Init(World worldReceiver)
		{
			World = worldReceiver;

			Initiate();
		}

		public abstract Task Load(ProgressReport progressReport);

		public virtual void Dispose()
		{

		}
	}
}