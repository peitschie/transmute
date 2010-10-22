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
using EmitMapper.AST.Helpers;
using System.Reflection.Emit;
using EmitMapper.AST;
using EmitMapper.AST.Nodes;
using EmitMapper.AST.Interfaces;
namespace Transmute.Builders
{
    public class DynamicMethodBuilder<TContext> : AbstractBuilder<TContext>
    {
        public DynamicMethodBuilder(IResourceMapper<TContext> mapper) : base(mapper)
        { }

        private static MethodInfo GetConvertMethod()
        {
            return typeof(IResourceMapper<TContext>).GetMethods().First(m => m.Name == "Map" && m.GetParameters().Length == 3);
        }

        public override MapperAction<TContext> BuildAction<TFrom, TTo>(IMappingCollection<TFrom, TTo, TContext> map)
        {
            ExportMapInformation(map);

            var convertMethod = _mapper.GetOrCreateConvertor(typeof(TFrom), typeof(TTo));
            var context = new CompilationContext(convertMethod.GetILGenerator());

//            if(map.UpdatesContext)
//            {
//                return (tfrom, tto, from, to, mapper, context) =>
//                    {
//                        if (to == null)
//                            to = mapper.ConstructOrThrow(tto);
//                        action(tfrom, tto, from, to, mapper, map.ContextUpdater(from, to, mapper, context));
//                        return to;
//                    };
//            }
//            else
//            {
//                return (tfrom, tto, from, to, mapper, context) =>
//                    {
//                        if (to == null)
//                            to = mapper.ConstructOrThrow(tto);
//                        action(tfrom, tto, from, to, mapper, context);
//                        return to;
//                    };
//            }

            new AstWriteArgument(1, typeof(TTo), new AstIfNull(
                (IAstRef)AstBuildHelper.ReadArgumentRA(1, typeof(TTo)), new AstNewObject(typeof(TTo), new IAstStackItem[0])))
                .Compile(context);

            foreach (var iteratingSetter in map.Setters.Where(s => !s.IsIgnored))
            {
                var setter = iteratingSetter;
                if(setter.Remap)
                {
                    _mapper.RequireOneWayMap(setter.SourceType, setter.DestinationType, typeof(TFrom), typeof(TTo));
                }
                switch(setter.SourceObjectType)
                {
                    case MemberEntryType.Function:
                        if(setter.Remap)
                        {

                        }
                        else
                        {

                        }
                        break;
                    case MemberEntryType.Member:
                        if(setter.Remap)
                        {
                            var remapMethod = AstBuildHelper.CallMethod(GetConvertMethod().MakeGenericMethod(setter.SourceType, setter.DestinationType),
                                                        AstBuildHelper.ReadArgumentRA(2, typeof(IResourceMapper<TContext>)),
                                                        new List<IAstStackItem>{
                                                            AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(0, typeof(TFrom)), setter.SourceMember),
                                                            AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(1, typeof(TTo)), setter.DestinationMember),
                                                            AstBuildHelper.ReadArgumentRA(3, typeof(TContext)),
                                                        });
                            var destination = AstBuildHelper.WriteMembersChain(setter.DestinationMember,
                                                                               AstBuildHelper.ReadArgumentRA(1, typeof(TTo)),
                                                                               remapMethod);
                            destination.Compile(context);
                        }
                        else
                        {
                            var source = AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(0, typeof(TFrom)), setter.SourceMember);
                            var destination = AstBuildHelper.WriteMembersChain(setter.DestinationMember,
                                                                               AstBuildHelper.ReadArgumentRA(1, typeof(TTo)),
                                                                               source);
                            destination.Compile(context);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("MemberEntryType not supported");
                }
            }

            new AstReturn { returnValue = AstBuildHelper.ReadArgumentRV(1, typeof(TTo)), returnType = typeof(TTo)}.Compile(context);
            var name = convertMethod.Name;
            return (tfrom, tto, from, to, mapper, contxt) => {
                return (TTo)mapper.Type.InvokeMember(name, BindingFlags.InvokeMethod, null, null,
                                             new object[]{from, to, mapper, contxt});
            };
        }
    }
}
