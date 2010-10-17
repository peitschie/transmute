using System;
using System.Reflection;
using System.Linq;
namespace Transmute.Internal
{
    public class MemberEntry
    {
        public MemberInfo[] DestinationMember { get; set; }
        public Type DestinationType { get; set; }

        public MemberInfo[] SourceRoot { get; set; }
        public Type SourceType { get; set; }
        public MemberEntryType SourceObjectType { get; set; }
        public object SourceObject { get; set; }
        
        public bool Remap { get; set; }
        public bool IsMapped { get; set; }
        public int SetOrder { get; set; }
        
        public bool IsForMember(MemberInfo[] prefix, params MemberInfo[] member)
        {
            return DestinationMember.Select(m => m.Name).SequenceEqual(prefix.Union(member).Select(m => m.Name));
        }

        public bool IsForPrefix(MemberInfo[] prefix)
        {
            return DestinationMember.Take(prefix.Length).Select(m => m.Name).SequenceEqual(prefix.Select(m => m.Name));
        }
    }
    
    public enum MemberEntryType
    {
        Member,
        Function
    }
}

