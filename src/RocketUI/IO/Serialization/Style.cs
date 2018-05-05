using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace RocketUI.IO.Serialization
{
    [XmlRoot]
    [Serializable]
    public class Style
    {
        [XmlAttribute]
        public string Key { get; set; }

        [XmlAttribute]
        public string TargetType { get; set; }
        
        [XmlElement]
        public List<Setter> Setters { get; set; }

        public Style()
        {

        }
    }
}
