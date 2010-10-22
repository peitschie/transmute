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
    public abstract class AbstractBuilder<TContext> : IMapBuilder<TContext>
    {
        protected readonly IResourceMapper<TContext> _mapper;

        protected AbstractBuilder(IResourceMapper<TContext> mapper)
        {
            _mapper = mapper;
        }

        protected void ExportMapInformation<TFrom, TTo>(IMappingCollection<TFrom, TTo, TContext> map)
        {
           if(_mapper.DiagnosticsEnabled && !string.IsNullOrEmpty(_mapper.ExportedMapsDirectory))
           {
               var filename = string.Format("{0}_To_{1}.xml",
                                            typeof(TFrom).Name, typeof(TTo).Name);
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

        public abstract MapperAction<TContext> BuildAction<TFrom, TTo>(IMappingCollection<TFrom, TTo, TContext> map);
        public abstract void InitializeType();
    }
}

