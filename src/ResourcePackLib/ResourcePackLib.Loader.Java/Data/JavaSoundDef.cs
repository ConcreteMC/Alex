using ResourcePackLib.Core;
using ResourcePackLib.Core.Utils;

namespace ResourcePackLib.Loader.Java.Data;

public class JavaSoundDef : ISoundDef
{
    public List<ISoundDefSound> Sounds { get; set; }
    
    public string Subtitle { get; set; }
    
    // TODO: Not defined in json, must be extracted from key
    public string Category { get; set; }
}