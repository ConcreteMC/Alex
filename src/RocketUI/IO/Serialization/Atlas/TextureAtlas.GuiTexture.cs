using System.Xml.Serialization;

namespace RocketUI.IO.Serialization.Atlas
{
    public partial class TextureAtlas
    {
        [XmlRoot(Namespace = "", ElementName = "GuiTexture")]
        public class GuiTexture
        {
            [XmlAttribute]
            public string TargetType { get; set; }
            [XmlAttribute]
            public string Property { get; set; }
            
            [XmlAttribute]
            public string TextureName { get; set; }

            [XmlAttribute]
            public string TextureVariant { get; set; } = "Default";

            [XmlAttribute]
            public TextureRepeatMode TextureRepeatMode { get; set; } = TextureRepeatMode.Stretch;
        }
    }
}
