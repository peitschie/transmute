using System;

namespace Transmute.Maps
{
    public interface ITypeSpecificMap<TContext> : ITypeMap<TContext>
    {
        Type FromType { get; }
        Type ToType { get; }
    }
}