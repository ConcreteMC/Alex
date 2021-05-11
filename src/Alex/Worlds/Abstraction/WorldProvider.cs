using System;
using System.Threading.Tasks;
using Alex.API.Gui.Elements;
using RocketUI;
using Alex.API.World;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Hud;
using Microsoft.Xna.Framework;

namespace Alex.Worlds.Abstraction
{
	public abstract class WorldProvider : IDisposable, ITicked
	{
		public delegate void ProgressReport(LoadingState state, int percentage, string subTitle = null);

		protected World           World          { get; set; }
		public    ITitleComponent TitleComponent { get; set; }
		public    IChatRecipient  ChatRecipient  { get; set; }
		public    ScoreboardView  ScoreboardView { get; set; }
		public    BossBarContainer  BossBarContainer { get; set; }
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

		/// <inheritdoc />
		public abstract void OnTick();
	}

	public enum LoadResult 
	{
		Done,
		Failed,
		ConnectionLost,
		LoginFailed,
		Timeout,
		Aborted
	}
}