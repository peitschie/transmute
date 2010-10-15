using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Transmute.Exceptions
{
    public class UnmappedMembersException : MappingException
    {
        private readonly IList<MemberInfo> _unmappedMembers;

        public UnmappedMembersException(Type from, Type to, IList<MemberInfo> unmappedMembers) : base(from, to, ToString(to, from, unmappedMembers))
        {
            _unmappedMembers = unmappedMembers;
        }

        public IList<MemberInfo> UnmappedMembers
        {
            get { return _unmappedMembers; }
        }

        private static string ToString(Type to, Type from, IEnumerable<MemberInfo> unmappedMembers)
        {
            return string.Format("One or more properties on {0} have no setters defined when mapped from {1}.  These should be explicitly ignored if unused.  Properties: {2}",
                to, from, string.Join(", ", unmappedMembers.Select(p => p.Name).ToArray()));
        }
    }
}