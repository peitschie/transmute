using System;
using System.Collections.Generic;
using System.Linq;
using Transmute.Internal;
using System.Reflection;
using System.Xml;
using System.IO;
using Transmute.Internal.Diagnostics;
using Transmute.Internal.Utils;
using System.Xml.Serialization;

namespace Transmute.Maps
{

    public class MapObject<TFrom, TTo, TContext> : IOverriddableTypeMap<TFrom, TTo, TContext>
    {
        private readonly IResourceMapper<TContext> _mapper;
        private readonly List<Action<IMappingCollection<TFrom, TTo, TContext>>> _override = new List<Action<IMappingCollection<TFrom, TTo, TContext>>>();
        private MapperAction<TContext> _mapStatement;

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
                _override.Clear();
                mappingCollection.DoAutomapping();
                mappingCollection.VerifyMap();
                _mapStatement = _mapper.Builder.BuildAction(mappingCollection);
                Initialized = true;
            }
        }

        public bool CanMap(Type from, Type to)
        {
            return Equals(typeof(TFrom), from) && Equals(typeof(TTo), to);
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            return _mapStatement;
        }

        public void AcceptOverrides(Action<IMappingCollection<TFrom, TTo, TContext>> overrides)
        {
            _override.Add(overrides);
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