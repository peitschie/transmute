using System;

namespace Transmute
{
    public interface ITwoWayMap<TContext>
    {
        Type Type1 { get; }
        Type Type2 { get; }
        IOneWayMap<TContext> Type1ToType2 { get; }
        IOneWayMap<TContext> Type2ToType1 { get; }
    }

    public interface ITwoWayMap<TType1, TType2, TContext> : ITwoWayMap<TContext>
    {
        new IOneWayMap<TType1, TType2, TContext> Type1ToType2 { get; }
        new IOneWayMap<TType2, TType1, TContext> Type2ToType1 { get; }
    }
}