using System;
using System.Collections.Generic;
using Transmute.Internal.Diagnostics;
using Transmute.Internal;
using System.Xml.Serialization;
using System.Linq;
using Transmute.Internal.Utils;
using System.Reflection;
using System.Xml;
using System.IO;
namespace Transmute.Builders
{
    public class DelegateBuilder<TContext> : IMapBuilder<TContext>
    {
        private readonly IResourceMapper<TContext> _mapper;

        public DelegateBuilder(IResourceMapper<TContext> mapper)
        {
            _mapper = mapper;
        }

        private void ExportMapInformation<TFrom, TTo>(IMappingCollection<TFrom, TTo, TContext> map)
        {
           if(_mapper.DiagnosticsEnabled && !string.IsNullOrEmpty(_mapper.ExportedMapsDirectory))
           {
               var filename = string.Format("{0}_To_{1}.xml",
                                            typeof(TFrom).FullName.Replace(".","-"),
                                            typeof(TTo).FullName.Replace(".","-"));
               var mapEntry = new TypeToTypeMap();
               mapEntry.Members = map.Setters.Select(s => new MapMemberDescription {
                       order = s.SetOrder,
                       name = s.DestinationMember.ToMemberName(),
                       type = s.DestinationType.FullName,
                       Source = s.IsIgnored ? null
                           : new MemberDescription {
                               name = s.SourceObjectType == MemberEntryType.Member ?
                                           s.SourceMember.ToMemberName()
                                           : null,
                               type = s.SourceType != null ? s.SourceType.FullName : null,
                               Function = s.SourceObjectType == MemberEntryType.Function ?
                                           "Custom Function"
                                           : null},
                       remapped = s.Remap
                   }).ToArray();

               var serializer = new XmlSerializer(mapEntry.GetType());
               using(var outputStream = XmlWriter.Create(Path.Combine(_mapper.ExportedMapsDirectory, filename),
                                                         new XmlWriterSettings{ Indent = true }))
               {
                   serializer.Serialize(outputStream, mapEntry);
               }
           }
        }

        public MapperAction<TContext> BuildAction<TFrom, TTo>(IMappingCollection<TFrom, TTo, TContext> map)
        {
            ExportMapInformation(map);

            MemberSetterAction<TContext> action = null;
            foreach (var iteratingSetter in map.Setters.Where(s => !s.IsIgnored))
            {
                var setter = iteratingSetter;
                if(setter.Remap)
                {
                    _mapper.RequireOneWayMap(setter.SourceType, setter.DestinationType, typeof(TFrom), typeof(TTo));
                }
                var toSetter = setter.DestinationMember.CreateConstructingAccessorChain<TContext>();
                var toAccessor = MapperUtils.CreateAccessorChain(setter.DestinationMember);
                switch(setter.SourceObjectType)
                {
                    case MemberEntryType.Function:
                        if(setter.Remap)
                        {
                            action += (tfrom, tto, from, to, mapper, context) => {
                                var dest = toAccessor.Get(to);
                                toSetter(to, mapper.Map(setter.SourceType, setter.DestinationType,
                                               ((MemberSource<TContext>)setter.SourceFunc)(from, to, mapper, context),
                                               dest, context), mapper, context);
                            };
                        }
                        else
                        {
                            action += (tfrom, tto, from, to, mapper, context) =>
                                toSetter(to, ((MemberSource<TContext>)setter.SourceFunc)(from, to, mapper, context), mapper, context);
                        }
                        break;
                    case MemberEntryType.Member:
                        var fromAccessor = MapperUtils.CreateAccessorChain(setter.SourceRoot.Union(setter.SourceMember));
                        if(setter.Remap)
                        {
                            action += (tfrom, tto, from, to, mapper, context) =>
                                toSetter(to, mapper.Map(setter.SourceType, setter.DestinationType,
                                                    fromAccessor.Get(from), toAccessor.Get(to), context), mapper, context);
                        }
                        else
                        {
                            action += (tfrom, tto, from, to, mapper, context) =>
                                toSetter(to, fromAccessor.Get(from), mapper, context);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("MemberEntryType not supported");
                }
            }

            if(map.UpdatesContext)
            {
                return (tfrom, tto, from, to, mapper, context) =>
                    {
                        if (to == null)
                            to = mapper.ConstructOrThrow(tto);
                        action(tfrom, tto, from, to, mapper, map.ContextUpdater(from, to, mapper, context));
                        return to;
                    };
            }
            else
            {
                return (tfrom, tto, from, to, mapper, context) =>
                    {
                        if (to == null)
                            to = mapper.ConstructOrThrow(tto);
                        action(tfrom, tto, from, to, mapper, context);
                        return to;
                    };
            }
        }
    }
}

