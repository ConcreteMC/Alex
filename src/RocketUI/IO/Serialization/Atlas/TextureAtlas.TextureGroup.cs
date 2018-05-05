using System.Collections.Generic;
using System.Xml.Serialization;

namespace RocketUI.IO.Serialization.Atlas
{
    public partial class TextureAtlas
    {
        [XmlRoot(Namespace = "", ElementName = "TextureGroup")]
        public class TextureGroup : Texture
        {
            [XmlElement(Namespace = "", ElementName = "Texture", Type          = typeof(Texture))]
            [XmlElement(Namespace = "", ElementName = "NinePatchTexture", Type = typeof(NinePatchTexture))]
            public List<Texture> Variants { get; set; }
        }
    }
}
