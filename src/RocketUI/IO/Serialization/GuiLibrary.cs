using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using RocketUI.IO.Serialization.Atlas;

namespace RocketUI.IO.Serialization
{
    [XmlRoot]
    public class GuiLibrary
    {
        [XmlArray]
        [XmlArrayItem(typeof(TextureAtlas.Texture))]
        [XmlArrayItem(typeof(TextureAtlas.NinePatchTexture))]
        public List<TextureAtlas.Texture> Textures { get; set; }


    }
}
