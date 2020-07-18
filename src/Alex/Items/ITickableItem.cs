using Alex.Entities;

namespace Alex.Items
{
	public interface ITickableItem
	{
		void Tick(Entity entity);

		bool RequiresTick();
	}
}