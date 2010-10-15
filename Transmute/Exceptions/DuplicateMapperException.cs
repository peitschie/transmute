using System;

namespace Transmute.Exceptions
{
    public class DuplicateMapperException : Exception
    {
        public DuplicateMapperException(Type type, Type type1, string previouslyDefined="unknown") 
            : base(string.Format("Mapping already exists for converting from {0} to {1}.{2}Previously defined at: {3}", type, type1, Environment.NewLine, previouslyDefined))
        {
        }
    }
}