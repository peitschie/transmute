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
using System.Linq.Expressions;
namespace Transmute.Builders
{
    public class DynamicMethodBuilder<TContext> : AbstractBuilder<TContext>
    {
        private int _fieldIndex = 0;
        private Dictionary<string, object> _constructorValues = new Dictionary<string, object>();

        public DynamicMethodBuilder(IResourceMapper<TContext> mapper) : base(mapper)
        { }

        private static MethodInfo GetConvertMethod()
        {
            return typeof(IResourceMapper<TContext>).GetMethods().First(m => m.Name == "Map" && m.GetParameters().Length == 3);
        }

        private string GetFieldName<TFrom, TTo>()
        {
            return string.Format("{0}_{1}_{2}", typeof(TFrom).Name, typeof(TTo).Name, _fieldIndex++);
        }

        public override void InitializeType()
        {
            foreach(var entry in _constructorValues)
            {
                _mapper.Type.GetField(entry.Key).SetValue(null, entry.Value);
            }
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
                        var funcField = _mapper.Type.DefineField(GetFieldName<TFrom, TTo>(), setter.SourceFunc.GetType(),
                                                                 FieldAttributes.Public | FieldAttributes.Static);
                        _constructorValues.Add(funcField.Name, setter.SourceFunc);
//                        var sourceFuncRoot = setter.SourceRoot.Length > 0 ?
//                            AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(0, typeof(TFrom)), setter.SourceRoot)
//                            : AstBuildHelper.ReadArgumentRV(0, typeof(TFrom));
                        var method = funcField.FieldType.GetMethod("Invoke", new []{typeof(object), typeof(object), typeof(IResourceMapper<TContext>), typeof(TContext)});
                        Console.Out.WriteLine(method);
                        var sourceFunc = AstBuildHelper.CallMethod(
                                   method,
                                   AstBuildHelper.ReadFieldRA(null, _mapper.Type.GetField(funcField.Name)),
                                   new List<IAstStackItem>{
                                        AstBuildHelper.ReadArgumentRV(0, typeof(TFrom)),
                                        AstBuildHelper.ReadArgumentRA(1, typeof(TTo)),
                                        AstBuildHelper.ReadArgumentRA(2, typeof(IResourceMapper<TContext>)),
                                        AstBuildHelper.ReadArgumentRA(3, typeof(TContext))});
                        if(setter.Remap)
                        {
                            var remapMethod = AstBuildHelper.CallMethod(GetConvertMethod().MakeGenericMethod(setter.SourceType, setter.DestinationType),
                                                        AstBuildHelper.ReadArgumentRA(2, typeof(IResourceMapper<TContext>)),
                                                        new List<IAstStackItem>{
                                                            sourceFunc,
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
                            var destination = AstBuildHelper.WriteMembersChain(setter.DestinationMember,
                                                                               AstBuildHelper.ReadArgumentRA(1, typeof(TTo)),
                                                                               sourceFunc);
                            destination.Compile(context);
                        }
                        break;
                    case MemberEntryType.Member:
                        var sourceMember = AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(0, typeof(TFrom)),
                                                                         setter.SourceRoot.Union(setter.SourceMember).ToArray());
                        if(setter.Remap)
                        {
                            var remapMethod = AstBuildHelper.CallMethod(GetConvertMethod().MakeGenericMethod(setter.SourceType, setter.DestinationType),
                                                        AstBuildHelper.ReadArgumentRA(2, typeof(IResourceMapper<TContext>)),
                                                        new List<IAstStackItem>{
                                                            sourceMember,
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
                            var destination = AstBuildHelper.WriteMembersChain(setter.DestinationMember,
                                                                               AstBuildHelper.ReadArgumentRA(1, typeof(TTo)),
                                                                               sourceMember);
                            destination.Compile(context);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("MemberEntryType not supported");
                }
            }

            new AstReturn { returnValue = AstBuildHelper.ReadArgumentRV(1, typeof(TTo)), returnType = typeof(TTo)}.Compile(context);
            var name = convertMethod.Name;
            Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TTo> converter = null;
            return (tfrom, tto, from, to, mapper, contxt) => {
                if(converter == null)
                {
                    converter = (Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TTo>)Delegate.CreateDelegate(
                                    typeof(Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TTo>), null,
                                    mapper.Type.GetMethod(name));
                }
                return converter((TFrom)from, (TTo)to, (IResourceMapper<TContext>)mapper, (TContext)contxt);
            };
        }
    }
}

