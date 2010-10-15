using System;

namespace Transmute
{
    public interface IOneWayMap<TContext>
    {
        Type FromType { get; }
        Type ToType { get; }
    }

    public interface IOneWayMap<TFrom, TTo, TContext> : IOneWayMap<TContext>
    {
        void OverrideMapping(IMappingCollection<TFrom, TTo, TContext> mapping);
    }
}