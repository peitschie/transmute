using System;
using Transmute.Tests.EmitMapper;
using Transmute.Tests.Integration;
namespace Transmute.EmitHelper
{
    public sealed class MyClass
    {
        public static Func<int> Lambda;
        public static MapperAction<object> SourceWithEnum_DestWithInt_0 = (from, to, context) => ((SourceWithEnum)from).Enum;

        public static void Convert(SourceObject source, DestinationObject destination)
        {
            destination.Destination = source.Source;
        }

        public static void ConvertLambda(SourceObject source, DestinationObject destination)
        {
            destination.Destination = Lambda();
        }

        public static void Test(SourceObject source, DestinationObject destination, object mapper, object context)
        {
            destination = destination ?? new DestinationObject();
        }

        public static void Convert_SourceWithEnum_DestWithInt(SourceWithEnum source, DestWithInt destination,
                                                              IResourceMapper<object> mapper, object context)
        {
            destination = destination ?? new DestWithInt();
            destination.Enum = mapper.Map(SourceWithEnum_DestWithInt_0(source, destination, context), destination.Enum, context);
        }
    }
}

