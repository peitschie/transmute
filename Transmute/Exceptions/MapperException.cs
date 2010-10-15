using System;

namespace Transmute.Exceptions
{
    public class MapperException : Exception
    {
        public MapperException(string message) : base(message)
        {}
    }
}