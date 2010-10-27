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
    public class DelegateBuilder<TContext> : AbstractBuilder<TContext>
    {

        public DelegateBuilder(IResourceMapper<TContext> mapper) : base(mapper)
        { }

        public override void FinalizeBuilder()
        {
        }

        public override MapperAction<TContext> BuildAction<TFrom, TTo>(IMappingCollection<TFrom, TTo, TContext> map)
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
                var toSetter = setter.DestinationMember.CreateConstructingAccessorChain<TContext>(_mapper);
                var toAccessor = MapperUtils.CreateAccessorChain(setter.DestinationMember);
                switch(setter.SourceObjectType)
                {
                    case MemberEntryType.Function:
                        if(setter.Remap)
                        {
                            var remapper = _mapper.GetMapper(setter.SourceType, setter.DestinationType);
                            action += (from, to, context) => {
                                var dest = toAccessor.Get(to);
                                toSetter(to, remapper.MapObject(((MapperAction<TContext>)setter.SourceFunc)(from, to, context),
                                               dest, context), context);
                            };
                        }
                        else
                        {
                            action += (from, to, context) => toSetter(to, ((MapperAction<TContext>)setter.SourceFunc)(from, to, context), context);
                        }
                        break;
                    case MemberEntryType.Member:
                        var fromAccessor = MapperUtils.CreateAccessorChain(setter.SourceRoot.Union(setter.SourceMember));
                        if(setter.Remap)
                        {
                            var remapper = _mapper.GetMapper(setter.SourceType, setter.DestinationType);
                            action += (from, to, context) => toSetter(to, remapper.MapObject(fromAccessor.Get(from), toAccessor.Get(to), context), context);
                        }
                        else
                        {
                            action += (from, to, context) => toSetter(to, fromAccessor.Get(from), context);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("MemberEntryType not supported");
                }
            }

            if(action == null)
            {
                return (from, to, context) => to;
            }
            else if(map.UpdatesContext)
            {
                return (from, to, context) =>
                    {
                        if (to == null)
                            to = _mapper.ConstructOrThrow(typeof(TTo));
                        action(from, to, map.ContextUpdater(from, to, context));
                        return to;
                    };
            }
            else
            {
                return (from, to, context) =>
                    {
                        if (to == null)
                            to = _mapper.ConstructOrThrow(typeof(TTo));
                        action(from, to, context);
                        return to;
                    };
            }
        }
    }
}

