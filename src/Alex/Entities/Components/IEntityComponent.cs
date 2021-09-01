using Microsoft.Xna.Framework;

namespace Alex.Entities.Components
{
	public interface IEntityComponent
	{
		bool Enabled { get; }
		//void Update(GameTime gameTime);
	}
}