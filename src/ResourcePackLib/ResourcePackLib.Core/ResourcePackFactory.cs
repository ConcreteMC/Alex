namespace ResourcePackLib.Core;

public class ResourcePackFactory
{
    private List<IResourcePackReader> _readers = new List<IResourcePackReader>();

    public ResourcePackFactory()
    {
        
    }

    public void Register<TResourcePackReader>()
        where TResourcePackReader : IResourcePackReader, new()
    {
        Register(new TResourcePackReader());
    }

    public void Register(IResourcePackReader reader)
    {
        _readers.Add(reader);
    }
}