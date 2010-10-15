using System;
using Transmute.Internal.Utils;

namespace Transmute.Exceptions
{
    public class MemberMappingException : MappingException
    {
        private readonly object _fromMember;
        private readonly object _toMember;

        public MemberMappingException(Type from, Type to, object fromMember, object toMember, string message) : base(from, to, message + "({0} => {1})".With(from, to))
        {
            _fromMember = fromMember;
            _toMember = toMember;
        }

        public MemberMappingException(Type from, Type to, object fromMember, object toMember, string message, Exception innerException)
            : base(from, to, message, innerException)
        {
            _fromMember = fromMember;
            _toMember = toMember;
        }

        public object FromMember
        {
            get { return _fromMember; }
        }

        public object ToMember
        {
            get { return _toMember; }
        }
    }
}