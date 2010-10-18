using System;
using Transmute.Tests.EmitMapper;
namespace Transmute.EmitHelper
{
    public sealed class MyClass
    {
        public static void Convert(SourceObject source, DestinationObject destination)
        {
            destination.Destination = source.Source;
        }
    }
}

