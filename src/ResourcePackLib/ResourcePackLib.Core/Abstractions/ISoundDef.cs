namespace ResourcePackLib.Core;

public interface ISoundDef
{
    List<ISoundDefSound> Sounds { get; }
    
    string Category { get; }
    
    string Subtitle { get; }
}