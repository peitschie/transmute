using System.Linq;

namespace Transmute.Tests.Types
{
    public class IgnoreAllDefaultCreators<TFrom, TTo, TContext> : OneWayMap<TFrom, TTo, TContext>
    {
        public override void OverrideMapping(IMappingCollection<TFrom, TTo, TContext> mapping)
        {
            foreach (var propertyInfo in mapping.Unmapped.Destination.ToArray())
            {
                mapping.IgnoreMember(propertyInfo);
            }
        }
    }
}