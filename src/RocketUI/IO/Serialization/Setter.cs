using System.Xml.Serialization;

namespace RocketUI.IO.Serialization
{
    [XmlRoot]
    public class Setter
    {
        [XmlAttribute]
        public string Property { get; set; }

        [XmlAttribute]
        public object Value { get; set; }

    }
}
