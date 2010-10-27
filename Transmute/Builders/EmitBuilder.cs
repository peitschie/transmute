using System;
using System.Collections.Generic;
using EmitMapper;
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
    public class EmitBuilder<TContext> : AbstractBuilder<TContext>
    {
        private const string MapperField = "Mapper";
        private int _fieldIndex = 0;
        private readonly Dictionary<string, object> _constructorValues = new Dictionary<string, object>();
        private readonly TypeDictionary<string> _converterFields = new TypeDictionary<string>();
        private readonly TypeBuilder _type;

        public EmitBuilder(IResourceMapper<TContext> mapper) : base(mapper)
        {
            _type = DynamicAssemblyManager.DefineMapperType("ResourceMapper");
            _type.DefineField(MapperField, typeof(IResourceMapper<TContext>), FieldAttributes.Public | FieldAttributes.Static);
            _constructorValues.Add(MapperField, _mapper);
        }

        private static string GetMethodName(Type from, Type to)
        {
            return string.Format("Convert_{0}_{1}", from.Name, to.Name);
        }

        private MethodBuilder GetOrCreateConvertor(Type from, Type to)
        {
            var convertMethod = _type.DefineMethod(GetMethodName(from, to),
                                                          MethodAttributes.Public | MethodAttributes.Static, to,
                                      new[] { from, to, typeof(TContext) });
            convertMethod.DefineParameter(1, ParameterAttributes.None, "source");
            convertMethod.DefineParameter(2, ParameterAttributes.None, "destination");
            convertMethod.DefineParameter(3, ParameterAttributes.None, "context");
            return convertMethod;
        }

        private static Type GetMapType(Type from, Type to)
        {
            return typeof(IMap<,,>).MakeGenericType(from, to, typeof(TContext));
        }

        private static MethodInfo GetConvertMethod(Type from, Type to)
        {
            return GetMapType(from, to).GetMethod("Map");
        }

        private static MethodInfo GetConstructOrThrowMethod()
        {
            return typeof(IResourceMapper<TContext>).GetMethod("ConstructOrThrow");
        }

        private string GetFieldName(Type from, Type to)
        {
            return string.Format("{0}_{1}_{2}", from.Name, to.Name, _fieldIndex++);
        }

        private string GetFieldName<TFrom, TTo>()
        {
            return string.Format("{0}_{1}_{2}", typeof(TFrom).Name, typeof(TTo).Name, _fieldIndex++);
        }

        public override void FinalizeBuilder()
        {
            _type.CreateType();
            Activator.CreateInstance(_type, false);
            foreach(var entry in _constructorValues)
            {
                _type.GetField(entry.Key).SetValue(null, entry.Value);
            }
        }

        private string GetOrCreateMapper(Type from, Type to)
        {
            string converter;
            if(!_converterFields.TryGetValue(from, to, out converter))
            {
                converter = GetFieldName(from, to);
                var funcField = _type.DefineField(converter, GetMapType(from, to),
                                                 FieldAttributes.Public | FieldAttributes.Static);
                _constructorValues.Add(converter, _mapper.GetMapper(from, to));
            }
            return converter;
        }

        public override MapperAction<TContext> BuildAction<TFrom, TTo>(IMappingCollection<TFrom, TTo, TContext> map)
        {
            ExportMapInformation(map);

            var convertMethod = GetOrCreateConvertor(typeof(TFrom), typeof(TTo));
            var context = new CompilationContext(convertMethod.GetILGenerator());

            new AstWriteArgument(1, typeof(TTo), new AstIfNull(
                (IAstRef)AstBuildHelper.ReadArgumentRA(1, typeof(TTo)),
                AstBuildHelper.CastClass(AstBuildHelper.CallMethod(GetConstructOrThrowMethod(),
                    AstBuildHelper.ReadFieldRA(null, _type.GetField(MapperField)),
                    new List<IAstStackItem>{new AstTypeof{type = typeof(TTo)}}), typeof(TTo))
                )).Compile(context);

            if(map.UpdatesContext)
            {
                var funcField = _type.DefineField(GetFieldName<TFrom, TTo>(),
                                                         typeof(Func<object, object, TContext, TContext>),
                                                         FieldAttributes.Public | FieldAttributes.Static);
                _constructorValues.Add(funcField.Name, map.ContextUpdater);
//                        var sourceFuncRoot = setter.SourceRoot.Length > 0 ?
//                            AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(0, typeof(TFrom)), setter.SourceRoot)
//                            : AstBuildHelper.ReadArgumentRV(0, typeof(TFrom));
                var method = funcField.FieldType.GetMethod("Invoke",
                    new []{typeof(object), typeof(object), typeof(TContext)});

                var contextUpdater = AstBuildHelper.CallMethod(
                           method,
                           AstBuildHelper.ReadFieldRA(null, _type.GetField(funcField.Name)),
                           new List<IAstStackItem>{
                                AstBuildHelper.ReadArgumentRV(0, typeof(TFrom)),
                                AstBuildHelper.ReadArgumentRA(1, typeof(TTo)),
                                AstBuildHelper.ReadArgumentRA(2, typeof(TContext))});

                new AstWriteArgument(2, typeof(TContext), contextUpdater).Compile(context);
            }

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
                        var funcField = _type.DefineField(GetFieldName<TFrom, TTo>(), setter.SourceFunc.GetType(),
                                                                 FieldAttributes.Public | FieldAttributes.Static);
                        _constructorValues.Add(funcField.Name, setter.SourceFunc);
//                        var sourceFuncRoot = setter.SourceRoot.Length > 0 ?
//                            AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(0, typeof(TFrom)), setter.SourceRoot)
//                            : AstBuildHelper.ReadArgumentRV(0, typeof(TFrom));
                        var method = funcField.FieldType.GetMethod("Invoke", new []{typeof(object), typeof(object), typeof(TContext)});
                        var sourceFunc = AstBuildHelper.CallMethod(
                                   method,
                                   AstBuildHelper.ReadFieldRA(null, _type.GetField(funcField.Name)),
                                   new List<IAstStackItem>{
                                        AstBuildHelper.ReadArgumentRV(0, typeof(TFrom)),
                                        AstBuildHelper.ReadArgumentRA(1, typeof(TTo)),
                                        AstBuildHelper.ReadArgumentRA(2, typeof(TContext))});
                        if(setter.Remap)
                        {
                            string remapper = GetOrCreateMapper(setter.SourceType, setter.DestinationType);
                            var remapMethod = AstBuildHelper.CallMethod(GetConvertMethod(setter.SourceType, setter.DestinationType),
                                                        AstBuildHelper.ReadFieldRA(null, _type.GetField(remapper)),
                                                        new List<IAstStackItem>{
                                                            sourceFunc,
                                                            AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(1, typeof(TTo)), setter.DestinationMember),
                                                            AstBuildHelper.ReadArgumentRA(2, typeof(TContext)),
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
                            string remapper = GetOrCreateMapper(setter.SourceType, setter.DestinationType);
                            var remapMethod = AstBuildHelper.CallMethod(GetConvertMethod(setter.SourceType, setter.DestinationType),
                                                        AstBuildHelper.ReadFieldRA(null, _type.GetField(remapper)),
                                                        new List<IAstStackItem>{
                                                            sourceMember,
                                                            AstBuildHelper.ReadMembersChain(AstBuildHelper.ReadArgumentRA(1, typeof(TTo)), setter.DestinationMember),
                                                            AstBuildHelper.ReadArgumentRA(2, typeof(TContext)),
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
            Func<TFrom, TTo, TContext, TTo> converter = null;
            return (from, to, contxt) => {
                if(converter == null)
                {
                    converter = (Func<TFrom, TTo, TContext, TTo>)Delegate.CreateDelegate(
                                    typeof(Func<TFrom, TTo, TContext, TTo>), null,
                                    _type.GetMethod(name));
                }
                return converter((TFrom)from, (TTo)to, (TContext)contxt);
            };
        }
    }
}

