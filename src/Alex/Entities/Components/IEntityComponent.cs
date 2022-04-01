namespace Alex.Entities.Components
{
	public interface IEntityComponent
	{
		string Name { get; }
		bool Enabled { get; }
		//void Update(GameTime gameTime);
	}
}