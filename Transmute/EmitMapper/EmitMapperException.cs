using System;

namespace EmitMapper
{
    public class EmitMapperException: ApplicationException
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