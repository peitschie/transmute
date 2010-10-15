using System;

namespace Transmute.Exceptions
{
    public class MappingException : Exception
    {
        private readonly Type _from;
        private readonly Type _to;

        public MappingException(Type from, Type to, string message)
            : base(message)
        {
            _from = from;
            _to = to;
        }

        public MappingException(Type from, Type to, string message, Exception innerException)
            : base(message, innerException)
        {
            _from = from;
            _to = to;
        }

        public Type From
        {
            get { return _from; }
        }

        public Type To
        {
            get { return _to; }
        }
    }
}