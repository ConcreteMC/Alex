using Alex.API.Entities;

namespace Alex.API.World
{
    public interface IEntityProvider
    {

    }

	public interface IEntityHolder
	{
		bool TryGet(long entityId, out IEntity result);
	}
}
