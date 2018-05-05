using System.Xml.Serialization;

namespace RocketUI.IO.Serialization.Atlas
{
    public partial class TextureAtlas
    {
        [XmlRoot(Namespace = "", ElementName = "Texture")]
        public class Texture
        {
            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public string Variant { get; set; } = "Default";

            [XmlAttribute]
            public int X { get; set; }

            [XmlAttribute]
            public int Y { get; set; }

            [XmlAttribute]
            public int Width { get; set; }

            [XmlAttribute]
            public int Height { get; set; }

        }
    }
}
