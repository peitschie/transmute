using System;
using Transmute.Tests.EmitMapper;
namespace Transmute.EmitHelper
{
    public sealed class MyClass
    {
        public static Func<int> Lambda;

        public static void Convert(SourceObject source, DestinationObject destination)
        {
            destination.Destination = source.Source;
        }

        public static void ConvertLambda(SourceObject source, DestinationObject destination)
        {
            destination.Destination = Lambda();
        }
    }
}

