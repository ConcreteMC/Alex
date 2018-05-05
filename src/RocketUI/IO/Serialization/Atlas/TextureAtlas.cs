using System.Collections.Generic;
using System.Xml.Serialization;

namespace RocketUI.IO.Serialization.Atlas
{
    [XmlRoot(Namespace = "", ElementName = "TextureAtlas", DataType = "string", IsNullable = true)]
    public partial class TextureAtlas
    {
        [XmlAttribute(Namespace = "", AttributeName = "ImagePath")]
        public string ImagePath { get; set; }

        [XmlArray("TextureGroups")]
        [XmlArrayItem(Namespace = "", ElementName = "TextureGroup", Type = typeof(TextureAtlas.TextureGroup))]
        [XmlArrayItem(Namespace = "", ElementName = "Texture", Type = typeof(TextureAtlas.Texture))]
        [XmlArrayItem(Namespace = "", ElementName = "NinePatchTexture", Type = typeof(TextureAtlas.NinePatchTexture))]
        public List<TextureAtlas.Texture> TextureGroups { get; set; }
        
        [XmlArray("GuiTextures")]
        [XmlArrayItem(Namespace = "", ElementName = "GuiTexture", Type = typeof(TextureAtlas.GuiTexture))]
        public List<TextureAtlas.GuiTexture> GuiTextures { get; set; }
    }
}
