
using System;
using System.Xml.Serialization;
namespace Transmute.Internal.Diagnostics
{
    public class MemberDescription
    {
        [XmlAttribute]
        public string type { get; set; }

        [XmlText]
        public string Name { get; set; }
    }
}
