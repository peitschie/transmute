using System;

namespace Transmute.Maps
{
    public interface IOverriddableTypeMap<TFrom, TTo, TContext> : ITypeSpecificMap<TContext>, IInitializableMap
    {
        void AcceptOverrides(Action<IMappingCollection<TFrom, TTo, TContext>> overrides);
    }
}