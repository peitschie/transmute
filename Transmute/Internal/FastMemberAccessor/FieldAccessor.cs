using System;
using System.Reflection;

namespace Transmute.Internal.FastMemberAccessor
{
    public class FieldAccessor : IMemberAccessor
    {
        private readonly FieldInfo _field;

        public FieldAccessor(FieldInfo field)
        {
            _field = field;
        }

        public object Get(object target)
        {
            if (target == null) return null;
            return _field.GetValue(target);
        }

        public void Set(object target, object value)
        {
            _field.SetValue(target, value);
        }

        public Type MemberType { get { return _field.FieldType; } }

        public Type ReflectedType { get { return _field.ReflectedType; } }
    }
}