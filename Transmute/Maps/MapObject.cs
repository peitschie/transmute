using System;
using System.Collections.Generic;
using System.Linq;
using Transmute.Internal;

namespace Transmute.Maps
{

    public class MapObject<TFrom, TTo, TContext> : IOverriddableTypeMap<TFrom, TTo, TContext>
    {
        private readonly IResourceMapper<TContext> _mapper;
        private readonly List<Action<IMappingCollection<TFrom, TTo, TContext>>> _override = new List<Action<IMappingCollection<TFrom, TTo, TContext>>>();
        private readonly List<MemberSetterAction<TContext>> _setters = new List<MemberSetterAction<TContext>>();
        private MemberSetterAction<TContext> _mapStatement;

        public MapObject(IResourceMapper<TContext> mapper)
        {
            _mapper = mapper;
        }

        public bool Initialized { get; private set; }

        public void Initialize()
        {
            if (!Initialized)
            {
                var mappingCollection = new MappingCollection<TFrom, TTo, TContext>(_mapper);
                foreach (var @override in _override)
                {
                    @override(mappingCollection);
                }
                mappingCollection.DoAutomapping();
                mappingCollection.VerifyMap();
                _setters.AddRange(mappingCollection.Setters.Select(s => s.GenerateCopyValueCall()).Where(n => n != null));
                _override.Clear();

                var generatedStatements = GenerateMapStatement(_setters);
                if(mappingCollection.UpdatesContext)
                {
                    _mapStatement = (tfrom, tto, from, to, mapper, context) =>
                        {
                            context = mappingCollection.ContextUpdater(from, to, mapper, context);
                            generatedStatements(tfrom, tto, from, to, mapper, context);
                        };
                }
                else
                {
                    _mapStatement = generatedStatements;
                }
                Initialized = true;
            }
        }

        public bool CanMap(Type from, Type to)
        {
            return Equals(typeof(TFrom), from) && Equals(typeof(TTo), to);
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            return Map;
        }

        private object Map(Type fromType, Type toType, object from, object to, IResourceMapper<TContext> mapper, TContext context)
        {
            if (to == null)
                to = mapper.ConstructOrThrow(toType);
            _mapStatement(fromType, toType, from, to, mapper, context);
            return to;
        }

        public void AcceptOverrides(Action<IMappingCollection<TFrom, TTo, TContext>> overrides)
        {
            _override.Add(overrides);
        }

        private static MemberSetterAction<TContext> GenerateMapStatement(IEnumerable<MemberSetterAction<TContext>> setters)
        {
            MemberSetterAction<TContext> combinedSetters = null;
            var first = true;
            foreach (var setter in setters)
            {
                if (first)
                {
                    combinedSetters = setter;
                    first = false;
                }
                else
                {
                    combinedSetters += setter;
                }
            }
            return combinedSetters ?? delegate { };
        }

        public Type FromType
        {
            get { return typeof (TFrom); }
        }

        public Type ToType
        {
            get { return typeof (TTo); }
        }
    }
}