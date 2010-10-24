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

