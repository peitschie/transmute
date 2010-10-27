using System;
namespace Transmute
{
    public interface IMap<TContext>
    {
        bool IsInitialized { get; }
        object MapObject(object from, object to, TContext context);
    }

    public interface IMap<TFrom, TTo, TContext> : IMap<TContext>
    {
        TTo Map(TFrom from, TTo to, TContext context);
    }
}

