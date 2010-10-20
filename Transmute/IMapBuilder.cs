using System;
namespace Transmute
{
    public interface IMapBuilder<TContext>
    {
        MapperAction<TContext> BuildAction<TFrom, TTo>(IMappingCollection<TFrom, TTo, TContext> map);
    }
}

