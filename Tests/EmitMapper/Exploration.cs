using System;
using NUnit.Framework;
using EmitMapper.AST.Helpers;
using EmitMapper;
using System.Reflection;
using EmitMapper.AST;
using EmitMapper.AST.Nodes;
namespace Transmute.Tests.EmitMapper
{
    [TestFixture]
    public class Exploration
    {
        [Test]
        public void TestCase()
        {
            var source = AstBuildHelper.ReadMemberRV(AstBuildHelper.ReadArgumentRA(1, typeof(DestinationObject)),
                                                                          typeof(SourceObject).GetProperty("Source"));
            var writer = AstBuildHelper.WriteMember(typeof(DestinationObject).GetProperty("Destination"),
                                       AstBuildHelper.ReadArgumentRA(0, typeof(SourceObject)), source);
            var type = DynamicAssemblyManager.DefineMapperType("MyClassType");
            var convertMethod = type.DefineMethod("Convert",
                                                  MethodAttributes.Public | MethodAttributes.Static,
                                                  null, new Type[]{typeof(SourceObject), typeof(DestinationObject)});
            var param1 = convertMethod.DefineParameter(1, ParameterAttributes.None, "source");
            var param2 = convertMethod.DefineParameter(2, ParameterAttributes.None, "destination");
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

