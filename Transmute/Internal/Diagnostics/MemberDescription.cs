
using System;
using System.Xml.Serialization;
namespace Transmute.Internal.Diagnostics
{
    public class MemberDescription
    {
        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        public string ignored { get; set; }

        [XmlText]
        public string Name { get; set; }
    }
}
