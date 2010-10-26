using System;
namespace Transmute
{
    public interface IMap<TContext>
    {
        bool IsInitialized { get; }
        object MapObject(object from, object to, IResourceMapper<TContext> mapper, TContext context);
    }

    public interface IMap<TFrom, TTo, TContext> : IMap<TContext>
    {
        TTo Map(TFrom from, TTo to, IResourceMapper<TContext> mapper, TContext context);
    }
}

