using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Transmute.Exceptions;
using Transmute.Internal;
using Transmute.Internal.Utils;
using Transmute.Maps;
using Transmute.MemberResolver;
using System.IO;
using Transmute.Builders;
using System.Reflection.Emit;
using EmitMapper;

namespace Transmute
{
    public class ResourceMapper<TContext> : IResourceMapper<TContext>
        where TContext : class
    {
        private readonly object _initializeLock = new object();
        private readonly Dictionary<Type, Func<object>> _constructors = new Dictionary<Type, Func<object>>();
        private readonly TypeDictionary<MapperAction<TContext>> _mapCache = new TypeDictionary<MapperAction<TContext>>();
        private readonly TypeDictionary<RequiredMapEntry> _requiredMaps = new TypeDictionary<RequiredMapEntry>();
        private readonly List<MapInfoEntry> _mapCreationInfo = new List<MapInfoEntry>();
        private readonly List<ITypeMap<TContext>> _defaultMaps = new List<ITypeMap<TContext>>();
        private readonly List<ITypeMap<TContext>> _maps = new List<ITypeMap<TContext>>();
        private readonly PriorityList<IMemberConsumer> _memberConsumers = new PriorityList<IMemberConsumer>();
        private readonly PriorityList<IMemberResolver> _memberResolvers = new PriorityList<IMemberResolver>();
        private bool _diagnosticsEnabled = true;
        private IMapBuilder<TContext> _builder;
        private readonly TypeBuilder _dynamicType;

        public ResourceMapper()
        {
            _dynamicType = DynamicAssemblyManager.DefineMapperType("ResourceMapper_" + GetHashCode());
             _builder = new DynamicMethodBuilder<TContext>(this);
            //_builder = new DelegateBuilder<TContext>(this);
            _memberConsumers.Add(new DefaultMemberConsumer());
            _memberResolvers.Add(new IgnoreCaseNameMatcher());
            _defaultMaps.Add(new MapList<TContext>(this));
            _defaultMaps.Add(new MapArray<TContext>(this));
            _defaultMaps.Add(new MapEnum<TContext>());
            _defaultMaps.Add(new MapByVal<TContext>());
            _defaultMaps.Add(new MapNonNullableToNullable<TContext>(this));
            _defaultMaps.Add(new MapNullableToNullable<TContext>(this));
        }


        private static string GetMethodName(Type from, Type to)
        {
            return string.Format("Convert_{0}_{1}", from.Name, to.Name);
        }

        public MethodBuilder GetOrCreateConvertor(Type from, Type to)
        {
            var convertMethod = _dynamicType.DefineMethod(GetMethodName(from, to),
                                                          MethodAttributes.Public | MethodAttributes.Static, to,
                                      new []{from, to, typeof(IResourceMapper<TContext>), typeof(TContext)});
            convertMethod.DefineParameter(1, ParameterAttributes.None, "source");
            convertMethod.DefineParameter(2, ParameterAttributes.None, "destination");
            convertMethod.DefineParameter(3, ParameterAttributes.None, "mapper");
            convertMethod.DefineParameter(4, ParameterAttributes.None, "context");
            return convertMethod;
        }

        public bool CanConstruct(Type type)
        {
            return _constructors.ContainsKey(type);
        }

        private Func<object> GetConstructor(Type type)
        {
            Func<object> constructor;
            lock (_constructors)
            {
                if (!_constructors.TryGetValue(type, out constructor))
                {
                    var constructorInfo = type.DefaultConstructor();
                    constructor = constructorInfo != null ? constructorInfo.CompileConstructor() : null;
                    _constructors[type] = constructor;
                }
            }
            return constructor;
        }

        public object ConstructOrThrow(Type type)
        {
            AssertIsInitialized();
            var constructor = GetConstructor(type);
            if (constructor == null)
                throw new MapperException(string.Format("Unable to construct {0}: No default constructor found", type));
            return constructor();
        }

        public TypeBuilder Type { get { return _dynamicType; } }
        public object Instance { get; private set; }
        public IMapBuilder<TContext> Builder { get { return _builder; } }
        public string ExportedMapsDirectory { get; private set; }
        public bool DiagnosticsEnabled { get {return _diagnosticsEnabled;} }
        public bool IsInitialized { get; private set; }
        public IPriorityList<IMemberConsumer> MemberConsumers { get { return _memberConsumers; } }
        public IPriorityList<IMemberResolver> MemberResolvers { get { return _memberResolvers; } }
        public IList<ITypeMap<TContext>> TypeMaps { get { return _maps; } }

        #region ConvertUsing
        public void ConvertUsing<TFrom, TTo>(Func<TFrom, TTo> convert)
        {
            ConvertUsing<TFrom, TTo>((from, to, mapper, context) => convert(from));
        }

        public void ConvertUsing<TFrom, TTo>(Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TTo> convert)
        {
            AssertIsNotInitialized();
            AssertNoExistingMaps(typeof(TFrom), typeof(TTo));
            MapperAction<TContext> mapperAction = (tf, tt, from, to, mapper, context) => convert((TFrom)from, (TTo)to, mapper, context);
            _mapCache.Add(typeof(TFrom), typeof(TTo), mapperAction);
            if(_diagnosticsEnabled)
                _mapCreationInfo.Add(new MapInfoEntry(mapperAction));
        }

        public void ConvertUsing(Type from, Type to, Func<object, object> convert)
        {
            ConvertUsing(from, to, (ofrom, oto, mapper, context) => convert(ofrom));
        }

        public void ConvertUsing(Type from, Type to, Func<object, object, IResourceMapper<TContext>, TContext, object> convert)
        {
            AssertIsNotInitialized();
            AssertNoExistingMaps(from, to);
            MapperAction<TContext> mapperAction = (tf, tt, ofrom, oto, mapper, context) => convert(ofrom, oto, mapper, context);
            _mapCache.Add(from, to, mapperAction);
            if(_diagnosticsEnabled)
                _mapCreationInfo.Add(new MapInfoEntry(mapperAction));
        }
        #endregion

        #region RequireOneWayMap
        public void RequireOneWayMap(Type fromType, Type toType, Type fromParentType, Type toParentType)
        {
            RequireOneWayMap(fromType, toType, string.Format("{0} => {1}", fromParentType, toParentType));
        }

        public void RequireOneWayMap(Type fromType, Type toType, string description)
        {
            RequiredMapEntry existingEntry;
            if (!_requiredMaps.TryGetValue(fromType, toType, out existingEntry))
            {
                existingEntry = new RequiredMapEntry{Type1 = fromType, Type2 = toType};
                _requiredMaps.Add(fromType, toType, existingEntry);
            }
            if(_diagnosticsEnabled)
            {
                existingEntry.Messages.Add(description);
            }
        }

        public void RequireOneWayMap<TFrom, TTo, TFromParent, TToParent>()
        {
            RequireOneWayMap(typeof(TFrom), typeof(TTo), typeof(TFromParent), typeof(TToParent));
        }

        public void RequireOneWayMap<TFrom, TTo>(string description)
        {
            RequireOneWayMap(typeof(TFrom), typeof(TTo), description);
        }
        #endregion

        #region RequireTwoWayMap
        public void RequireTwoWayMap(Type type1, Type type2, Type parentType1, Type parentType2)
        {
            RequireOneWayMap(type1, type2, parentType1, parentType2);
            RequireOneWayMap(type2, type1, parentType2, parentType1);
        }

        public void RequireTwoWayMap(Type type1, Type type2, string description)
        {
            RequireOneWayMap(type1, type2, description);
            RequireOneWayMap(type2, type1, description);
        }

        public void RequireTwoWayMap<TType1, TType2, TParentType1, TParentType2>()
        {
            RequireTwoWayMap(typeof(TType1), typeof(TType2), typeof(TParentType1), typeof(TParentType2));
        }

        public void RequireTwoWayMap<TType1, TType2>(string description)
        {
            RequireTwoWayMap(typeof(TType1), typeof(TType2), description);
        }
        #endregion

        public bool CanMap(Type from, Type to)
        {
            return _mapCache.ContainsKey(from, to) || _maps.Any(m => m.CanMap(from, to)) || _defaultMaps.Any(m => m.CanMap(from, to));
        }

        public void RegisterMapping(ITypeMap<TContext> map)
        {
            AssertIsNotInitialized();
            _maps.Add(map);
            if(_diagnosticsEnabled)
                _mapCreationInfo.Add(new MapInfoEntry(map));
        }

        #region RegisterTwoWayMapping
        public void RegisterTwoWayMapping<TType1, TType2>(ITwoWayMap<TType1, TType2, TContext> overrides)
        {
            InternalRegisterOneWayMappingAction<TType1, TType2>(overrides.Type1ToType2.OverrideMapping);
            InternalRegisterOneWayMappingAction<TType2, TType1>(overrides.Type2ToType1.OverrideMapping);
        }

        public void RegisterTwoWayMapping<TFrom, TTo>()
        {
            InternalRegisterOneWayMappingAction<TFrom, TTo>(null);
            InternalRegisterOneWayMappingAction<TTo, TFrom>(null);
        }

        public void RegisterTwoWayMapping<TFrom, TTo>(Action<IMappingCollection<TFrom, TTo, TContext>> type1Overrides, Action<IMappingCollection<TTo, TFrom, TContext>> type2Overrides)
        {
            InternalRegisterOneWayMappingAction(type1Overrides);
            InternalRegisterOneWayMappingAction(type2Overrides);
        }

        public void RegisterTwoWayMapping<TMappingConvention>() where TMappingConvention : ITwoWayMap<TContext>, new()
        {
            var variable = new TMappingConvention();
            InternalNonGenericRegisterOneWayMappingInterface(variable.Type1, variable.Type2, variable.Type1ToType2);
            InternalNonGenericRegisterOneWayMappingInterface(variable.Type2, variable.Type1, variable.Type2ToType1);
        }
        #endregion

        #region RegisterOneWayMapping
        public void RegisterOneWayMapping(Type from, Type to)
        {
            InternalNonGenericRegisterOneWayMappingAction(from, to, null);
        }

        public void RegisterOneWayMapping<TFrom, TTo>()
        {
            InternalRegisterOneWayMappingAction<TFrom, TTo>(null);
        }

        public void RegisterOneWayMapping<TFrom, TTo>(Action<IMappingCollection<TFrom, TTo, TContext>> overrides)
        {
            InternalRegisterOneWayMappingAction(overrides);
        }

        public void RegisterOneWayMapping<TFrom, TTo>(IOneWayMap<TFrom, TTo, TContext> overrides)
        {
            InternalRegisterOneWayMappingAction<TFrom, TTo>(overrides.OverrideMapping);
        }

        public void RegisterOneWayMapping<TMappingConvention>() where TMappingConvention : IOneWayMap<TContext>, new()
        {
            var convention = new TMappingConvention();
            InternalNonGenericRegisterOneWayMappingInterface(convention.FromType, convention.ToType, convention);
        }
        #endregion

        private void InternalNonGenericRegisterOneWayMappingAction(Type from, Type to, object overrides)
        {
            try
            {
                GetType().GetMethod("InternalRegisterOneWayMappingAction", BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(from, to)
                    .Invoke(this, new[] {overrides});
            }
            catch(TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        private void InternalNonGenericRegisterOneWayMappingInterface(Type from, Type to, object overrides)
        {
            try
            {
                GetType().GetMethod("InternalRegisterOneWayMappingInterface", BindingFlags.Default | BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(from, to)
                    .Invoke(this, new[] { overrides });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        // ReSharper disable UnusedMember.Local
        private void InternalRegisterOneWayMappingInterface<TFrom, TTo>(IOneWayMap<TFrom, TTo, TContext> overrides)
        {
            InternalRegisterOneWayMappingAction<TFrom, TTo>(overrides.OverrideMapping);
        }
        // ReSharper restore UnusedMember.Local

        private void InternalRegisterOneWayMappingAction<TFrom, TTo>(Action<IMappingCollection<TFrom, TTo, TContext>> overrides)
        {
            AssertIsNotInitialized();
            StackTrace stackTrace = null;
            if(_diagnosticsEnabled)
            {
                stackTrace = CaptureStackTrace();
            }
            RequireOneWayMap<TFrom, TTo>(stackTrace != null ? stackTrace.ToString() : "unknown");
            var map = new MapObject<TFrom, TTo, TContext>(this);
            AssertNoExistingMaps(typeof(TFrom), typeof(TTo));
            RegisterMapping(map);
            if (overrides != null)
                map.AcceptOverrides(overrides);
        }

        public void RegisterConstructor(Type type, Func<object> constructor)
        {
            AssertIsNotInitialized();
            _constructors.Add(type, constructor);
        }

        public void RegisterConstructor<TType>(Func<TType> constructor)
        {
            AssertIsNotInitialized();
            _constructors.Add(typeof(TType), () => constructor());
        }

        public TTo Map<TFrom, TTo>(TFrom from, TTo to, TContext context)
        {
            return (TTo) Map(typeof (TFrom), typeof (TTo), from, to, context);
        }

        public TTo Map<TFrom, TTo>(TFrom from, TContext context)
        {
            return (TTo)Map(typeof(TFrom), typeof(TTo), from, default(TTo), context);
        }

        public object Map(Type fromType, Type toType, object from, object to, TContext context)
        {
            AssertIsInitialized();
            if (from == null)
            {
                to = toType.IsValueType ? Activator.CreateInstance(toType) : null;
                return to;
            }
            var setter = GetMapperFor(fromType, toType);
            to = setter(fromType, toType, from, to, this, context);
            return to;
        }

        private MapperAction<TContext> GetMapperFor(Type fromType, Type toType)
        {
            MapperAction<TContext> setter;
            if (!_mapCache.TryGetValue(fromType, toType, out setter))
            {
                var additionalInfo = string.Empty;
                if (CanMap(fromType, toType))
                {
                    additionalInfo = " A map creator is available, so it is likely a RequireMap call is missing on the resource mapper setup";
                }
                throw new MapperException(string.Format("Unable to map from {0} to {1}.{2}", fromType, toType, additionalInfo));
            }
            return setter;
        }

        public void DeactivateDiagnostics()
        {
            _diagnosticsEnabled = false;
        }

        public void ExportMapsTo(string filename)
        {
            _diagnosticsEnabled = true;
            ExportedMapsDirectory = filename;
        }

        public void InitializeMap()
        {
            if (IsInitialized)
                return;

            lock (_initializeLock)
            {
                if (IsInitialized)
                    return;

                if(_diagnosticsEnabled
                   && !string.IsNullOrEmpty(ExportedMapsDirectory)
                   && !Directory.Exists(ExportedMapsDirectory))
                {
                    Directory.CreateDirectory(ExportedMapsDirectory);
                }


                var missingMaps = new List<RequiredMapEntry>();
                var uninitialisedMaps = _requiredMaps.Where(e => !_mapCache.ContainsKey(e.Value.Type1, e.Value.Type2) 
                    && !missingMaps.Contains(e.Value)).ToArray();
                while (uninitialisedMaps.Length > 0)
                {
                    foreach (var requiredMap in uninitialisedMaps)
                    {
                        var fromType = requiredMap.Value.Type1;
                        var toType = requiredMap.Value.Type2;
                        // This will initialize the map cache for these items
                        var map = _maps.LastOrDefault(m => m.CanMap(fromType, toType)) 
                            ?? _defaultMaps.LastOrDefault(m => m.CanMap(fromType, toType));
                        if (map == null)
                        {
                            missingMaps.Add(requiredMap.Value);
                        }
                        else
                        {
                            var initializableMap = map as IInitializableMap;
                            if (initializableMap != null)
                                initializableMap.Initialize();
                            var setter = map.GetMapper(fromType, toType);
                            _mapCache.Add(fromType, toType, setter);
                        }
                    }
                    uninitialisedMaps = _requiredMaps.Where(e => !_mapCache.ContainsKey(e.Value.Type1, e.Value.Type2)
                        && !missingMaps.Contains(e.Value)).ToArray();
                }
                if(missingMaps.Count > 0)
                {
                    var reportString = new StringBuilder();
                    foreach (var missingMap in missingMaps)
                    {
                        reportString.AppendLine(string.Format("{0} to {1}: Required by {2}", missingMap.Type1, 
                            missingMap.Type2, string.Join(", ", missingMap.Messages.ToArray())));    
                    }
                    throw new MapperException(string.Format("Unable to complete maps.  One or more required maps could not be found.\n{0}", reportString));
                }
                Type.CreateType();
                Instance = Activator.CreateInstance(Type, false);
                _builder.InitializeType();
                IsInitialized = true;
            }
        }

        private void AssertIsNotInitialized()
        {
            if(IsInitialized)
                throw new MapperInitializedException();
        }

        private void AssertIsInitialized()
        {
            if (!IsInitialized)
                throw new MapperNotInitializedException();
        }

        private void AssertNoExistingMaps(Type fromType, Type toType)
        {
            var existing = _maps.FirstOrDefault(m => m.CanMap(fromType, toType));
            if (existing != null)
            {
                var info = _mapCreationInfo.FirstOrDefault(m => ReferenceEquals(m.Map, existing));
                throw new DuplicateMapperException(fromType, toType, info != null ? info.Creator.ToString() : "unknown");
            }

            MapperAction<TContext> setter;
            if (_mapCache.TryGetValue(fromType, toType, out setter))
            {
                var info = _mapCreationInfo.FirstOrDefault(m => ReferenceEquals(m.Map, setter));
                throw new DuplicateMapperException(fromType, toType, info != null ? info.Creator.ToString() : "unknown");
            }
        }

        private static StackTrace CaptureStackTrace()
        {
#if SILVERLIGHT
            return null;
#else
            var stackTrace = new StackTrace();
            int skipFrames = stackTrace.GetFrames().TakeWhile(stackFrame => stackFrame.GetMethod().DeclaringType.Assembly == typeof(MapHelpers).Assembly).Count();
            skipFrames--;
            return new StackTrace(skipFrames, true);
#endif
        }

        private class MapInfoEntry
        {
            private readonly object _map;
            private readonly StackTrace _creator;

            public MapInfoEntry(object map)
            {
                _map = map;
                _creator = CaptureStackTrace();
            }

            public object Map { get { return _map; } }
            public StackTrace Creator { get { return _creator; } }
        }
    }
}