using System.Xml.Serialization;

namespace RocketUI.IO.Serialization.Atlas
{
    public partial class TextureAtlas
    {
        [XmlRoot(Namespace = "", ElementName = "NinePatchTexture")]
        public class NinePatchTexture : Texture
        {

            [XmlAttribute]
            public string Padding { get; set; }
        }
    }
}
