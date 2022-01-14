namespace ResourcePackLib.Core;

public interface ISoundDefSound
{
    string Name { get; }
    
    bool Stream { get; }
    
    bool LoadOnLowMemory { get; }
    
    float Volume { get; }
    
    int Weight { get; }
}