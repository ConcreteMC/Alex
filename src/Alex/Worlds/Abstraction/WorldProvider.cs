using System;
using System.Threading.Tasks;
using RocketUI;
using Alex.API.World;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Abstraction
{
	public abstract class WorldProvider : IDisposable
	{
		public delegate void ProgressReport(LoadingState state, int percentage, string subTitle = null);

		protected World           World          { get; set; }
		public    ITitleComponent TitleComponent { get; set; }
		public    IChatRecipient  ChatRecipient  { get; set; }
		public    ScoreboardView  ScoreboardView { get; set; }
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

		public abstract LoadResult Load(ProgressReport progressReport);

		public virtual void Dispose()
		{

		}
	}

	public enum LoadResult 
	{
		Done,
		Failed,
		Timeout,
		Aborted
	}
}