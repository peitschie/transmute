
using System;
using System.Xml.Serialization;
namespace Transmute.Internal.Diagnostics
{
    [XmlType("Member")]
    public class MapMemberDescription
    {
        [XmlAttribute]
        public int order { get; set; }

        [XmlAttribute]
        public bool remapped { get; set; }

        public MemberDescription Destination { get; set; }

        public MemberDescription Source { get; set; }

        public string DefinedAt { get; set; }

        public string MemberConsumer { get; set; }
        public string MemberResolver { get; set; }
    }



}

