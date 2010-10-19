using System;
using System.Xml.Serialization;
namespace Transmute.Internal.Diagnostics
{
    [XmlRoot("Map")]
    public class TypeToTypeMap
    {
        public string DefinedAt { get; set; }
        public TypeToTypeName[] DependantMaps { get; set; }
        public TypeToTypeName[] RequiredTypes { get; set; }
        public MapMemberDescription[] Members { get; set; }
    }


}

