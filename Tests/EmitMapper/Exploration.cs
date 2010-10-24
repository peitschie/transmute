using System;
using NUnit.Framework;
using EmitMapper.AST.Helpers;
using EmitMapper;
using System.Reflection;
using EmitMapper.AST;
using EmitMapper.AST.Nodes;
using System.Collections.Generic;
using EmitMapper.AST.Interfaces;
namespace Transmute.Tests.EmitMapper
{
    [TestFixture]
    [Explicit("Does not run under .NET... thought it does under mono.  Fun!")]
    public class Exploration
    {
        [Test]
        public void Convert_Method()
        {
            var source = AstBuildHelper.ReadMemberRV(AstBuildHelper.ReadArgumentRA(0, typeof(SourceObject)),
                                                                          typeof(SourceObject).GetProperty("Source"));
            var writer = AstBuildHelper.WriteMember(typeof(DestinationObject).GetProperty("Destination"),
                                       AstBuildHelper.ReadArgumentRA(1, typeof(DestinationObject)), source);
            var type = DynamicAssemblyManager.DefineMapperType("MyClassType");
            var convertMethod = type.DefineMethod("Convert",
                                                  MethodAttributes.Public | MethodAttributes.Static,
                                                  null, new Type[]{typeof(SourceObject), typeof(DestinationObject)});
            convertMethod.DefineParameter(1, ParameterAttributes.None, "source");
            convertMethod.DefineParameter(2, ParameterAttributes.None, "destination");
            var context = new CompilationContext(convertMethod.GetILGenerator());
            writer.Compile(context);
            new AstReturnVoid().Compile(context);
            type.CreateType();
            DynamicAssemblyManager.SaveAssembly();

            var sourceObj = new SourceObject{Source = 10};
            var destinationObj = new DestinationObject();
            object dynamicType = Activator.CreateInstance(type, false);
            type.InvokeMember("Convert", BindingFlags.InvokeMethod, null, dynamicType, new object[]{sourceObj, destinationObj});

            Assert.AreEqual(sourceObj.Source, destinationObj.Destination);
        }

        [Test]
        public void Convert_WithDelegate()
        {
            var type = DynamicAssemblyManager.DefineMapperType("MyClassType");
            var field = type.DefineField("Lambda", typeof(Func<int>), FieldAttributes.Static | FieldAttributes.Public);
            Func<int> getValue = () => 10;
            var source = AstBuildHelper.CallMethod(field.FieldType.GetMethod("Invoke", new Type[0]),
                                                   AstBuildHelper.ReadFieldRA(null, type.GetField("Lambda")), new List<IAstStackItem>());
            var writer = AstBuildHelper.WriteMember(typeof(DestinationObject).GetProperty("Destination"),
                                       AstBuildHelper.ReadArgumentRA(1, typeof(DestinationObject)), source);

            var convertMethod = type.DefineMethod("ConvertLambda",
                                                  MethodAttributes.Public | MethodAttributes.Static,
                                                  null, new Type[]{typeof(SourceObject), typeof(DestinationObject)});
            convertMethod.DefineParameter(1, ParameterAttributes.None, "source");
            convertMethod.DefineParameter(2, ParameterAttributes.None, "destination");
            var context = new CompilationContext(convertMethod.GetILGenerator());
            writer.Compile(context);
            new AstReturnVoid().Compile(context);
            type.CreateType();
            DynamicAssemblyManager.SaveAssembly();

            var destinationObj = new DestinationObject();
            object dynamicType = Activator.CreateInstance(type, false);
            type.GetField("Lambda").SetValue(dynamicType, getValue);
            type.InvokeMember("ConvertLambda", BindingFlags.InvokeMethod, null, dynamicType, new object[]{null, destinationObj});

            Assert.AreEqual(10, destinationObj.Destination);
        }

        [Test]
        public void Create_IfNull()
        {
            var type = DynamicAssemblyManager.DefineMapperType("MyClassType");
            var convertMethod = type.DefineMethod("Create_IfNull", MethodAttributes.Public, null,
                new []{typeof(SourceObject), typeof(DestinationObject), typeof(IResourceMapper<object>), typeof(object)});

            convertMethod.DefineParameter(1, ParameterAttributes.None, "source");
            convertMethod.DefineParameter(2, ParameterAttributes.None, "destination");
            convertMethod.DefineParameter(3, ParameterAttributes.None, "mapper");
            convertMethod.DefineParameter(4, ParameterAttributes.None, "context");
            var context = new CompilationContext(convertMethod.GetILGenerator());

            new AstWriteArgument(1, typeof(DestinationObject), new AstIfNull(
                (IAstRef)AstBuildHelper.ReadArgumentRA(1, typeof(DestinationObject)),
                new AstNewObject(typeof(DestinationObject), new IAstStackItem[0])))
                .Compile(context);
            new AstReturnVoid().Compile(context);
            type.CreateType();
            DynamicAssemblyManager.SaveAssembly();
        }

        [Test]
        public void Func_NotPassedByReference()
        {
            Func<int> func = () => 10;
            var store = new TestFunction(ref func);
            func = () => 12;
            Assert.AreNotEqual(12, store._method());
        }

        private class TestFunction
        {
            public Func<int> _method;

            public TestFunction(ref Func<int> method)
            {
                _method = method;
            }
        }
    }

    public class SourceObject
    {
        public int Source { get; set; }
    }

    public class DestinationObject
    {
        public int Destination { get; set; }
    }
}

