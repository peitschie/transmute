using System;

namespace EmitMapper
{
    public class EmitMapperException: Exception
    {
        public EmitMapperException()
        { 
        }

        public EmitMapperException(string message)
            : base(message)
        {
        }

        public EmitMapperException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}