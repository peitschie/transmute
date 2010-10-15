using System;

namespace Transmute.Internal.FastMemberAccessor
{
    public class FuncBasedAccessor : IMemberAccessor
    {
        private readonly Func<object, object> _getter;
        private readonly Action<object, object> _setter;
        private readonly Type _type;
        private readonly Type _reflectedType;

        public FuncBasedAccessor(Func<object, object> getter, Action<object, object> setter, Type type, Type reflectedType)
        {
            _getter = getter;
            _setter = setter;
            _type = type;
            _reflectedType = reflectedType;
        }

        public object Get(object target)
        {
            return _getter(target);
        }

        public void Set(object target, object value)
        {
            _setter(target, value);
        }

        public Type MemberType { get { return _type; } }

        public Type ReflectedType { get { return _reflectedType; } }
    }
}