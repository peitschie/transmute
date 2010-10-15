using System;

namespace Transmute.Maps
{
    public interface ITypeMap<TContext>
    {
        bool CanMap(Type from, Type to);
        MapperAction<TContext> GetMapper(Type fromType, Type toType);
    }
}