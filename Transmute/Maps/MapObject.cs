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
                _override.Clear();
                mappingCollection.DoAutomapping();
                mappingCollection.VerifyMap();

                if(_mapper.DiagnosticsEnabled && !string.IsNullOrEmpty(_mapper.ExportedMapsDirectory))
                {
                    var filename = string.Format("{0}_To_{1}.xml",
                                                 typeof(TFrom).FullName.Replace(".","-"),
                                                 typeof(TTo).FullName.Replace(".","-"));
                    Console.Out.WriteLine(filename);
                    var mapEntry = new TypeToTypeMap();
                    mapEntry.Members = mappingCollection.Setters.OrderBy(x => x.SetOrder).Select(s => new MapMemberDescription {
                            order = s.SetOrder,
                            Destination = new MemberDescription {
                                Name = s.DestinationMember.ToMemberName(),
                                type = s.DestinationType.FullName },
                            Source = new MemberDescription {
                                Name = s.SourceObjectType == MemberEntryType.Member ?
                                            ((MemberInfo[])s.SourceObject).ToMemberName()
                                            : "Custom function",
                                type = s.SourceType != null ? s.SourceType.FullName : null,
                                ignored = s.SourceObject == null ? "true" : null },
                            remapped = s.Remap
                        }).ToArray();

                    var serializer = new XmlSerializer(mapEntry.GetType());
                    using(var outputStream = XmlWriter.Create(Path.Combine(_mapper.ExportedMapsDirectory, filename),
                                                              new XmlWriterSettings{ Indent = true }))
                    {
                        serializer.Serialize(outputStream, mapEntry);
                    }
                }
                foreach (var iteratingSetter in mappingCollection.Setters.Where(s => s.SourceObject != null))
                {
                    var setter = iteratingSetter;
                    MemberSetterAction<TContext> action;
                    if(setter.Remap)
                    {
                        _mapper.RequireOneWayMap(setter.SourceType, setter.DestinationType, typeof(TFrom), typeof(TTo));
                    }
                    var toAccessor = MapperUtils.CreateAccessorChain(setter.DestinationMember);
                    switch(setter.SourceObjectType)
                    {
                        case MemberEntryType.Function:

                            if(setter.Remap)
                            {
                                action = new MemberSetter<TContext>(setter.DestinationMember,
                                    (from, to, mapper, context) => mapper.Map(setter.SourceType, setter.DestinationType,
                                    ((MemberSource<TContext>)setter.SourceObject)(from, to, mapper, context), toAccessor.Get(to), context)).GenerateCopyValueCall();
                            }
                            else
                            {
                                action = new MemberSetter<TContext>(setter.DestinationMember,
                                    ((MemberSource<TContext>)setter.SourceObject)).GenerateCopyValueCall();
                            }
                            break;
                        case MemberEntryType.Member:
                            var fromAccessor = MapperUtils.CreateAccessorChain(setter.SourceRoot.Union((MemberInfo[])setter.SourceObject));
                            if(setter.Remap)
                            {
                                action = new MemberSetter<TContext>(setter.DestinationMember,
                                    (from, to, mapper, context) => mapper.Map(setter.SourceType, setter.DestinationType,
                                        fromAccessor.Get(from), toAccessor.Get(to), context)).GenerateCopyValueCall();
                            }
                            else
                            {
                                action = new MemberSetter<TContext>(setter.DestinationMember,
                                    (from, to, mapper, context) => fromAccessor.Get(from)).GenerateCopyValueCall();
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("MemberEntryType not supported");
                    }
                    _setters.Add(action);
                }

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