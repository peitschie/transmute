using System;
using System.Reflection;

namespace Transmute.MemberResolver
{
    public class IgnoreCaseNameMatcher : IMemberResolver
    {
        public bool IsSourceFor<TFrom, TTo, TContext>(MemberInfo dest, MemberInfo source, IMappingCollection<TFrom, TTo, TContext> mappers)
        {
            return string.Equals(dest.Name, source.Name, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}