using System;
using System.Collections.Generic;
using Transmute.Builders;
using Transmute.Internal;
using Transmute.Maps;

namespace Transmute
{
    public interface IResourceMapper<TContext>
    {
        bool CanConstruct(Type type);
        object ConstructOrThrow(Type type);

        void ConvertUsing<TFrom, TTo>(Func<TFrom, TTo> convert);
        void ConvertUsing<TFrom, TTo>(Func<TFrom, TTo, TContext, TTo> convert);
        void ConvertUsing(Type from, Type to, Func<object, object> convert);
        void ConvertUsing(Type from, Type to, Func<object, object, TContext, object> convert);

        void RequireOneWayMap(Type fromType, Type toType, Type fromParentType, Type toParentType);
        void RequireOneWayMap(Type fromType, Type toType, string description);
        void RequireOneWayMap<TFrom, TTo, TFromParent, TToParent>();
        void RequireOneWayMap<TFrom, TTo>(string description);
        void RequireTwoWayMap(Type type1, Type type2, Type parentType1, Type parentType2);
        void RequireTwoWayMap(Type type1, Type type2, string description);
        void RequireTwoWayMap<TType1, TType2, TParentType1, TParentType2>();
        void RequireTwoWayMap<TType1, TType2>(string description);

        bool CanMap(Type from, Type to);
        IMap<TContext> GetMapper(Type from, Type to);
        IMap<TFrom, TTo, TContext> GetMapper<TFrom, TTo>();
        TTo Map<TFrom, TTo>(TFrom from, TTo to, TContext context);
        TTo Map<TFrom, TTo>(TFrom from, TContext context);
        object Map(Type fromType, Type toType, object from, object to, TContext context);

        void RegisterMapping(ITypeMap<TContext> overrides);

        void RegisterTwoWayMapping<TType1, TType2>(ITwoWayMap<TType1, TType2, TContext> overrides);
        void RegisterTwoWayMapping<TFrom, TTo>();
        void RegisterTwoWayMapping<TFrom, TTo>(Action<IMappingCollection<TFrom, TTo, TContext>> type1Overrides, Action<IMappingCollection<TTo, TFrom, TContext>> type2Overrides);
        void RegisterTwoWayMapping<TMappingConvention>() where TMappingConvention : ITwoWayMap<TContext>, new();

        void RegisterOneWayMapping<TFrom, TTo>(IOneWayMap<TFrom, TTo, TContext> overrides);
        void RegisterOneWayMapping(Type from, Type to);
        void RegisterOneWayMapping<TFrom, TTo>();
        void RegisterOneWayMapping<TFrom, TTo>(Action<IMappingCollection<TFrom, TTo, TContext>> overrides);
        void RegisterOneWayMapping<TMappingConvention>() where TMappingConvention : IOneWayMap<TContext>, new();

        void RegisterConstructor(Type type, Func<object> constructor);
        void RegisterConstructor<TType>(Func<TType> constructor);

        void DeactivateDiagnostics();
        void ExportMapsTo(string directory);

        void InitializeMap();

        IMapBuilder<TContext> Builder { get; }
        string ExportedMapsDirectory { get; }
        bool DiagnosticsEnabled { get; }
        bool IsInitialized { get; }
        IPriorityList<IMemberConsumer> MemberConsumers { get; }
        IPriorityList<IMemberResolver> MemberResolvers { get; }
        IList<ITypeMap<TContext>> TypeMaps { get; }
    }

    public enum MapBuilder
    {
        Delegate,
        Emit
    }
}