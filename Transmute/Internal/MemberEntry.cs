using System;
using System.Reflection;
using System.Linq;
namespace Transmute.Internal
{
    public class MemberEntry
    {
        public MemberInfo[] DestinationMember { get; set; }
        public Type DestinationType { get; set; }
        
        public Type SourceType { get; set; }
        public MemberEntryType SourceObjectType { get; set; }
        public object SourceObject { get; set; }
        
        public bool Remap { get; set; }
        public bool IsMapped { get; set; }
        public int SetOrder { get; set; }
        
        public bool IsForMember(params MemberInfo[] member)
        {
            return DestinationMember.Select(m => m.Name).SequenceEqual(member.Select(m => m.Name)); 
        }
    }
    
    public enum MemberEntryType
    {
        Member,
        Function
    }
}

