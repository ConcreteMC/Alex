using Microsoft.Xna.Framework;
using RocketUI.Input.Listeners;

namespace Alex.Common.Input
{
	public abstract class AlexInputListenerFactoryBase<TInputListener> : IInputListenerFactory
		where TInputListener : class, IInputListener
	{
		public AlexInputListenerFactoryBase() { }

		public virtual IInputListener CreateInputListener(PlayerIndex playerIndex)
		{
			var l = CreateInstance(playerIndex);
			RegisterMaps(l);

			return l;
		}

		protected abstract TInputListener CreateInstance(PlayerIndex playerIndex);

		protected virtual void RegisterMaps(TInputListener l) { }
	}
}