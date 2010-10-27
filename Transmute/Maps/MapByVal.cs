using System;
using System.Linq;

namespace Transmute.Maps
{
    public class MapByVal<TContext> : ITypeMap<TContext>
    {
        private static readonly Type[] ImmutableTypes = new[] { typeof(string), };
        
        public static bool IsValType(Type type)
        {
            return type.IsValueType || ImmutableTypes.Any(t => t == type);
        }

        public bool CanMap(Type from, Type to)
        {
            return to.IsAssignableFrom(from) && IsValType(from);
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            return (from, to, context) => from;
        }
    }
}