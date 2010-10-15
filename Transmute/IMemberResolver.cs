using System.Reflection;

namespace Transmute
{
    public interface IMemberResolver
    {
        bool IsSourceFor<TFrom, TTo, TContext>(MemberInfo dest, MemberInfo source, IMappingCollection<TFrom, TTo, TContext> mappers);
    }
}