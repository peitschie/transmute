using System.Reflection;

namespace Transmute.Internal
{
    public interface IAvailablePropertiesClass
    {
        MemberInfo[] Source { get; }
        MemberInfo[] Destination { get; }
    }
}